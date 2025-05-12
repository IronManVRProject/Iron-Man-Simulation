using UnityEngine;

public class CopyCameraTransform : MonoBehaviour
{
  [Tooltip("Assign the main VR Camera here.")]
  public Camera targetCamera; // Assign Main VR Camera in Inspector

  void LateUpdate()
  {
    if (targetCamera != null)
    {
      // Copy position and rotation exactly each frame
      transform.position = targetCamera.transform.position;
      transform.rotation = targetCamera.transform.rotation;
    }
    else
    {
      Debug.LogWarning("CopyCameraTransform: Target Camera is not assigned!", this.gameObject);
    }
  }
}