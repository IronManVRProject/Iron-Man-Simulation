using UnityEngine;

namespace AI.FSM
{
    public class EnemyAI : MonoBehaviour
    {
        public GameObject playerObj;
        
        public float detectionRadius = 10f;

        public bool IsPlayerInRange()
        {
            var playerLayer = LayerMask.GetMask("Player");
            
            bool playerInRange = Physics.SphereCast(transform.position, detectionRadius, transform.position, out var hit, detectionRadius, playerLayer);

            if (playerInRange)
            {
                Debug.Log($"Discovered player {playerObj.name} in range!");
            }

            return playerInRange;
        }
    }   
}
