using System.Collections.Generic;

using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

public class MonstersSystem : MonoBehaviour {

    Vector3 respawnPosition, finishPosition;

    List<GameObject> monstersPrefabs;

    const int kMONSTERS_MAX = 10;
    int monstersCount = 0;

    void Start()
    {
        respawnPosition = GameObject.FindWithTag("Respawn").transform.position;
        finishPosition = GameObject.FindWithTag("Finish").transform.position;

        monstersPrefabs = new List<GameObject>();

        var prefabs = Resources.LoadAll<GameObject>("Prefabs/Monsters");

        foreach (var prefab in prefabs) {
            if (prefab.GetComponent<MonsterComponent>() != null) {
                if (prefab.GetComponent<NavMeshAgent>() == null) {
                    Debug.LogError("Prefab has 'MonsterComponent', but doesn't have 'NavMeshAgent'.");
                    continue;
                }

                monstersPrefabs.Add(prefab);
            }
        }

        InvokeRepeating("AddMonster", 2.0f, 2.0f);
    }

    void AddMonster()
    {
        //foreach (var monstersPrefab in monstersPrefabs)
        InstantiateMonster(monstersPrefabs[0], respawnPosition, Quaternion.identity);

        if (monstersCount >= kMONSTERS_MAX)
            CancelInvoke();
    }

    void InstantiateMonster(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var monster = Instantiate<GameObject>(prefab, position, rotation);

        if (monster == null) {
            Debug.LogAssertion("Can't instantiate prefab.");
            return;
        }

        try {
            monster.GetComponent<NavMeshAgent>().SetDestination(finishPosition);
        }

        catch {
            Debug.LogAssertion("Can't set agent destination.");
            DestroyImmediate(monster);
            return;
        }

        ++monstersCount;
    }

    void KillMonster()
    {
    }
}