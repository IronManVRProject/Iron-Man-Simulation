using AI.FSM.Internal;
using UnityEngine;

namespace AI.FSM.Decisions
{
    [CreateAssetMenu(menuName = "FSM/Decisions/Player In Range")]
    public class PlayerInRangeDecision : Decision
    {
        public float detectionRadius = 5f;
        
        public override bool Decide(FiniteStateMachine stateMachine)
        {
            var enemy = stateMachine.GetComponent<EnemySightSensor>();
            
            return enemy.IsPlayerInRange(detectionRadius);
        }
    }
}