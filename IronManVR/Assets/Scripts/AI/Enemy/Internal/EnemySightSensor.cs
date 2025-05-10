using UnityEngine;

namespace AI.FSM.Internal
{
    public class EnemySightSensor : MonoBehaviour
    {
        public Transform Player;

        [SerializeField] private LayerMask _ignoreMask;

        private Ray _ray;

        private void Awake()
        {
            Player = GameObject.Find("Player").transform;
        }

        public bool Ping()
        {
            if (Player == null)
                return false;

            _ray = new Ray(this.transform.position, Player.position-this.transform.position);

            var dir = new Vector3(_ray.direction.x, 0, _ray.direction.z);

            var angle = Vector3.Angle(dir, this.transform.forward);

            if (angle>60)
                return false;

            if (!Physics.Raycast(_ray, out var hit, 100, ~_ignoreMask))
            {
                return false;
            }
        
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Player in sight");
                
                return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_ray.origin, _ray.origin + _ray.direction * 100);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 100);
        }
    }
}