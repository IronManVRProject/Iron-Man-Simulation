using AI.FSM.Internal;
using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Actions/Attack")]
    public class AttackAction : FSMAction
    {
        public float damage = 10;
        public State onKillState;
        
        public override void Execute(FiniteStateMachine stateMachine)
        {
            var enemy = stateMachine.GetComponent<EnemySightSensor>();
            
            if (!enemy.player.gameObject.activeSelf)
            {
                stateMachine.currentState = onKillState;
                return;
            }
            
            var health = enemy.player.GetComponent<Health>();
            
            if (health.IsAlive())
            {
                health.TakeDamage(damage * Time.deltaTime);
            }
        }
    }
}