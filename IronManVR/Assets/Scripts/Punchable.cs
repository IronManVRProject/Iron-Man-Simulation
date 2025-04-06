using UnityEngine;

public class Punchable : MonoBehaviour
{
    [Header("Punchable Settings")]
    [Tooltip("Sound that gets played when punched")]
    public AudioClip[] punchHitSoundClips;

    public void PlayHitSound()
    {
        var punchHitSound = punchHitSoundClips[Random.Range(0, punchHitSoundClips.Length)];
        
        AudioSource.PlayClipAtPoint(punchHitSound, transform.position);
    }
}
