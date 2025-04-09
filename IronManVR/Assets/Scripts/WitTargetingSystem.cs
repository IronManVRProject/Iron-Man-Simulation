using UnityEngine;
using Meta.WitAi.Json;
using System.Collections.Generic; // For List<>
using Oculus.Voice;
using UnityEngine.InputSystem;

public class WitTargetingSystem : MonoBehaviour
{

  [Tooltip("Assign the Enemy Indicator UI Prefab you created.")]
  public GameObject indicatorPrefab;

  [Tooltip("Optional: Parent transform for instantiated indicators.")]
  public Transform indicatorParent; // Assign the World Space Canvas if you want indicators grouped
  
  [Tooltip("Button to press to activate voice mode.")]
  public InputActionReference activateButton;

  public AudioClip buttonTestAudio;
  public GameObject player;
  
  private AppVoiceExperience appVoiceExperience;
  private Dictionary<GameObject, GameObject> activeIndicators = new Dictionary<GameObject, GameObject>(); // Key: Enemy, Value: Indicator

  void Start()
  {
    if (indicatorPrefab == null)
    {
      Debug.LogError("WitTargetingSystem: Indicator Prefab is not assigned!");
      this.enabled = false;
      return;
    }
    if (indicatorPrefab.GetComponent<FollowTarget3D>() == null)
    {
      Debug.LogError("WitTargetingSystem: Assigned Indicator Prefab is missing the 'FollowTarget3D' script!");
      this.enabled = false;
      return;
    }
    appVoiceExperience = GetComponent<AppVoiceExperience>();
    if (appVoiceExperience == null)
    {
      appVoiceExperience = FindFirstObjectByType<AppVoiceExperience>();
      if (appVoiceExperience == null)
      {
        Debug.LogError("WitTargetingSystem: Could not find AppVoiceExperience component on this GameObject or in the scene!");
        this.enabled = false;
        return;
      }
      else
      {
        Debug.LogWarning("WitTargetingSystem: Found AppVoiceExperience on a different GameObject. Consider putting this script on the same object.");
      }
    }

    if (appVoiceExperience.VoiceEvents != null)
    {
      // Subscribe using the VoiceEvents property found within AppVoiceExperience
      appVoiceExperience.VoiceEvents.OnResponse.AddListener(HandleWitResponse);
      appVoiceExperience.VoiceEvents.OnError.AddListener(HandleWitError);
      appVoiceExperience.VoiceEvents.OnStartListening.AddListener(HandleListenStart);
      appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(HandleListenStop);

      Debug.Log("WitTargetingSystem initialized using AppVoiceExperience.VoiceEvents. Ready.");
    }
    else
    {
      // Fallback: Maybe events are directly on the AppVoiceExperience object itself? (Less likely)
      // Check the available members using IntelliSense (type 'appVoiceExperience.' and see options)
      // Example structure (PSEUDOCODE - check actual members):
      // if (appVoiceExperience.OnResponse != null) { appVoiceExperience.OnResponse.AddListener(HandleWitResponse); }

      Debug.LogError("WitTargetingSystem: Could not find VoiceEvents property or suitable events directly on AppVoiceExperience. Check SDK structure/version or component setup.");
      this.enabled = false;
      return;
    }
  }

  // --- Example Activation (Keyboard) ---
  void Update()
  {

    if (activateButton.action.triggered)
    {
      AudioSource.PlayClipAtPoint(buttonTestAudio, player.transform.position);
      
      Debug.Log("SimpleWit: Voice key pressed. Toggling voice activation.");
      if (appVoiceExperience != null)
      {
        if (!appVoiceExperience.Active)
        {
          appVoiceExperience.Activate();
        }
        else
        {
          appVoiceExperience.Deactivate();
        }
      }
      else
      {
        Debug.LogError("SimpleWit: Cannot activate, AppVoiceExperience is null.");
      }
    }

  }

  private void HandleWitResponse(WitResponseNode response)
  {
    if (response == null) return; // Ignore null response

    // Debug.Log($"Wit Response: {response.ToString()}"); // Log full response if needed

    if (response["intents"] == null || response["intents"].Count == 0) return; // No intent found

    string intentName = response["intents"][0]["name"].Value;
    float intentConfidence = response["intents"][0]["confidence"].AsFloat;

    Debug.Log($"Targeting System received Intent: {intentName} (Confidence: {intentConfidence:P})");

    // --- Handle Targeting Intent ---
    if (intentName == "target_enemy" && intentConfidence > 0.7f) // Confidence threshold
    {
      Debug.Log("Executing: Highlight Enemies");
      HighlightEnemies();
    }
    // --- Handle Disengage Intent ---
    else if (intentName == "disengage_target" && intentConfidence > 0.7f)
    {
      Debug.Log("Executing: Clear Highlights");
      ClearHighlights();
    }
  }

  // --- Core Logic Functions ---

  void HighlightEnemies()
  {
    // Clear existing highlights before adding new ones
    ClearHighlights();

    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    Debug.Log($"Found {enemies.Length} enemies.");

    if (enemies.Length == 0)
    {
      return;
    }

    foreach (GameObject enemy in enemies)
    {
      if (indicatorPrefab != null)
      {
        // Instantiate the indicator prefab
        GameObject indicatorInstance = Instantiate(indicatorPrefab, indicatorParent); // Instantiate under parent if assigned

        // Get the FollowTarget3D script and set its target
        FollowTarget3D followScript = indicatorInstance.GetComponent<FollowTarget3D>();
        if (followScript != null)
        {
          followScript.target = enemy.transform; // Tell the indicator who to follow
          Debug.Log($"Assigning indicator to follow enemy: {enemy.name}");
          // Keep track of the indicator instance associated with this enemy
          activeIndicators.Add(enemy, indicatorInstance);
        }
        else
        {
          Debug.LogError("Instantiated indicator prefab is missing FollowTarget3D script!", indicatorInstance);
          Destroy(indicatorInstance); // Clean up broken instance
        }
      }
    }
  }

  void ClearHighlights()
  {
    Debug.Log($"Clearing {activeIndicators.Count} highlights.");
    // Loop through the values (indicator GameObjects) in the dictionary and destroy them
    foreach (GameObject indicator in activeIndicators.Values)
    {
      if (indicator != null) // Check if it hasn't already been destroyed
      {
        Destroy(indicator);
      }
    }
    // Clear the dictionary
    activeIndicators.Clear();
  }

  // --- Cleanup ---
  void OnDestroy()
  {
    if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null)
    {
      appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(HandleWitResponse);
      appVoiceExperience.VoiceEvents.OnError.RemoveListener(HandleWitError);
      appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(HandleListenStart);
      appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(HandleListenStop);
    }
    // Ensure highlights are cleared when the system is destroyed
    ClearHighlights();
  }

  private void HandleWitError(string status, string error)
  {
    Debug.LogError($"Wit Error: Status '{status}', Message '{error}'");
  }

  private void HandleListenStart()
  {
    Debug.Log("Targeting System Listening...");
  }

  private void HandleListenStop()
  {
    Debug.Log("Targeting System Stopped Listening.");
  }
}