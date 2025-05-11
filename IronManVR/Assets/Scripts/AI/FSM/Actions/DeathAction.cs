using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Actions/Death")]
    public class DeathAction : FSMAction
    {
        public override void Execute(FiniteStateMachine fsm)
        {
            Debug.Log($"{fsm.name} died.");
            
            Destroy(fsm.gameObject);
        }
    }
}