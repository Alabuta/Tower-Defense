using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PubSubHub), typeof(MonstersSystem), typeof(TowersSystem))]
public class GameSystem : MonoBehaviour {

    [Header("Player Settings"), Range(50, 500)]
    public int healthOnStart = 100;

    [Range(10, 100)]
    public int moneyOnStart = 100;

    [Header("Monster System Settings")]
    public string tagMonster = "Monster";
    public int layerMonster = 0;

    [Range(10, 100)]
    public int monstersMaxCount = 24;

    [Range(.1f, 10f)]
    public float rateOfSpawn = 2.0f;

    [Header("Tower System Settings")]
    public string tagTower = "Tower";
    public int layerTower = 0;

    [HideInInspector]
    public int health, money;

    [HideInInspector]
    public int aliveMonstersCount;

    Vector3 respawnPosition, finishPosition;

    Button btnStartWave;

    public class ProjectileHasHitMonsterMessage {
        public GameObject monster {
            get; private set;
        }

        public int hitDamage {
            get; private set;
        }

        public ProjectileHasHitMonsterMessage(GameObject monster, int damage)
        {
            this.monster = monster;
            this.hitDamage = damage;
        }
    }

    void Reset()
    {
        if (FindObjectsOfType<GameSystem>().Length > 1) {
            Debug.LogWarning("'Game' component is already instantiated.");
            enabled = false;
        }
    }

    void Start()
    {
        health = healthOnStart;
        money = moneyOnStart;

        var respawnObject = GameObject.FindWithTag("Respawn");
        respawnPosition = respawnObject.transform.position;

        var finishObject = GameObject.FindWithTag("Finish");
        finishPosition = finishObject.transform.position;

        try {
            finishObject.GetComponent<OnTriggerEnterExitEventRaiser>().onEnter.AddListener(GameObjectHasReachedFinish);
        }

        catch {
            finishObject.AddComponent<OnTriggerEnterExitEventRaiser>().onEnter.AddListener(GameObjectHasReachedFinish);
        }

        finally {
            finishObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        InitUI();
    }

    void InitUI()
    {
        try {
            btnStartWave = GameObject.Find("btnStartWave").GetComponent<Button>();

            btnStartWave.onClick.AddListener(delegate
            {
                StartNewWave();
            });

            btnStartWave.GetComponent<Button>().interactable = true;

            GameObject.Find("btnExit").GetComponent<Button>().onClick.AddListener(delegate
            {
                Application.Quit();
            });
        }

        catch (System.Exception ex) {
            Debug.LogAssertion("Can't reach one of UI objects. " + ex.ToString(), transform);
            Application.Quit();
        }
    }

    void StartNewWave()
    {
        btnStartWave.GetComponent<Button>().interactable = false;

        GetComponent<MonstersSystem>().StartSpawnMonsters(respawnPosition, finishPosition, monstersMaxCount, rateOfSpawn);
        GetComponent<TowersSystem>().GetReady();
    }

    void WavePassed()
    {
        btnStartWave.GetComponent<Button>().interactable = true;

        GetComponent<TowersSystem>().HoldFire();
    }

    void FixedUpdate()
    {
        if (aliveMonstersCount < 1)
            WavePassed();
    }

    void GameObjectHasReachedFinish(GameObject subject, Collider other)
    {
        if (other.tag == tagMonster) {
            health -= other.GetComponent<MonsterComponent>().monsterParams.damage;

            --aliveMonstersCount;

            DestroyObject(other.gameObject);
        }
    }
}