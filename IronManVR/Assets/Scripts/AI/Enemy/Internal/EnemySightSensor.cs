using UnityEngine;

namespace AI.FSM.Internal
{
    public class EnemySightSensor : MonoBehaviour
    {
        public Transform player;

        [SerializeField] private LayerMask ignoreMask;

        private Ray ray;

        private void Awake()
        {
            if (player == null)
            {
                player = GameObject.Find("Player").transform;
            }
        }

        public bool Ping()
        {
            if (player == null)
                return false;

            ray = new Ray(transform.position, player.position-transform.position);

            var dir = new Vector3(ray.direction.x, ray.direction.y, ray.direction.z);

            var angle = Vector3.Angle(dir, transform.forward);

            if (angle > 60)
                return false;

            if (!Physics.Raycast(ray, out var hit, 100, ~ignoreMask))
            {
                Debug.Log("Raycast hit nothing");
                return false;
            }
        
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Player in sight");
                
                return true;
            }

            return false;
        }
        
        
        public bool IsPlayerInRange(float detectionRadius = 10f)
        {
            var playerLayer = LayerMask.GetMask("Player");
            
            bool playerInRange = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer).Length > 0;
            
            if (playerInRange)
            {
                Debug.Log("Player in range");
            }

            return playerInRange;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 100);
        }
    }
}