using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Actions/ChangeAnimationStateAction")]
    public class ChangeAnimationStateAction : FSMAction
    {
        public string animationStateName;
        
        public override void Execute(FiniteStateMachine stateMachine)
        {
            var animator = stateMachine.GetComponent<Animator>();

            if (!animator)
            {
                Debug.LogError("Animator component not found on the state machine.");
                return;
            }

            if (animator != null && !animator.GetBool(animationStateName))
            {
                var parameters = animator.parameters;

                foreach (var parameter in parameters)
                {
                    animator.SetBool(parameter.name, false);
                }

                animator.SetBool(animationStateName, true);

                Debug.Log($"Animation state {animationStateName} toggled");
            }
        }
    }
}