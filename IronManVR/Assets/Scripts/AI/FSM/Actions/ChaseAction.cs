using UnityEngine;
using UnityEngine.AI;

namespace AI.FSM
{
	[CreateAssetMenu(menuName = "FSM/Actions/Chase")]
    public class ChaseAction : FSMAction
    {
        public override void Execute(FiniteStateMachine stateMachine)
        {
            var navMeshAgent = stateMachine.GetComponent<NavMeshAgent>();
            var enemy = stateMachine.GetComponent<EnemyAI>();

            navMeshAgent.SetDestination(enemy.playerObj.transform.position);
        }
    }
}