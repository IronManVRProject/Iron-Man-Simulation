using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RepulsorModeController : MonoBehaviour
{
    // Modes the repulsor can fire in
    public enum RepulsorMode { Single, Burst, Continuous }

    [Header("Repulsor Modes")]
    [SerializeField] private RepulsorMode currentMode = RepulsorMode.Single;

    [Header("Mode Settings")]
    [SerializeField] private float singleShotPower = 100f;        // Power for single shot
    [SerializeField] private float burstShotPower = 5f;          // Power per shot in burst mode
    [SerializeField] private int burstCount = 10;                 // Number of shots in burst mode
    [SerializeField] private float burstDelay = 0.25f;            // Delay between shots in burst mode
    [SerializeField] private float continuousShotPower = 8f;     // Power during continuous fire
    [SerializeField] private float continuousDrainRate = 0.05f;  // Energy drain per second

    [Header("Energy System")]
    [SerializeField] private float maxEnergy = 1f;               // Maximum energy available
    [SerializeField] private float energyRechargeRate = 0.2f;    // Recharge rate when not firing
    private float currentEnergy = 1f;                            // Current energy level

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem modeChangeEffect;    // Visual effect on mode switch
    [SerializeField] private ParticleSystem impactEffectPrefab;  // Particle effect on hit
    [SerializeField] private Color singleModeColor = Color.cyan; // Beam color for single mode
    [SerializeField] private Color burstModeColor = new Color(1f, 0.5f, 0f); // Orange for burst
    [SerializeField] private Color continuousModeColor = Color.red; // Red for continuous

    [Header("Audio")]
    [SerializeField] private AudioClip modeChangeSound;          // Sound on mode change
    [SerializeField] private AudioClip singleShotSound;          // Sound for single shot
    [SerializeField] private AudioClip burstShotSound;           // Sound for burst mode
    [SerializeField] private AudioClip continuousShotSound;      // Sound for continuous mode
    [SerializeField] private AudioSource audioSource;            // Audio source to play sounds

    [Header("Input")]
    [SerializeField] private InputActionReference cycleModesAction; // Input to cycle modes

    [Header("Haptics")]
    [SerializeField] private XRController hapticController;       // XR controller for haptic feedback
    [SerializeField] private float hapticAmplitude = 0.5f;       // Strength of vibration
    [SerializeField] private float hapticDuration = 0.1f;        // Duration of vibration

    // Internal references and state
    private RepulsorController repulsorController;               // Access to main repulsor logic
    private LineRenderer beamRenderer;                           // Line renderer for beam
    private float originalBeamWidth;                             // Cached beam width
    private Color originalBeamColor;                             // Cached beam color
    private bool firingContinuous = false;                       // Tracks continuous fire
    private Coroutine burstCoroutine;                            // Tracks burst coroutine
    private Material repulsorMaterial;                           // Material to update emission color
    private Queue<ParticleSystem> impactEffectPool = new Queue<ParticleSystem>(); // Pool for effects

    // Optional hooks
    public event System.Action<RepulsorMode> OnModeChanged;      // Event on mode switch
    public event System.Action OnEnergyDepleted;                 // Event when energy runs out

    private void Awake()
    {
        // Get reference to main repulsor logic
        repulsorController = GetComponent<RepulsorController>();

        // Ensure we have an audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Get the beam material for color control
        Renderer sphereRenderer = transform.GetComponentInChildren<Renderer>();
        if (sphereRenderer != null)
            repulsorMaterial = sphereRenderer.material;

        // Set initial visuals
        UpdateRepulsorVisuals();
    }

    private void OnEnable()
    {
        // Register input and repulsor events
        if (cycleModesAction != null)
            cycleModesAction.action.performed += OnCycleModes;

        if (repulsorController != null)
        {
            repulsorController.OnRepulsorActivated += HandleRepulsorActivated;
            repulsorController.OnRepulsorDeactivated += HandleRepulsorDeactivated;
        }
    }

    private void OnDisable()
    {
        // Unregister input and repulsor events
        if (cycleModesAction != null)
            cycleModesAction.action.performed -= OnCycleModes;

        if (repulsorController != null)
        {
            repulsorController.OnRepulsorActivated -= HandleRepulsorActivated;
            repulsorController.OnRepulsorDeactivated -= HandleRepulsorDeactivated;
        }

        StopContinuousFiring();
    }

    private void Update()
    {
        // Recharge energy over time if not firing
        if (!firingContinuous && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRechargeRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }
    }

    private void OnCycleModes(InputAction.CallbackContext context)
    {
        CycleRepulsorMode();
    }

    public void CycleRepulsorMode()
    {
        // Stop any active firing
        StopContinuousFiring();
        if (burstCoroutine != null)
            StopCoroutine(burstCoroutine);

        // Go to the next mode in the enum
        currentMode = (RepulsorMode)(((int)currentMode + 1) % System.Enum.GetValues(typeof(RepulsorMode)).Length);

        // Update visuals
        UpdateRepulsorVisuals();

        // Stop any current sounds before playing new one
        StopCurrentAudio();

        // Play visual and audio effects
        if (modeChangeEffect != null)
            modeChangeEffect.Play();
        if (modeChangeSound != null)
            audioSource.PlayOneShot(modeChangeSound);

        // Fire event for UI or feedback
        OnModeChanged?.Invoke(currentMode);
    }

    private void UpdateRepulsorVisuals()
    {
        if (repulsorMaterial == null) return;

        // Get the color based on current mode
        Color modeColor = currentMode switch
        {
            RepulsorMode.Single => singleModeColor,
            RepulsorMode.Burst => burstModeColor,
            RepulsorMode.Continuous => continuousModeColor,
            _ => singleModeColor
        };

        // Apply to material and beam color
        repulsorMaterial.SetColor("_EmissionColor", modeColor * 2f);
        if (repulsorController != null)
            repulsorController.SetRepulsorColor(modeColor);
    }

    private void HandleRepulsorActivated(GameObject beam)
    {
        // Cache the beam appearance
        if (beam == null) return;
            beamRenderer = beam.GetComponent<LineRenderer>();
            
        if (beamRenderer != null)
        {
            originalBeamWidth = beamRenderer.startWidth;
            originalBeamColor = beamRenderer.startColor;
        }

        // Fire depending on selected mode
        switch (currentMode)
        {
            case RepulsorMode.Single:
                FireSingleShot();
                break;
            case RepulsorMode.Burst:
                if (burstCoroutine != null)
                    StopCoroutine(burstCoroutine);
                burstCoroutine = StartCoroutine(FireBurst());
                break;
            case RepulsorMode.Continuous:
                StartContinuousFiring();
                break;
        }
    }

    private void HandleRepulsorDeactivated()
    {
        // Stop any firing or effects when disabled
        StopContinuousFiring();
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
    }

    private void FireSingleShot()
    {
        // Stop any overlapping audio
        StopCurrentAudio();

        // Fire one shot
        repulsorController.SetRepulsorPower(singleShotPower);
        if (singleShotSound != null)
            audioSource.PlayOneShot(singleShotSound);
        StartCoroutine(SingleShotPulse());
        TriggerBeamHitWithEffect();
        TriggerHapticPulse();
    }

    private IEnumerator SingleShotPulse()
    {
        // Briefly show beam
        if (beamRenderer != null)
        {
            beamRenderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
            beamRenderer.enabled = false;
        }
    }

    private IEnumerator FireBurst()
    {
        // Stop any overlapping audio
        StopCurrentAudio();

        // Play burst sound and fire multiple times
        if (burstShotSound != null)
            audioSource.PlayOneShot(burstShotSound);

        repulsorController.SetRepulsorPower(burstShotPower);

        for (int i = 0; i < burstCount; i++)
        {
            if (beamRenderer != null)
            {
                beamRenderer.enabled = true;
                TriggerBeamHitWithEffect();
                TriggerHapticPulse();
                yield return new WaitForSeconds(0.2f);
                beamRenderer.enabled = false;
            }

            yield return new WaitForSeconds(burstDelay);
        }

        burstCoroutine = null;
    }

    private void StartContinuousFiring()
    {
        // Don't restart if already firing
        if (firingContinuous) return;

        // Don't start if no energy
        if (currentEnergy <= 0f)
        {
            OnEnergyDepleted?.Invoke();
            return;
        }

        firingContinuous = true;
        repulsorController.SetRepulsorPower(continuousShotPower);

        // Play continuous sound in loop
        StopCurrentAudio();
        if (continuousShotSound != null)
        {
            audioSource.clip = continuousShotSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(ContinuousBeamEffect());
    }

    private void StopContinuousFiring()
    {
        if (!firingContinuous) return;

        firingContinuous = false;

        // Stop looping audio
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
        }

        // Reset beam appearance
        if (beamRenderer != null)
        {
            beamRenderer.startWidth = originalBeamWidth;
            beamRenderer.endWidth = originalBeamWidth * 0.5f;
            beamRenderer.startColor = originalBeamColor;
            beamRenderer.endColor = new Color(originalBeamColor.r, originalBeamColor.g, originalBeamColor.b, 0.2f);
        }
    }

    private IEnumerator ContinuousBeamEffect()
    {
        float time = 0f;

        while (firingContinuous && beamRenderer != null)
        {
            // Drain energy while firing
            currentEnergy -= continuousDrainRate * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                StopContinuousFiring();
                OnEnergyDepleted?.Invoke();
                yield break;
            }

            time += Time.deltaTime;

            // Animate beam size and color
            float growth = Mathf.Clamp01(time / 2f);
            float pulseWidth = originalBeamWidth * (1 + 0.2f * Mathf.Sin(time * 15)) * (1 + growth);
            beamRenderer.startWidth = pulseWidth;
            beamRenderer.endWidth = pulseWidth * 0.3f;

            Color intensifiedColor = continuousModeColor * (1.5f + 0.5f * Mathf.Sin(time * 10));
            beamRenderer.startColor = intensifiedColor;
            beamRenderer.endColor = new Color(intensifiedColor.r, intensifiedColor.g, intensifiedColor.b, 0.3f);

            // Apply hits and haptics every few frames
            if (Time.frameCount % 3 == 0)
            {
                TriggerBeamHitWithEffect();
                TriggerHapticPulse();
            }

            yield return null;
        }
    }

    private void TriggerBeamHitWithEffect()
    {
        // Try get hit info and spawn effect
        if (repulsorController.TryGetLastHit(out RaycastHit hit))
        {
            repulsorController.TriggerBeamHit();

            if (impactEffectPrefab != null)
            {
                ParticleSystem impact = GetImpactEffect();
                impact.transform.position = hit.point;
                impact.transform.rotation = Quaternion.LookRotation(hit.normal);
                impact.Play();
                StartCoroutine(ReleaseImpactEffectAfter(impact, 2f));
            }
        }
        else
        {
            repulsorController.TriggerBeamHit();
        }
    }

    private ParticleSystem GetImpactEffect()
    {
        // Get from pool or create new
        if (impactEffectPool.Count > 0)
        {
            var pooled = impactEffectPool.Dequeue();
            pooled.gameObject.SetActive(true);
            return pooled;
        }

        return Instantiate(impactEffectPrefab);
    }

    private IEnumerator ReleaseImpactEffectAfter(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.Stop();
        ps.gameObject.SetActive(false);
        impactEffectPool.Enqueue(ps);
    }

    private void TriggerHapticPulse()
    {
        // Vibrate the controller
        if (hapticController != null)
    {
        hapticController.SendHapticImpulse(hapticAmplitude, hapticDuration);
    }
}

    private void StopCurrentAudio()
    {
        // Stop whatever audio is playing
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}



