using AI.FSM.Internal;
using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Actions/Attack")]
    public class AttackAction : FSMAction
    {
        
        public override void Execute(FiniteStateMachine stateMachine)
        {
            Debug.Log("Attacking player");
        }
    }
}