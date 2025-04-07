using UnityEngine;

public class FollowTarget3D : MonoBehaviour
{
  public Transform target; // Assign the enemy transform via code later
  public Vector3 offset = new Vector3(0, 1.5f, 0); // Adjust Y value to position above enemy pivot
  public Camera mainCamera; // Should be found automatically if tagged
  public bool billboard = true; // Make it always face the camera

  void Start()
  {
    // Attempt to find the main camera by tag if not assigned
    if (mainCamera == null)
    {
      mainCamera = Camera.main;
    }

    // Log warnings if setup seems incomplete
    if (target == null)
    {
      // This is expected initially, as target is set after instantiation
      // Debug.LogWarning("FollowTarget3D: Target is initially null. Should be assigned by instantiating script.", this.gameObject);
    }
    if (mainCamera == null)
    {
      Debug.LogWarning("FollowTarget3D: Camera not found (is your main VR camera tagged 'MainCamera'?)", this.gameObject);
    }
  }

  void LateUpdate()
  {
    // Only run if we have a target to follow
    if (target != null)
    {
      // Update our position to the target's position plus the offset
      transform.position = target.position + offset;

      // If billboarding is enabled and we have a camera reference
      if (billboard && mainCamera != null)
      {
        // // Make the text face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
      }
    }
    else
    {
      // If the target gets destroyed (e.g. enemy killed), destroy this indicator too.
      // Check prevents error if script starts without target assigned yet.
      if (Time.time > 0.1f) // Avoid destroying immediately on first frame before target might be assigned
      {
        Destroy(gameObject);
      }
    }
  }
}