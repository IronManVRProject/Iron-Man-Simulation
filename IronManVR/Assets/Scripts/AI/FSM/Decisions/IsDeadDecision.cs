using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Decisions/IsDead")]
    public class IsDeadDecision : Decision
    {
        public override bool Decide(FiniteStateMachine fsm)
        {
            var health = fsm.GetComponent<Health>();

            if (health && !health.IsAlive())
            {
                Debug.Log("Thanos died!");
                return true;
            }

            return false;
        }
    }
}