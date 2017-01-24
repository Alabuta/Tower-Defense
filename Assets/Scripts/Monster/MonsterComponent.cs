using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

[ExecuteInEditMode, RequireComponent(typeof(NavMeshAgent))]
public class MonsterComponent : MonoBehaviour {

    [SerializeField]
    public MonsterParams monsterParams;

    void Reset()
    {
        var agent = gameObject.GetComponent<NavMeshAgent>();

        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        var agent = gameObject.GetComponent<NavMeshAgent>();

        agent.radius = monsterParams.radius;

        agent.speed = monsterParams.speed;
        agent.angularSpeed = monsterParams.angularSpeed;
        agent.acceleration = monsterParams.acceleration;
        agent.stoppingDistance = 0.0f;
        agent.autoBraking = false;

        agent.avoidancePriority = monsterParams.avoidancePriority;
#endif
    }
}