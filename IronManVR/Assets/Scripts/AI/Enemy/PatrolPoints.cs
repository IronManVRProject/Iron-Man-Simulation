using UnityEngine;
using UnityEngine.AI;

namespace AI.FSM
{
    public class PatrolPoints : MonoBehaviour
    {
        public Transform[] points;
        private int currentPointIndex;

        public Transform GetNext()
        {
            currentPointIndex = (currentPointIndex + 1) % points.Length;
            return points[currentPointIndex];
        }

        public bool HasReached(NavMeshAgent agent)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}