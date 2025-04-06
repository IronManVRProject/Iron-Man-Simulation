using UnityEngine;

public class PunchInteractor : MonoBehaviour
{
    [Header("Punching Settings")]
    [Tooltip("Minimum speed the controller must reach to be considered a punch")]
    public float punchVelocityThreshold = 2.0f;
    
    [Tooltip("How long (in seconds) after a punch before another can be registered")]
    public float punchCooldown = 0.5f;
    
    [Tooltip("Tag for objects that can be punched")]
    public string punchableTag = "Punchable";

    [Tooltip("Multiplier for punch strength based on velocity")]
    public float punchStrengthMultiplier = 10f;
    
    [Header("Extras")]
    
    [Tooltip("Prefab for visual effect on punch")]
    public GameObject punchEffectPrefab;

    public Rigidbody controllerRigidbody;
    private Vector3 previousPosition;
    [HideInInspector]
    public float currentVelocity;
    private float lastPunchTime = -1f; // Initialize to allow punching immediately

    void Start()
    {
        if (controllerRigidbody == null)
        {
            controllerRigidbody = GetComponent<Rigidbody>();
            if (controllerRigidbody == null)
            {
                Debug.LogError("PunchDetector requires a Rigidbody component!", this);
                enabled = false;
                return;
            }
        }

        // Ensure Rigidbody is Kinematic (as it should be controlled by tracking)
        controllerRigidbody.isKinematic = true;

        previousPosition = transform.position;
    }

    void FixedUpdate() // Good for physics related calculations
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, previousPosition);
        currentVelocity = distance / Time.fixedDeltaTime;
        previousPosition = currentPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastPunchTime + punchCooldown)
        {
            return; // Still in cooldown
        }
        
        if (currentVelocity < punchVelocityThreshold)
        {
            return; // Not fast enough to register as a punch
        }
        
        if (other.gameObject.CompareTag(punchableTag))
        {
            Debug.Log($"PUNCHED: {other.gameObject.name} with velocity {currentVelocity}!");

            // --- Apply Punch Effects ---
            lastPunchTime = Time.time;

            var punchable = other.GetComponent<Punchable>();
            if (punchable != null)
            {
                punchable.PlayHitSound();
            }

            // Apply physics force (if the target has a non-kinematic Rigidbody)
            Rigidbody targetRb = other.GetComponent<Rigidbody>();
            if (targetRb != null && !targetRb.isKinematic)
            {
                Vector3 punchDirection = (other.transform.position - transform.position).normalized;
                float forceMagnitude = punchStrengthMultiplier * currentVelocity;
                targetRb.AddForce(punchDirection * forceMagnitude, ForceMode.Impulse);
            }

            // Trigger Haptics (Requires configuration in Input Actions)
            // controller.SendHapticImpulse(0.7f, 0.2f); // (intensity, duration) - Adjust values

            // Instantiate Visual Effect (e.g., particle system)
            var effect = Instantiate(punchEffectPrefab, transform.position, Quaternion.identity);
            var particle = effect.GetComponent<ParticleSystem>();

            if (particle)
            {
                particle.Play();
            }
            
            Destroy(effect, 2f);
        }
    }
}
