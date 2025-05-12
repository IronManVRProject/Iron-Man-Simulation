using UnityEngine;

namespace AI.FSM.Decisions
{
    [CreateAssetMenu(menuName = "FSM/Decisions/IsInStateDecision")]
    public class FSMIsInStateDecision : Decision
    {
        public FiniteStateMachine otherStateMachine;
        public State stateToCheck;
        
        public override bool Decide(FiniteStateMachine fsm)
        {
            return stateToCheck == otherStateMachine.currentState;
        }
    }
}