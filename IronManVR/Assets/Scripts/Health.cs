using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float initialHealth = 100f;
    
    [SerializeField]
    private float currentHealth;
    
    void Start()
    {
        currentHealth = initialHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. New health: {currentHealth}");
        
        // if (currentHealth <= 0)
        // {
        //     gameObject.SetActive(false);
        // }
    }
}
