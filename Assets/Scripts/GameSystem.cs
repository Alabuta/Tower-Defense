using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonstersSystem), typeof(TowersSystem))]
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
    public int wavesTotalNumber = 4;

    [Range(10, 100)]
    public int monstersMaxCount = 24;

    [Range(.1f, 10f)]
    public float rateOfSpawn = 2.0f;

    [Header("Tower System Settings")]
    public string tagTower = "Tower";

    [Range(0, 31)]
    public int layerTower = 0;

    internal int health, money, waveNumber;

    internal Button btnStartWave;
    internal Text textHealthValue, textMoneyValue, textWavesCurrentValue;
    internal Text textYouWin, textYouLose;

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

        InitUI();
        InitTowersStoreUI();
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

            textWavesCurrentValue = GameObject.Find("textWavesCurrentValue").GetComponent<Text>();
            textWavesCurrentValue.text = waveNumber.ToString();

            GameObject.Find("textWavesTotalValue").GetComponent<Text>().text = wavesTotalNumber.ToString();
        }

        catch (System.Exception ex) {
            Debug.LogAssertion("Can't reach one of UI objects. " + ex.ToString(), transform);
            Application.Quit();
        }
    }

    void InitTowersStoreUI()
    {
        try {
            GameObject.Find("btnTower_#1").GetComponent<Button>().onClick.AddListener(delegate
            {
                SendMessage("SetTower", 0, SendMessageOptions.RequireReceiver);
            });

            GameObject.Find("btnTower_#2").GetComponent<Button>().onClick.AddListener(delegate
            {
                SendMessage("SetTower", 1, SendMessageOptions.RequireReceiver);
            });

            GameObject.Find("btnTower_#3").GetComponent<Button>().onClick.AddListener(delegate
            {
                SendMessage("SetTower", 2, SendMessageOptions.RequireReceiver);
            });

            textWavesCurrentValue = GameObject.Find("textWavesCurrentValue").GetComponent<Text>();
            textWavesCurrentValue.text = waveNumber.ToString();

            GameObject.Find("textWavesTotalValue").GetComponent<Text>().text = wavesTotalNumber.ToString();
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

        textWavesCurrentValue.text = (++waveNumber).ToString();

        btnStartWave.GetComponent<Button>().interactable = false;

        var respawnPosition = Vector3.one;
        var finishPosition = Vector3.one;

        try {
            respawnPosition = GameObject.FindWithTag("Respawn").transform.position;
            finishPosition = GameObject.FindWithTag("Finish").transform.position;
        }

        catch (System.Exception ex) {
            Debug.LogAssertion("'Finish' or 'Respawn' tagged object is not exist. " + ex.ToString());
            Application.Quit();
        }

        GetComponent<MonstersSystem>().StartSpawnMonsters(respawnPosition, finishPosition, monstersMaxCount, rateOfSpawn);
        SendMessage("StartShooting", SendMessageOptions.RequireReceiver);
    }

    void AllMonstersHaveDied()
    {
        SendMessage("HoldFire", SendMessageOptions.RequireReceiver);

        if (health > 0) {
            if (waveNumber >= wavesTotalNumber)
                textYouWin.enabled = true;

            else
                btnStartWave.GetComponent<Button>().interactable = true;
        }

        else
            textYouLose.enabled = true;
    }

    void GameOver()
    {
        textYouLose.enabled = true;
        SendMessage("HoldFire", SendMessageOptions.RequireReceiver);
    }

    void ApplyDamageToPlayer(int damage)
    {
        health = Mathf.Clamp(health - damage, 0, healthOnStart);

        if (health < 1)
            GameOver();

        textHealthValue.text = health.ToString();
    }

    void SpendMoney(int amount)
    {
        money = Mathf.Clamp(money - amount, 0, money);
        textMoneyValue.text = money.ToString();
    }

    void MonsterHasBeenKilled(int rewardForKilling)
    {
        money = Mathf.Clamp(money + rewardForKilling, money, int.MaxValue);
        textMoneyValue.text = money.ToString();
    }
}