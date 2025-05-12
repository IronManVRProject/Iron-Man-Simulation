using UnityEngine;

// Code from https://www.toptal.com/unity/unity-ai-development-finite-state-machine-tutorial
namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Transition")]
    public sealed class Transition : ScriptableObject
    {
        public Decision decision;
        public State trueState;
        public State falseState;

        public void Execute(FiniteStateMachine fsm)
        {
            if (decision.Decide(fsm) && trueState is not RemainInState)
                fsm.currentState = trueState;
            else if (falseState is not RemainInState)
                fsm.currentState = falseState;
        }
    }
}