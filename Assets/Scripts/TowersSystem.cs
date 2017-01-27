using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(GameSystem))]
public class TowersSystem : MonoBehaviour {

    internal List<GameObject> towersPrefabs;
    internal List<GameObject> towers;

    internal GameObject projectilePrefab;

    internal PubSubHub pubSubHub;

    public void Start()
    {
        towersPrefabs = new List<GameObject>();
        towers = new List<GameObject>();

        var towerPrefabs = Resources.LoadAll<GameObject>("Prefabs/Towers");

        foreach (var prefab in towerPrefabs)
            if (prefab.GetComponent<TowerComponent>() != null)
                towersPrefabs.Add(prefab);

        projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");

        foreach (var prefab in towersPrefabs)
            towers.Add(Instantiate<GameObject>(prefab));

        pubSubHub = GetComponent<PubSubHub>();

        if (pubSubHub == null) {
            Debug.LogAssertion("Publisher-Subscriber Hub server wasn't found.");
            Application.Quit();
        }
    }

    public void GetReady()
    {
        foreach (var tower in towers) {
            StartCoroutine(StartShooting(tower.GetComponent<TowerComponent>(), Instantiate<GameObject>(projectilePrefab)));
        }
    }

    public void HoldFire()
    {
        StopAllCoroutines();
    }

    IEnumerator StartShooting(TowerComponent tower, GameObject projectile)
    {
        var force = Vector3.zero;
        var targets = new Collider[1];
        var monsterInAttackArea = false;

        var projectileRigidbody = projectile.GetComponent<Rigidbody>();

        var hitDamage = tower.towerParams.hitDamage;

        var contactRadius = tower.towerParams.projectileContactRadius;
        projectile.transform.localScale = Vector3.one * contactRadius;

        var iterationTime = new WaitForFixedUpdate();
        var shotDuration = new WaitForSeconds(tower.towerParams.fireRate);

        var layerMask = 1 << GetComponent<GameSystem>().layerMonster;

        while (true) {
            monsterInAttackArea = Physics.OverlapSphereNonAlloc(tower.transform.position, tower.towerParams.attackRadius, targets, layerMask) > 0 ? true : false;

            if (!monsterInAttackArea) {
                if (projectile.gameObject.activeSelf)
                    projectile.gameObject.SetActive(false);

                StopCoroutine(CheckProjectile(projectileRigidbody, contactRadius, hitDamage));

                yield return iterationTime;
                continue;
            }

            projectileRigidbody.velocity = Vector3.zero;
            projectile.transform.position = tower.transform.position;

            projectile.gameObject.SetActive(true);

            force = targets[0].transform.position - projectile.transform.position;
            force.Normalize();

            StartCoroutine(CheckProjectile(projectileRigidbody, contactRadius, hitDamage));

            projectileRigidbody.AddForce(force * 20, ForceMode.VelocityChange);

            yield return shotDuration;
        }
    }

    IEnumerator CheckProjectile(Rigidbody rigidbody, float contactRadius, int hitDamage)
    {
        var monster = new Collider[1];
        var projectileHasHitMonster = false;

        var iterationStep = new WaitForFixedUpdate();

        var layerMask = 1 << GetComponent<GameSystem>().layerMonster;

        while (true) {
            projectileHasHitMonster = Physics.OverlapSphereNonAlloc(rigidbody.position, contactRadius, monster, layerMask) > 0 ? true : false;

            if (projectileHasHitMonster) {
                rigidbody.gameObject.SetActive(false);

                var message = new GameSystem.ProjectileHasHitMonsterMessage(monster[0].gameObject, hitDamage);

                pubSubHub.Publish(rigidbody.gameObject, message);

                yield break;
            }

            yield return iterationStep;
        }
    }
}