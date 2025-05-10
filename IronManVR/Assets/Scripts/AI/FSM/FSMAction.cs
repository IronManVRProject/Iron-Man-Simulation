using UnityEngine;

// Code from https://www.toptal.com/unity/unity-ai-development-finite-state-machine-tutorial
namespace AI.FSM
{
    public abstract class FSMAction : ScriptableObject
    {
        public abstract void Execute(FiniteStateMachine fsm);
    }
}