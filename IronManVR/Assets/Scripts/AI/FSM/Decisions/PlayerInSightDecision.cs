using UnityEngine;

namespace AI.FSM.Decisions
{
    [CreateAssetMenu(menuName = "FSM/Decisions/Player In Range")]
    public class PlayerInRangeDecision : Decision
    {
        public override bool Decide(FiniteStateMachine stateMachine)
        {
            var enemy = stateMachine.GetComponent<EnemyAI>();
            
            return enemy.IsPlayerInRange();
        }
    }
}