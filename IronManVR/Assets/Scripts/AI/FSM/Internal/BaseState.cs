using AI.FSM;
using UnityEngine;

// Code from https://www.toptal.com/unity/unity-ai-development-finite-state-machine-tutorial
namespace AI.FSM
{
    public abstract class BaseState : ScriptableObject
    {
        /// <summary>
        /// Called every frame while in the state.
        /// </summary>
        public virtual void Execute(FiniteStateMachine fsm) {}
    }
}
