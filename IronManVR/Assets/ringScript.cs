using UnityEngine;

public class RingTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Flew through ring!");
            Destroy(gameObject); // or play sound, effect, etc.
        }
    }
}
