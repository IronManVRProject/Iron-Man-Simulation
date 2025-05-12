using System.Collections.Generic;
using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/State")]
    public class State : BaseState
    {
        public List<FSMAction> Action = new List<FSMAction>();
        public List<Transition> Transitions = new List<Transition>();

        public override void Execute(FiniteStateMachine machine)
        {
            foreach (var action in Action)
                action.Execute(machine);

            foreach(var transition in Transitions)
                transition.Execute(machine);
        }
    }
}