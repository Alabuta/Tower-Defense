﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(GameSystem))]
public sealed class TowersSystem : MonoBehaviour {

    internal List<TowerComponent> towersPrefabs;
    internal List<TowerComponent> towers;

    internal GameObject projectilePrefab;

    // Used in looking place for tower case.
    internal GameObject spawnZoneIsValid, spawnZoneIsInvalid;

    internal bool keepShooting;

    public void Start()
    {
        towersPrefabs = new List<TowerComponent>();
        towers = new List<TowerComponent>();
        keepShooting = false;

        var towerPrefabs = Resources.LoadAll<GameObject>("Prefabs/Towers");

        foreach (var prefab in towerPrefabs)
            if (prefab.GetComponent<TowerComponent>() != null)
                towersPrefabs.Add(prefab.GetComponent<TowerComponent>());

        towersPrefabs.Sort(delegate (TowerComponent a, TowerComponent b)
        {
            var x = a.towerParams.price;
            var y = b.towerParams.price;

            if (x > y)
                return 1;

            else if (x < y)
                return -1;

            else
                return 0;
        });

        projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");

        if (projectilePrefab == null) {
            Debug.LogAssertion("'Projectile' prefab doesn't exist.");
            Application.Quit();
        }

        // Valid - green, invalid - red circles.
        var spawnZoneIsValidPrefab = Resources.Load<GameObject>("Prefabs/spawnZoneIsValid");
        var spawnZoneIsInvalidPrefab = Resources.Load<GameObject>("Prefabs/spawnZoneIsInvalid");

        if (spawnZoneIsValidPrefab == null || spawnZoneIsInvalidPrefab == null) {
            Debug.LogAssertion("'Spawn Zone' prefabs don't exist.");
            Application.Quit();
        }

        spawnZoneIsValid = Instantiate<GameObject>(spawnZoneIsValidPrefab);
        spawnZoneIsInvalid = Instantiate<GameObject>(spawnZoneIsInvalidPrefab);

        if (spawnZoneIsValid == null || spawnZoneIsInvalid == null) {
            Debug.LogAssertion("'Spawn Zone' objects are invalid.");
            Application.Quit();
        }

        spawnZoneIsValid.SetActive(false);
        spawnZoneIsInvalid.SetActive(false);
    }

    public List<TowerComponent> GetAllTowerPrefabs()
    {
        return towersPrefabs;
    }

    void SetTower(int number)
    {
        var index = Mathf.Clamp(number, 0, towersPrefabs.Count - 1);

        StartCoroutine(FindPlaceForTower(towersPrefabs[index]));
    }

    void InstantiateTowerPrefab(TowerComponent towerPrefab, Vector3 position)
    {
        var tower = Instantiate<TowerComponent>(towerPrefab, position, Quaternion.identity);
        towers.Add(tower);

        if (keepShooting)
            StartCoroutine(StartShooting(tower, Instantiate<GameObject>(projectilePrefab)));

        SendMessage("SpendMoney", tower.towerParams.price, SendMessageOptions.RequireReceiver);
    }

    IEnumerator FindPlaceForTower(TowerComponent towerPrefab)
    {
        var results = new RaycastHit[1];
        var neighbours = new Collider[1];

        var layerMask = 1 << GetComponent<GameSystem>().layerGround;

        var iterationStep = new WaitForFixedUpdate();

        var extents = towerPrefab.GetComponent<MeshRenderer>().bounds.extents;
        var max = Mathf.Max(extents.x, extents.z);

        var radius = towerPrefab.towerParams.attackRadius;

        spawnZoneIsValid.transform.localScale = new Vector3(radius, spawnZoneIsValid.transform.localScale.y / 2, radius) * 2;
        spawnZoneIsInvalid.transform.localScale = new Vector3(radius, spawnZoneIsValid.transform.localScale.y / 2, radius) * 2;

        var tower = Instantiate<TowerComponent>(towerPrefab);
        Destroy(tower.GetComponent<Collider>());
        tower.gameObject.SetActive(false);

        var isFit = false;

        while (true) {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var amount = Physics.RaycastNonAlloc(ray, results, 100, layerMask);

            if (amount == 1) {
                isFit = Physics.OverlapSphereNonAlloc(results[0].point, max, neighbours, ~layerMask) == 0 ? true : false;

                if (isFit) {
                    spawnZoneIsValid.SetActive(true);
                    spawnZoneIsValid.transform.position = results[0].point;

                    spawnZoneIsInvalid.SetActive(false);
                }

                else {
                    spawnZoneIsInvalid.SetActive(true);
                    spawnZoneIsInvalid.transform.position = results[0].point;

                    spawnZoneIsValid.SetActive(false);
                }

                tower.transform.position = results[0].point + new Vector3(0, extents.y, 0);
            }

            else {
                isFit = false;

                spawnZoneIsValid.SetActive(false);
                spawnZoneIsInvalid.SetActive(false);
            }

            tower.gameObject.SetActive(amount == 1);

            if (Input.GetMouseButtonDown(0)) {//Input.GetButtonDown("Fire")
                spawnZoneIsValid.SetActive(false);
                spawnZoneIsInvalid.SetActive(false);

                DestroyObject(tower.gameObject);

                if (isFit)
                    InstantiateTowerPrefab(towerPrefab, results[0].point + new Vector3(0, extents.y, 0));

                yield break;
            }

            yield return iterationStep;
        }
    }

    void StartShooting()
    {
        keepShooting = true;

        foreach (var tower in towers)
            StartCoroutine(StartShooting(tower, Instantiate<GameObject>(projectilePrefab)));
    }

    void HoldFire()
    {
        keepShooting = false;
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

        while (keepShooting) {
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

            projectileRigidbody.AddForce(force * 20.0f / tower.towerParams.fireRate, ForceMode.VelocityChange);

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

                var message = new MonstersSystem.ProjectileHasHitMonsterMessage {
                    monster = monster[0].gameObject,
                    hitDamage = hitDamage
                };

                SendMessage("ApplyDamageToMonster", message, SendMessageOptions.RequireReceiver);

                yield break;
            }

            yield return iterationStep;
        }
    }
}