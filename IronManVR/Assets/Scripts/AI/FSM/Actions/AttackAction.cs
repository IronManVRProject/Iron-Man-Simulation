using AI.FSM.Internal;
using UnityEngine;

namespace AI.FSM
{
    [CreateAssetMenu(menuName = "FSM/Actions/Attack")]
    public class AttackAction : FSMAction
    {
        public int damage = 10;
        
        public override void Execute(FiniteStateMachine stateMachine)
        {
            var enemy = stateMachine.GetComponent<EnemySightSensor>();
            var health = enemy.player.GetComponent<Health>();
            
            if (enemy.player.GetComponent<Health>().IsAlive())
            {
                health.TakeDamage(damage);
            }
        }
    }
}