using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

[RequireComponent(typeof(GameSystem))]
public class MonstersSystem : MonoBehaviour {

    List<GameObject> monstersPrefabs;
    WaitForSeconds spawnDuration;

    int spawnedMonstersCount;

    void Start()
    {
        monstersPrefabs = new List<GameObject>();

        var prefabs = Resources.LoadAll<GameObject>("Prefabs/Monsters");

        foreach (var prefab in prefabs) {
            if (prefab.GetComponent<MonsterComponent>() != null) {
                if (prefab.GetComponent<SphereCollider>() == null) {
                    Debug.LogError("Prefab has 'MonsterComponent', but doesn't have 'SphereCollider'.");
                    continue;
                }

                if (prefab.GetComponent<NavMeshAgent>() == null) {
                    Debug.LogError("Prefab has 'MonsterComponent', but doesn't have 'NavMeshAgent'.");
                    continue;
                }

                monstersPrefabs.Add(prefab);
            }
        }

        monstersPrefabs.Sort(delegate (GameObject a, GameObject b)
        {
            var x = a.GetComponent<MonsterComponent>().monsterParams.chanceToSpawn;
            var y = b.GetComponent<MonsterComponent>().monsterParams.chanceToSpawn;

            if (x < y)
                return 1;

            else if (x > y)
                return -1;

            else
                return 0;
        });

        var pubSubHub = GetComponent<PubSubHub>();

        if (pubSubHub == null) {
            Debug.LogAssertion("Publisher-Subscriber Hub server wasn't found.");
            Application.Quit();
        }

        pubSubHub.Subscribe<GameSystem.ProjectileHasHitMonsterMessage>(this, ApplyHitDamageToMonster);
    }

    public void StartSpawnMonsters(Vector3 respawnPosition, Vector3 finishPosition, int monstersMaxCount, float rateOfSpawn)
    {
        spawnedMonstersCount = 0;

        GetComponent<GameSystem>().aliveMonstersCount = 0;

        StartCoroutine(SpawnMonsters(respawnPosition, finishPosition, monstersMaxCount, rateOfSpawn));
    }

    IEnumerator SpawnMonsters(Vector3 respawnPosition, Vector3 finishPosition, int monstersMaxCount, float rateOfSpawn)
    {
        spawnDuration = new WaitForSeconds(rateOfSpawn);

        while (true) {
            if (SpawnMonster(respawnPosition, finishPosition)) {
                ++GetComponent<GameSystem>().aliveMonstersCount;
                ++spawnedMonstersCount;
            }

            if (spawnedMonstersCount >= monstersMaxCount)
                yield break;

            yield return spawnDuration;
        }
    }

    bool SpawnMonster(Vector3 respawnPosition, Vector3 finishPosition)
    {
        var index = Mathf.RoundToInt((monstersPrefabs.Count - 1) * Mathf.Pow(Random.value, 2.0f));
        index = Mathf.Clamp(index, 0, monstersPrefabs.Count - 1);

        var monster = Instantiate<GameObject>(monstersPrefabs[index], respawnPosition, Quaternion.identity);

        if (monster == null) {
            Debug.LogError("Can't instantiate prefab.");
            return false;
        }

        try {
            monster.GetComponent<NavMeshAgent>().SetDestination(finishPosition);
        }

        catch {
            Debug.LogError("Can't set agent destination.");
            DestroyImmediate(monster);
            return false;
        }

        return true;
    }

    void ApplyHitDamageToMonster(GameSystem.ProjectileHasHitMonsterMessage message)
    {
        var monsterComponent = message.monster.GetComponent<MonsterComponent>();

        monsterComponent.health -= message.hitDamage;

        if (monsterComponent.health < 1) {
            DestroyObject(message.monster);
            --GetComponent<GameSystem>().aliveMonstersCount;

            GetComponent<GameSystem>().money += monsterComponent.monsterParams.rewardForKilling;
        }
    }
}