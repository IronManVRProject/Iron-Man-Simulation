using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;


public class Jetpack : MonoBehaviour
{
    public InputActionReference jetpackInputActionReference;
    public float jetpackForce;
    public AudioSource audioSource;
    public ParticleSystem particles;

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
        jetpackInputActionReference.action.canceled += ResetTriggerValue;
    }
    
    private void OnDisable()
    {
        jetpackInputActionReference.action.Disable();
        jetpackInputActionReference.action.performed -= SetTriggerValue;
        jetpackInputActionReference.action.canceled -= ResetTriggerValue;
    }

    // Event function
    private void SetTriggerValue(InputAction.CallbackContext obj)
    {
        _triggerValue = obj.ReadValue<float>();
    }
    
    void ResetTriggerValue(InputAction.CallbackContext obj)
    {
        _triggerValue = 0;
    }

    private void FixedUpdate()
    {
        Debug.Log(_triggerValue);
        
		if (_triggerValue > 0.1)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            };
            if (!particles.isPlaying)
            {
                particles.Play();
            }
            
            _playerRb.AddForce(-transform.forward * (jetpackForce * Time.deltaTime), ForceMode.Force);
        }
        
        if (_triggerValue <= 0.6) 
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (particles.isPlaying)
            {
                particles.Stop();
            }
        }
    }






}
