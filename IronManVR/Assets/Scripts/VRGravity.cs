using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRGravity : MonoBehaviour
{
  private CharacterController characterController;
  public float gravity = -9.81f; // Standard gravity value
  private float verticalVelocity = 0f;

  void Start()
  {
    characterController = GetComponent<CharacterController>();
    if (characterController == null)
    {
      Debug.LogError("VRGravity requires a CharacterController component on the same GameObject.");
      this.enabled = false;
    }
  }

  void Update()
  {
    if (characterController.isGrounded && verticalVelocity < 0)
    {
      // Reset vertical velocity if grounded
      verticalVelocity = -1f; // Small downward force helps stick to ground
    }
    else
    {
      // Apply gravity over time
      verticalVelocity += gravity * Time.deltaTime;
    }

    // Apply the calculated vertical movement
    Vector3 gravityMovement = new Vector3(0, verticalVelocity, 0);
    characterController.Move(gravityMovement * Time.deltaTime);
  }
}