using UnityEngine;
using UnityEngine.AI;

namespace AI.FSM
{
    public class PatrolPoints : MonoBehaviour
    {
        [SerializeField] private Transform[] patrolPoints;

        public Transform currentPoint => patrolPoints[currentPointIndex];

        private int currentPointIndex;

        /// <summary>
        /// Gets the next point to patrol to
        /// </summary>
        /// <returns></returns>
        public Transform GetNext()
        {
            var point = patrolPoints[currentPointIndex];
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            return point;
        }

        /// <summary>
        /// Checks if destination reached
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool HasReached(NavMeshAgent agent)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}