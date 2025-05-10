using AI.FSM.Internal;
using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Decisions/In Line Of Sight")]
    public class InLineOfSightDecision : Decision
    {
        public override bool Decide(FiniteStateMachine stateMachine)
        {
            var enemyInLineOfSight = stateMachine.GetComponent<EnemySightSensor>();
            return enemyInLineOfSight.Ping();
        }
    }
}