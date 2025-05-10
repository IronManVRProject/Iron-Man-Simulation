using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public InputActionReference switchSceneButton;
    
    public string[] sceneNames;
    private int currentSceneIndex = 0;

    private void Update()
    {
        if (switchSceneButton.action.WasPressedThisFrame())
        {
            SwitchToNextScene();
        }
    }

    public void SwitchToNextScene()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            currentSceneIndex = 0;
        }
        else
        {
            currentSceneIndex++;
        }
        
        StartCoroutine(LoadSceneAsync());
    }
    
    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentSceneIndex);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    public void SwitchToSceneIndex(int index)
    {
        if (index < 0 || index >= sceneNames.Length)
        {
            Debug.LogError("Invalid scene index: " + index);
            return;
        }
        
        currentSceneIndex = index;
        string sceneName = sceneNames[currentSceneIndex];
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
