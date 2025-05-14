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

    public float punchDamageMultiplier = 1f;
    
    [Header("Extras")]
    
    [Tooltip("Prefab for visual effect on punch")]
    public GameObject punchEffectPrefab;
    
    private Vector3 previousPosition;
    [HideInInspector]
    public float currentVelocity;
    private float lastPunchTime = -1f;

    void Start()
    {
        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, previousPosition);
        currentVelocity = distance / Time.fixedDeltaTime;
        previousPosition = currentPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastPunchTime + punchCooldown) return;
        if (currentVelocity < punchVelocityThreshold) return;
        if (!other.gameObject.CompareTag(punchableTag)) return;
        
        
        
        Debug.Log($"PUNCHED: {other.gameObject.name} with velocity {currentVelocity}!");
        
        lastPunchTime = Time.time;

        var punchable = other.GetComponent<Punchable>();
        if (punchable == null) return;

        punchable.PlayHitSound();
        
        Rigidbody targetRb = other.GetComponent<Rigidbody>();
        if (targetRb != null && !targetRb.isKinematic)
        {
            Vector3 punchDirection = (other.transform.position - transform.position).normalized;
            float forceMagnitude = punchStrengthMultiplier * currentVelocity;
            targetRb.AddForce(punchDirection * forceMagnitude, ForceMode.Impulse);
        }
        
        Health health = other.GetComponentInChildren<Health>();
        if (health)
        {
            float damage = punchDamageMultiplier * currentVelocity;
            health.TakeDamage(damage);
            Debug.Log($"Dealt {damage} damage to {other.gameObject.name}!");
        }
        
        var effect = Instantiate(punchEffectPrefab, transform.position, Quaternion.identity);
        var particle = effect.GetComponent<ParticleSystem>();

        if (particle)
        {
            particle.Play();
        }
        
        Destroy(effect, 2f);
    }
}
