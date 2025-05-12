using System.Collections.Generic;
using Oculus.VoiceSDK.UX;
using TMPro;
using Unity.XR.CoreUtils.Datums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
  public static GameMenuManager Instance { get; private set; }

  public Transform head;
  public float spawnDistance = 2f;
  public GameObject menu;
  public InputActionReference menuAction;
  public TMP_Dropdown sceneDropdown;
  private List<string> sceneNamesInBuild = new List<string>();

  private bool isMenuActive = false;
  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);

      if (menu != null)
      {
        if (menu.transform.parent != this.transform)
        {
          DontDestroyOnLoad(menu);
        }
      }
      else
      {
        Debug.LogError("GameMenuManager: 'menu' GameObject is not assigned!");
      }

      FindAndAssignHead();
      PopulateSceneDropdown();
    }
    else if (Instance != this)
    {
      if (menu != null && menu != Instance.menu)
      {
        Destroy(menu);
      }
      Destroy(gameObject);
      return;
    }

    if (menu != null)
    {
      menu.SetActive(false);
      isMenuActive = false;
    }
  }
  void Start()
  {
    if (sceneDropdown != null)
    {
      sceneDropdown.onValueChanged.AddListener(delegate
      {
        DropdownItemSelected(sceneDropdown);
      });
    }
    else
    {
      Debug.LogWarning("GameMenuManager: Scene Dropdown is not assigned in the Inspector. Scene switching via dropdown will not work.");
    }
  }


  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoadedCallback;
    if (menuAction != null && menuAction.action != null)
    {
      menuAction.action.Enable();
      menuAction.action.performed += ToggleMenu;
    }
    else
    {
      Debug.LogError("GameMenuManager: 'menuAction' is not assigned or action is null!");
    }

    if (head == null)
    {
      FindAndAssignHead();
    }
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoadedCallback;
    if (menuAction != null && menuAction.action != null)
    {
      menuAction.action.performed -= ToggleMenu;
      menuAction.action.Disable();
    }
  }

  private void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
  {
    FindAndAssignHead();
    RefreshDropdownToCurrentScene();
  }
  private void ToggleMenu(InputAction.CallbackContext context)
  {
    if (menu == null) return;

    isMenuActive = !isMenuActive;
    menu.SetActive(isMenuActive);

    if (isMenuActive)
    {
      if (head == null)
      {
        Debug.LogError("GameMenuManager: 'head' Transform is null. Attempting to re-acquire.");
        FindAndAssignHead();
        if (head == null)
        {
          Debug.LogError("GameMenuManager: Failed to re-acquire 'head' Transform. Menu may not position correctly.");
          return;
        }
      }
      Vector3 spawnPosition = head.position + head.forward * spawnDistance;
      menu.transform.position = spawnPosition;
      menu.transform.LookAt(head.position);
      menu.transform.Rotate(0, 180, 0);
    }
  }

  void PopulateSceneDropdown()
  {
    if (sceneDropdown == null) return;

    sceneDropdown.ClearOptions();
    sceneNamesInBuild.Clear();

    List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

    int sceneCount = SceneManager.sceneCountInBuildSettings;

    for (int i = 0; i < sceneCount; i++)
    {
      string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
      string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

      if (string.IsNullOrEmpty(sceneName))
      {
        continue;
      }

      options.Add(new TMP_Dropdown.OptionData(sceneName));
      sceneNamesInBuild.Add(sceneName);
    }
    sceneDropdown.AddOptions(options);

    string currentSceneName = SceneManager.GetActiveScene().name;
    int currentSceneDropdownIndex = sceneNamesInBuild.IndexOf(currentSceneName);
    if (currentSceneDropdownIndex >= 0)
    {
      sceneDropdown.SetValueWithoutNotify(currentSceneDropdownIndex);
    }
  }
  void FindAndAssignHead()
  {
    if (Camera.main != null)
    {
      head = Camera.main.transform;
    }
    else
    {
      Debug.LogError("GameMenuManager: Main Camera not found. Head tracking for menu positioning may fail.");
    }
  }

  void DropdownItemSelected(TMP_Dropdown dropdown)
  {
    int selectedIndex = dropdown.value;
    if (selectedIndex >= 0 && selectedIndex < sceneNamesInBuild.Count)
    {
      string sceneToLoad = sceneNamesInBuild[selectedIndex];

      // hide menu
      if (menu != null)
      {
        isMenuActive = false;
        menu.SetActive(false);
      }

      SceneManager.LoadScene(sceneToLoad);

      // The menu's active state will persist as is.
      // If you want it to always hide after scene selection:
      // if (menu != null)
      // {
      //   isMenuActive = false;
      //   menu.SetActive(false);
      // }
    }
    else
    {
      Debug.LogError($"Invalid dropdown index: {selectedIndex}");
    }
  }

  public void RefreshDropdownToCurrentScene()
  {
    if (sceneDropdown == null || sceneNamesInBuild == null || sceneNamesInBuild.Count == 0) return;

    string currentSceneName = SceneManager.GetActiveScene().name;
    int currentSceneDropdownIndex = sceneNamesInBuild.IndexOf(currentSceneName);
    if (currentSceneDropdownIndex >= 0)
    {
      sceneDropdown.SetValueWithoutNotify(currentSceneDropdownIndex);
    }
  }

  void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }
    if (sceneDropdown != null)
    {
      sceneDropdown.onValueChanged.RemoveAllListeners();
    }
  }
}