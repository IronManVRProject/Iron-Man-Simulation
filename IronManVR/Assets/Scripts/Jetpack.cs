using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;


public class Jetpack : MonoBehaviour
{
    public InputActionReference jetpackInputActionReference;
    public float jetpackForce;

    private Rigidbody _playerRb;
    private float _triggerValue;

    void Start()
    {
        _playerRb = FindObjectOfType<XROrigin>().GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        jetpackInputActionReference.action.Enable();
        jetpackInputActionReference.action.performed += SetTriggerValue;
    }

    private void OnDisable()
    {
        jetpackInputActionReference.action.Disable();
        jetpackInputActionReference.action.performed -= SetTriggerValue;
    }

 // Event function
    private void SetTriggerValue(InputAction.CallbackContext obj)
    {
        _triggerValue = obj.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        _playerRb.AddForce(-transform.forward * (_triggerValue * jetpackForce * Time.deltaTime), ForceMode.Force);
    }






}
