using Unity.Mathematics.Geometry;
using UnityEngine;

public class PunchInteractor : MonoBehaviour
{
    public GameObject punchPoint;
    
    public float punchStrengthMultiplier = 1.0f;

    [HideInInspector]
    public float velocity;

    private Vector3 oldPos;


    private void Start()
    {
        if (punchPoint == null)
        {
            Debug.LogError("Punch Point not assigned in the inspector.");
            gameObject.SetActive(false);
        }
    }


    private void FixedUpdate()
    {
        CalculateVelocity();
    }

    private void CalculateVelocity()
    {
        Vector3 newPos = punchPoint.transform.position;
        float newVelocity = Mathf.Abs((newPos - oldPos).magnitude) / Time.deltaTime;
        oldPos = newPos;
        
        velocity = newVelocity * punchStrengthMultiplier;
    }
}
