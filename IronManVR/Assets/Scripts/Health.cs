using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int initialHealth = 100;
    
    [SerializeField]
    private int currentHealth;
    
    void Start()
    {
        currentHealth = initialHealth;
    }

    public bool IsAlive()
    {
        return currentHealth <= 0;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. New health: {currentHealth}");
    }
}
