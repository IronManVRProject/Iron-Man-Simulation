using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Events;
using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine.InputSystem;

public class WitTargetingSystem : MonoBehaviour
{
  [Tooltip("Input Action Reference for left hand (for hold-to-listen).")]
  public InputActionReference leftHandInputAction;

  [Tooltip("Assign the Enemy Indicator UI Prefab you created.")]
  public GameObject indicatorPrefab;

  [Tooltip("Optional: Parent transform for instantiated indicators.")]
  public Transform indicatorParent;

  [Tooltip("Sound to play when activating listening.")]
  public AudioClip activationSound;
  [Tooltip("Optional: GameObject to show when listening.")]
  public GameObject listeningIndicator;

  private AudioSource audioSource;
  private AppVoiceExperience appVoiceExperience;
  private Dictionary<GameObject, GameObject> activeIndicators = new Dictionary<GameObject, GameObject>();

  void Awake()
  {

    audioSource = gameObject.GetComponent<AudioSource>();
    if (audioSource == null)
    {
      audioSource = gameObject.AddComponent<AudioSource>();
    }
    audioSource.playOnAwake = false;

    appVoiceExperience = GetComponent<AppVoiceExperience>();
    if (appVoiceExperience == null)
    {
      appVoiceExperience = FindFirstObjectByType<AppVoiceExperience>();
      if (appVoiceExperience == null)
      {
        Debug.LogError("WitTargetingSystem: Could not find AppVoiceExperience component on this GameObject or in the scene! Disabling script.", this);
        this.enabled = false;
        return;
      }
      else
      {
        Debug.LogWarning("WitTargetingSystem: Found AppVoiceExperience on a different GameObject. Consider putting this script on the same object for clarity.", this);
      }
    }

    if (indicatorPrefab == null)
    {
      Debug.LogError("WitTargetingSystem: Indicator Prefab is not assigned! Disabling script.", this);
      this.enabled = false;
      return;
    }
    if (indicatorPrefab.GetComponent<FollowTarget3D>() == null)
    {
      Debug.LogError("WitTargetingSystem: Assigned Indicator Prefab is missing the 'FollowTarget3D' script! Disabling script.", this);
      this.enabled = false;
      return;
    }

    if (leftHandInputAction == null)
    {
      Debug.LogError("WitTargetingSystem: Left Hand Input Action Reference is not assigned! Voice activation via input will not work.", this);
    }
  }

  void Start()
  {
    foreach (var device in Microphone.devices)
    {
      Debug.Log("Mic device found: " + device, this);
    }
    if (Microphone.devices.Length == 0)
    {
      Debug.LogWarning("No microphone devices found in Unity.", this);
    }

    if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null)
    {
      appVoiceExperience.VoiceEvents.OnResponse.AddListener(HandleWitResponse);
      appVoiceExperience.VoiceEvents.OnError.AddListener(HandleWitError);
      appVoiceExperience.VoiceEvents.OnStartListening.AddListener(HandleListenStart);
      appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(HandleListenStop);
      Debug.Log("WitTargetingSystem initialized and subscribed to AppVoiceExperience.VoiceEvents. Ready.", this);
    }
    else if (appVoiceExperience != null)
    {
      Debug.LogError("WitTargetingSystem: AppVoiceExperience found, but its VoiceEvents property is null. Check SDK structure/version or component setup. Disabling script.", this);
      this.enabled = false;
    }
  }

  private void OnEnable()
  {
    if (leftHandInputAction != null && leftHandInputAction.action != null)
    {
      leftHandInputAction.action.Enable();
      leftHandInputAction.action.started += OnLeftHandActionStarted;
      leftHandInputAction.action.canceled += OnLeftHandActionCanceled;
      Debug.Log("Left Hand Input Action enabled and events (started, canceled) subscribed for hold-to-listen.", this);
    }
    else if (leftHandInputAction == null)
    {
      Debug.LogWarning("WitTargetingSystem: Left Hand Input Action Reference is not assigned in OnEnable. Hold-to-listen will not function.", this);
    }
    else
    {
      Debug.LogError("WitTargetingSystem: Left Hand Input Action Reference is assigned, but its 'action' property is null. Check your Input Action Asset setup.", this);
    }
  }

  private void OnDisable()
  {
    if (leftHandInputAction != null && leftHandInputAction.action != null)
    {
      leftHandInputAction.action.started -= OnLeftHandActionStarted;
      leftHandInputAction.action.canceled -= OnLeftHandActionCanceled;
      Debug.Log("Left Hand Input Action events (started, canceled) unsubscribed.", this);
    }
  }

  private void OnLeftHandActionStarted(InputAction.CallbackContext context)
  {
    Debug.Log("WitTargetingSystem: Left hand action STARTED (button pressed).", this);
    if (appVoiceExperience != null)
    {
      if (!appVoiceExperience.Active)
      {
        Debug.Log("WitTargetingSystem: Activating AppVoiceExperience (Hold-to-listen).", this);
        appVoiceExperience.Activate();
        if (listeningIndicator != null)
        {
          listeningIndicator.SetActive(true);
          Debug.Log("WitTargetingSystem: Listening indicator activated.", this);
        }
        else
        {
          Debug.LogWarning("WitTargetingSystem: Listening indicator is null. Cannot activate.", this);
        }
      }
      else
      {
        Debug.LogWarning("WitTargetingSystem: AppVoiceExperience was already active when action started. This might indicate a stuck state or overlapping activations.", this);
      }
    }
    else
    {
      Debug.LogError("WitTargetingSystem: Cannot activate, AppVoiceExperience is null.", this);
    }
  }

  private void OnLeftHandActionCanceled(InputAction.CallbackContext context)
  {
    Debug.Log("WitTargetingSystem: Left hand action CANCELED (button released).", this);
    if (appVoiceExperience != null)
    {
      if (appVoiceExperience.Active)
      {
        Debug.Log("WitTargetingSystem: Deactivating AppVoiceExperience (Hold-to-listen).", this);
        appVoiceExperience.Deactivate();
      }
      else
      {
        Debug.LogWarning("WitTargetingSystem: AppVoiceExperience was not active when action canceled. No deactivation needed.", this);
      }
    }
    else
    {
      Debug.LogError("WitTargetingSystem: Cannot deactivate, AppVoiceExperience is null.", this);
    }
  }

  private void HandleWitResponse(WitResponseNode response)
  {
    if (response == null)
    {
      Debug.LogWarning("WitTargetingSystem: Received null WitResponseNode.", this);
      return;
    }

    var intentsArray = response["intents"];
    if (intentsArray == null || intentsArray.Count == 0)
    {
      Debug.Log("WitTargetingSystem: No intents found in the response.", this);
      return;
    }

    var firstIntent = intentsArray[0];
    if (firstIntent == null || firstIntent["name"] == null || firstIntent["confidence"] == null)
    {
      Debug.LogWarning("WitTargetingSystem: First intent is malformed or missing name/confidence.", this);
      return;
    }

    string intentName = firstIntent["name"].Value;
    float intentConfidence = firstIntent["confidence"].AsFloat;

    Debug.Log($"Targeting System received Intent: {intentName} (Confidence: {intentConfidence:P})", this);

    if (intentName == "target_enemy" && intentConfidence > 0.7f)
    {
      Debug.Log("Executing: Highlight Enemies", this);
      HighlightEnemies();
    }
    else if (intentName == "disengage_target" && intentConfidence > 0.7f)
    {
      Debug.Log("Executing: Clear Highlights", this);
      ClearHighlights();
    }
    else
    {
      Debug.Log($"Targeting System: Intent '{intentName}' not handled or confidence too low ({intentConfidence:P}).", this);
    }
  }

  void HighlightEnemies()
  {
    ClearHighlights();
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    Debug.Log($"Found {enemies.Length} enemies.", this);

    if (enemies.Length == 0)
    {
      Debug.Log("No enemies with tag 'Enemy' found to highlight.", this);
      return;
    }

    foreach (GameObject enemy in enemies)
    {
      if (indicatorPrefab != null)
      {
        GameObject indicatorInstance = Instantiate(indicatorPrefab, enemy.transform.position, Quaternion.identity, indicatorParent);
        FollowTarget3D followScript = indicatorInstance.GetComponent<FollowTarget3D>();
        if (followScript != null)
        {
          followScript.target = enemy.transform;
          Debug.Log($"Assigning indicator to follow enemy: {enemy.name}", this);
          if (!activeIndicators.ContainsKey(enemy))
          {
            activeIndicators.Add(enemy, indicatorInstance);
          }
          else
          {
            Debug.LogWarning($"Enemy {enemy.name} already had an indicator. This should not happen if ClearHighlights was called.", this);
            Destroy(indicatorInstance);
          }
        }
        else
        {
          Debug.LogError($"Instantiated indicator prefab '{indicatorPrefab.name}' is missing FollowTarget3D script!", indicatorInstance);
          Destroy(indicatorInstance);
        }
      }
    }
  }

  void ClearHighlights()
  {
    if (activeIndicators.Count == 0) return;

    Debug.Log($"Clearing {activeIndicators.Count} highlights.", this);
    foreach (GameObject indicator in activeIndicators.Values)
    {
      if (indicator != null)
      {
        Destroy(indicator);
      }
    }
    activeIndicators.Clear();
  }

  void OnDestroy()
  {
    if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null)
    {
      appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(HandleWitResponse);
      appVoiceExperience.VoiceEvents.OnError.RemoveListener(HandleWitError);
      appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(HandleListenStart);
      appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(HandleListenStop);
      Debug.Log("WitTargetingSystem: Unsubscribed from AppVoiceExperience.VoiceEvents on destroy.", this);
    }

    if (leftHandInputAction != null && leftHandInputAction.action != null && leftHandInputAction.action.enabled)
    {
      leftHandInputAction.action.started -= OnLeftHandActionStarted;
      leftHandInputAction.action.canceled -= OnLeftHandActionCanceled;
      Debug.Log("WitTargetingSystem: Ensured Left Hand Input Action events unsubscribed on destroy.", this);
    }

    ClearHighlights();
    Debug.Log("WitTargetingSystem: Highlights cleared on destroy.", this);
  }

  private void HandleWitError(string status, string error)
  {
    Debug.LogError($"Wit Error: Status '{status}', Message '{error}'", this);
  }

  private void HandleListenStart()
  {
    Debug.Log("Targeting System Listening...", this);
    if (activationSound != null && audioSource != null)
    {
      if (!audioSource.isPlaying)
      {
        Debug.Log("Playing activation sound.", this);
        audioSource.PlayOneShot(activationSound);
      }

      if (listeningIndicator != null)
      {
        listeningIndicator.SetActive(true);
        Debug.Log("WitTargetingSystem: Listening indicator activated.", this);
      }
    }
    else if (activationSound == null)
    {
    }
    else
    {
      Debug.LogWarning("AudioSource is missing. Cannot play activation sound.", this);
    }
  }

  private void HandleListenStop()
  {
    Debug.Log("Targeting System Stopped Listening.", this);
    if (listeningIndicator != null)
    {
      listeningIndicator.SetActive(false);
      Debug.Log("WitTargetingSystem: Listening indicator deactivated.", this);
    }
  }
}
