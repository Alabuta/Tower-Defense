using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

[RequireComponent(typeof(GameSystem))]
public sealed class MonstersSystem : MonoBehaviour {

    internal List<MonsterComponent> monstersPrefabs;
    internal WaitForSeconds spawnDuration;

    internal int spawnedMonstersCount;
    internal int monstersMaxCount;

    public int aliveMonstersCount {
        get; private set;
    }

    // Message class for applying damage to a monster.
    // Receives from 'TowerSystem' subsystem.
    public class ProjectileHasHitMonsterMessage {
        public GameObject monster {
            get; set;
        }

        public int hitDamage {
            get; set;
        }
    }

    void Start()
    {
        monstersPrefabs = new List<MonsterComponent>();

        var prefabs = Resources.LoadAll<GameObject>("Prefabs/Monsters");

        // Creating monsters' prefabs collection.
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

                monstersPrefabs.Add(prefab.GetComponent<MonsterComponent>());
            }
        }

        monstersPrefabs.Sort(delegate (MonsterComponent a, MonsterComponent b)
        {
            var x = a.monsterParams.chanceToSpawn;
            var y = b.monsterParams.chanceToSpawn;

            if (x < y)
                return 1;

            else if (x > y)
                return -1;

            else
                return 0;
        });
    }

    public void StartSpawnMonsters(Vector3 respawnPosition, Vector3 finishPosition, int monstersMaxCount, float rateOfSpawn)
    {
        spawnedMonstersCount = 0;
        aliveMonstersCount = 0;

        this.monstersMaxCount = monstersMaxCount;

        StartCoroutine(CheckWhetherMonsterHadReachedFinish(finishPosition, 0.64f));

        StartCoroutine(SpawnMonsters(respawnPosition, finishPosition, monstersMaxCount, rateOfSpawn));
    }

    IEnumerator SpawnMonsters(Vector3 respawnPosition, Vector3 finishPosition, int monstersMaxCount, float rateOfSpawn)
    {
        spawnDuration = new WaitForSeconds(rateOfSpawn);

        while (true) {
            if (SpawnMonster(respawnPosition, finishPosition)) {
                ++aliveMonstersCount;
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

        var monster = Instantiate<MonsterComponent>(monstersPrefabs[index], respawnPosition, Quaternion.identity);

        if (monster == null) {
            Debug.LogError("Can't instantiate prefab.");
            return false;
        }

        var successful = monster.GetComponent<NavMeshAgent>().SetDestination(finishPosition);

        if (!successful) {
            Debug.LogError("Can't set agent destination.");
            DestroyImmediate(monster.gameObject);
            return false;
        }

        return true;
    }

    void ApplyDamageToMonster(ProjectileHasHitMonsterMessage message)
    {
        var monsterComponent = message.monster.GetComponent<MonsterComponent>();

        monsterComponent.health -= message.hitDamage;

        if (monsterComponent.health < 1) {
            DestroyImmediate(message.monster.gameObject);

            SendMessage("MonsterHasBeenKilled", monsterComponent.monsterParams.rewardForKilling, SendMessageOptions.RequireReceiver);

            --aliveMonstersCount;

            if (aliveMonstersCount < 1 && spawnedMonstersCount >= monstersMaxCount)
                SendMessage("AllMonstersHaveDied", SendMessageOptions.RequireReceiver);
        }
    }

    IEnumerator CheckWhetherMonsterHadReachedFinish(Vector3 position, float contactRadius)
    {
        var monsters = new Collider[20];
        var monstersHadReachedFinishAmount = 0;

        var iterationStep = new WaitForFixedUpdate();

        var layerMask = 1 << GetComponent<GameSystem>().layerMonster;

        while (true) {
            monstersHadReachedFinishAmount = Physics.OverlapSphereNonAlloc(position, contactRadius, monsters, layerMask);

            for (var i = 0; i < monstersHadReachedFinishAmount; ++i) {
                SendMessage("ApplyDamageToPlayer", monsters[i].GetComponent<MonsterComponent>().monsterParams.damage, SendMessageOptions.RequireReceiver);

                DestroyImmediate(monsters[i].gameObject);

                --aliveMonstersCount;

                if (aliveMonstersCount < 1 && spawnedMonstersCount >= monstersMaxCount)
                    SendMessage("AllMonstersHaveDied", SendMessageOptions.RequireReceiver);
            }

            yield return iterationStep;
        }
    }
}