using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles damage, effects, and destruction for a repulsor-targetable enemy (like Thanos).
/// Now supports correct flashing during animations and multiple hit sounds.
/// </summary>
public class RepulsorTarget : MonoBehaviour, IRepulsorTarget
{
    [Header("Target Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private bool isDestructible = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private Material hitMaterial; // Material used for blood flash
    [SerializeField] private float hitFlashDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private List<AudioClip> hitSounds = new List<AudioClip>(); // Randomized hit sounds
    [SerializeField] private AudioClip destructionSound;

    [Header("Animation (Animator Required)")]
    [SerializeField] private Animator animator; // Animator with Hit and Death triggers

    // Cached components
    private SkinnedMeshRenderer[] meshRenderers;
    private Dictionary<SkinnedMeshRenderer, Material[]> originalMaterials;
    private AudioSource audioSource;
    private bool isDestroyed = false;
    private Transform playerTransform;
    private Health health;
    
    // Events
    public delegate void TargetHitEvent(int scoreValue);
    public static event TargetHitEvent OnTargetHit;

    public delegate void TargetDestroyedEvent(int scoreValue);
    public static event TargetDestroyedEvent OnTargetDestroyed;

    private void Awake()
    {
        // Cache all renderers and their original materials
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        originalMaterials = new Dictionary<SkinnedMeshRenderer, Material[]>();

        foreach (var smr in meshRenderers)
        {
            // Use sharedMaterials to avoid Unity instancing issues
            originalMaterials[smr] = smr.sharedMaterials;
        }

        // Add AudioSource if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hitSounds.Count > 0 || destructionSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        health = GetComponent<Health>();

        if (!health)
        {
            health = GetComponentInChildren<Health>();
            
            if (health)
                Debug.Log($"Found health component on child object {health.name}.");
            else
                Debug.Log($"No health component found on {name} or its children.");
        }
    }

    private void Start()
    {
        GameObject player = GameObject.Find("XR Origin (XR Rig)"); // This matches your exact object name
        if (player != null)
        {
            playerTransform = player.transform;
            FacePlayer();
        }
    }

    private void FacePlayer()
        {
            if (playerTransform == null) return;

        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    public void OnHitByRepulsor(float power, Vector3 hitPoint, Vector3 direction)
    {
        if (isDestroyed) return;

        // Apply damage
        if (isDestructible)
            health.TakeDamage(Mathf.RoundToInt(power));

        // Trigger animation
        if (animator != null)
            animator.SetTrigger("Hit");

        // Visual flash
        StartCoroutine(FlashHitEffect());

        // Spawn hit VFX
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(-direction));
            
            effect.transform.SetParent(transform);
            
            Destroy(effect, 2f);
        }

        // Random hit sound
        if (audioSource != null && hitSounds.Count > 0)
        {
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Count)];
            audioSource.clip = randomClip;
            audioSource.Play();
        }

        // Notify score event
        OnTargetHit?.Invoke(scoreValue);

        // If dead, destroy
        if (isDestructible && healthPoints <= 0 && !isDestroyed)
        
        // Notify of hit for scoring
        if (OnTargetHit != null)
            OnTargetHit(scoreValue);
            
        // Check if target is destroyed
        if (isDestructible && health && health.IsAlive())
        {
            DestroyTarget();
    }

    private void DestroyTarget()
    {
        isDestroyed = true;

        // Trigger death animation
        if (animator != null)
            animator.SetTrigger("Death");

        // Spawn destruction VFX
        if (destructionEffectPrefab != null)
        {
            GameObject effect = Instantiate(destructionEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }

        // Play death sound
        if (audioSource != null && destructionSound != null)
        {
            audioSource.clip = destructionSound;
            audioSource.Play();
        }

        // Notify event
        OnTargetDestroyed?.Invoke(scoreValue * 2);

        // Disable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
            col.enabled = false;

        float destroyDelay = (audioSource != null && destructionSound != null)
            ? destructionSound.length
            : 0.5f;

        StartCoroutine(DestroyAfterDelay(destroyDelay));
    }

    private IEnumerator FlashHitEffect()
    {
    if (meshRenderers == null)
        yield break;

    Color flashColor = Color.red; // Flash color
    float flashIntensity = 1.5f;  // Boost brightness

    List<Material> modifiedMaterials = new List<Material>();

    // Loop through each renderer
    foreach (var smr in meshRenderers)
    {
        foreach (var mat in smr.materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color original = mat.color;
                mat.color = flashColor * flashIntensity;
                modifiedMaterials.Add(mat);

                // Store original color in coroutine below
                StartCoroutine(ResetMaterialColor(mat, original, hitFlashDuration));
            }
        }
    }

    yield return null;
}

// Restores the material color after the flash
private IEnumerator ResetMaterialColor(Material mat, Color originalColor, float delay)
{
    yield return new WaitForSeconds(delay);
    if (mat != null)
        mat.color = originalColor;
}


    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}



