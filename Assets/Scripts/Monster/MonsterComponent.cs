using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

[RequireComponent(typeof(SphereCollider), typeof(NavMeshAgent))]
public class MonsterComponent : MonoBehaviour {

    public MonsterParams monsterParams;

    [HideInInspector]
    public int health = 0;

    void Reset()
    {
        if (gameObject.GetComponent<NavMeshAgent>() == null)
            gameObject.AddComponent<NavMeshAgent>();
    }

    void OnValidate()
    {
        var agent = gameObject.GetComponent<NavMeshAgent>();

        agent.radius = monsterParams.radius;

        agent.speed = monsterParams.speed;
        agent.angularSpeed = monsterParams.angularSpeed;
        agent.acceleration = monsterParams.acceleration;
        agent.stoppingDistance = 0.0f;
        agent.autoBraking = false;

        agent.avoidancePriority = monsterParams.avoidancePriority;

        health = monsterParams.health;
    }
}