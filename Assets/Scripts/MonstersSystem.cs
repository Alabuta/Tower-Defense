using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class MonstersSystem : MonoBehaviour, ISystem {

    public GameObject monsters;
    Vector3 respawnPosition, finishPosition;

    GameObject weakMonster;

    int count = 0;

    void Start()
    {
        respawnPosition = GameObject.FindWithTag("Respawn").transform.position;
        finishPosition = GameObject.FindWithTag("Finish").transform.position;

        weakMonster = Resources.Load<GameObject>("Prefabs/MonsterWeak");
    }

    void FixedUpdate()
    {
        StartCoroutine(ProduceMonsters());

        if (count > 10)
            StopCoroutine(ProduceMonsters());
    }

    IEnumerator ProduceMonsters()
    {
        yield return new WaitForSeconds(5);

        monsters = Instantiate<GameObject>(weakMonster, respawnPosition, Quaternion.identity);

        var agent = monsters.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.SetDestination(finishPosition);

        ++count;
    }

    void ISystem.UpdateSystem()
    {
        throw new NotImplementedException();
    }

    void AddMonster()
    {
    }

    void KillMonster()
    {
    }

    void Reset()
    {
        //monsters = FindObjectsOfType<MonsterComponent>();
    }
}