using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(PubSubHub), typeof(MonstersSystem), typeof(TowersSystem))]
public sealed class GameSystem : MonoBehaviour {

    [Header("Player Settings"), Range(50, 500)]
    public int healthOnStart = 100;

    [Range(10, 500)]
    public int moneyOnStart = 100;

    [Range(0, 31)]
    public int layerGround = 0;

    [Header("Monster System Settings")]
    public string tagMonster = "Monster";

    [Range(0, 31)]
    public int layerMonster = 0;

    [Range(1, 10)]
    public int wavesNumber = 4;

    [Range(10, 100)]
    public int monstersMaxCount = 24;

    [Range(.1f, 10f)]
    public float rateOfSpawn = 2.0f;

    [Header("Tower System Settings")]
    public string tagTower = "Tower";

    [Range(0, 31)]
    public int layerTower = 0;

    internal int health, money, waveNumber;

    internal Vector3 respawnPosition, finishPosition;

    internal Button btnStartWave;
    internal Text textHealthValue, textMoneyValue;
    internal Text textYouWin, textYouLose;

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

        waveNumber = 0;

        var respawnObject = GameObject.FindWithTag("Respawn");
        respawnPosition = respawnObject.transform.position;

        var finishObject = GameObject.FindWithTag("Finish");
        finishPosition = finishObject.transform.position;

        InitUI();
    }

    void InitUI()
    {
        try {
            var canvas = GameObject.Find("Canvas").transform;

            textHealthValue = GameObject.Find("textHealthValue").GetComponent<Text>();
            textHealthValue.text = health.ToString();

            textMoneyValue = GameObject.Find("textMoneyValue").GetComponent<Text>();
            textMoneyValue.text = money.ToString();

            textYouWin = canvas.FindChild("textYouWin").GetComponent<Text>();
            textYouWin.enabled = false;

            textYouLose = canvas.FindChild("textYouLose").GetComponent<Text>();
            textYouLose.enabled = false;

            btnStartWave = canvas.FindChild("btnStartWave").GetComponent<Button>();

            btnStartWave.onClick.AddListener(StartNewWave);

            btnStartWave.GetComponent<Button>().interactable = true;

            canvas.FindChild("btnExit").GetComponent<Button>().onClick.AddListener(Application.Quit);

            GameObject.Find("btnTower_#1").GetComponent<Button>().onClick.AddListener(delegate
            {
                GetComponent<TowersSystem>().SetTower(1);
            });
        }

        catch (System.Exception ex) {
            Debug.LogAssertion("Can't reach one of UI objects. " + ex.ToString(), transform);
            Application.Quit();
        }
    }

    void StartNewWave()
    {
        textYouWin.enabled = false;
        textYouLose.enabled = false;

        ++waveNumber;

        btnStartWave.GetComponent<Button>().interactable = false;

        GetComponent<MonstersSystem>().StartSpawnMonsters(respawnPosition, finishPosition, monstersMaxCount, rateOfSpawn);
        GetComponent<TowersSystem>().GetReady();
    }

    void WavePassed()
    {
        btnStartWave.GetComponent<Button>().interactable = true;

        GetComponent<TowersSystem>().HoldFire();

        if (waveNumber >= wavesNumber)
            textYouWin.enabled = true;
    }

    void FixedUpdate()
    {
        if (GetComponent<MonstersSystem>().aliveMonstersCount < 1)
            WavePassed();
    }

    public void ApplyDamageToPlayer(int damage)
    {
        health = Mathf.Clamp(health - damage, 0, healthOnStart);

        if (health < 1) {
            textYouLose.enabled = true;
            GetComponent<TowersSystem>().HoldFire();
        }

        textHealthValue.text = health.ToString();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        textMoneyValue.text = money.ToString();
    }

    public void SpendMoney(int amount)
    {
        money -= amount;
        textMoneyValue.text = money.ToString();
    }
}