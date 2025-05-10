using TMPro;
using UnityEngine;

public class PunchInteractorDebugText : MonoBehaviour
{
    public TextMeshPro pokeStrengthText;
    
    public PunchInteractor punchInteractor;
    
    private float maxVelocity = 0.0f;
    
    
    private void Start()
    {
        if (pokeStrengthText == null)
        {
            Debug.LogError("TextMeshPro component not found in children.");
            gameObject.SetActive(false);
        }
        
        if (punchInteractor == null)
        {
            Debug.LogError("Punch Interactor not assigned in the inspector.");
            gameObject.SetActive(false);
        }
    }
    
    
    private void Update()
    {
        float velocity = punchInteractor.currentVelocity;
        if (velocity > maxVelocity)
        {
            maxVelocity = velocity;
        }
        
        pokeStrengthText.text = "Velocity: " + punchInteractor.currentVelocity.ToString("F2") + "\nMax Velocity: " + maxVelocity.ToString("F2");
    }
}
