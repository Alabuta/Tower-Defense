using System.Collections;
using System.Collections.Generic;

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

    internal List<Button> btnTowerInStore;

    void Reset()
    {
        // Warning message for duplicate instances of the script.
        if (FindObjectsOfType<GameSystem>().Length > 1) {
            Debug.LogWarning("'Game' component is already instantiated.");
            enabled = false;
        }
    }

    IEnumerator Start()
    {
        // Make sure that subsystems are initialized.
        yield return new WaitWhile(() => GetComponent<TowersSystem>() == null || GetComponent<MonstersSystem>() == null);

        health = healthOnStart;
        money = moneyOnStart;

        waveNumber = 0;

        InitUI();
        InitTowersStoreUI();
    }

    void InitUI()
    {
        try {
            // GUI initialization routine...
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
        var buttonTower = Resources.Load<GameObject>("Prefabs/UI/buttonTower");

        if (buttonTower == null) {
            Debug.LogAssertion("'buttonTower' UI prefab doesn't exist.");
            Application.Quit();
        }

        try {
            // Object 'panelTowers' is being used like chart for all towers' prefab buttons.
            var panelTowers = GameObject.Find("panelTowers").transform;
            var buttonSize = buttonTower.GetComponent<RectTransform>().sizeDelta;

            // All we got from 'TowerSystem' subsytem for towers chart.
            var towersPrefabs = GetComponent<TowersSystem>().GetAllTowerPrefabs();

            // Margin calculations...
            panelTowers.GetComponent<RectTransform>().sizeDelta = new Vector2(256, 16 + towersPrefabs.Count * (buttonSize.y + 16));

            btnTowerInStore = new List<Button>();

            // Passing through all existing towers' prefabs and creating relevant buttons.
            for (var i = 0; i < towersPrefabs.Count; ++i) {
                var localIndex = i;

                var towerParams = towersPrefabs[i].towerParams;

                var button = Instantiate<GameObject>(buttonTower, panelTowers).GetComponent<Button>();
                button.transform.FindChild("Text").GetComponent<Text>().text = towerParams.towerName;

                var buttonRect = button.GetComponent<RectTransform>();

                // Very tricky part, check after changes.
                buttonRect.localRotation = Quaternion.identity;
                buttonRect.localScale = Vector3.one;
                buttonRect.anchorMin = new Vector2(0, 1);
                buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.pivot = new Vector2(0, 1);
                buttonRect.sizeDelta = new Vector2(-32, 64);
                buttonRect.anchoredPosition3D = new Vector3(16, -16 - i * (buttonRect.sizeDelta.y + 16));

                var text = button.transform.FindChild("TooltipPanel").FindChild("TooltipText").GetComponent<Text>();

                text.text = towerParams.towerDescription;
                text.text += "\n\n\tPrice: " + towerParams.price.ToString();

                button.interactable = towerParams.price > money ? false : true;

                button.onClick.AddListener(delegate
                {
                    SendMessage("SetTower", localIndex, SendMessageOptions.RequireReceiver);
                });

                btnTowerInStore.Add(button);
            }
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

        GetComponent<MonstersSystem>().StartSpawnMonsters(respawnPosition, finishPosition, Mathf.RoundToInt(monstersMaxCount * Mathf.Sqrt(waveNumber)), rateOfSpawn / Mathf.Sqrt(waveNumber));
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

        CheckTowerStoreUI();
    }

    void MonsterHasBeenKilled(int rewardForKilling)
    {
        money = Mathf.Clamp(money + rewardForKilling, money, int.MaxValue);
        textMoneyValue.text = money.ToString();

        CheckTowerStoreUI();
    }

    void CheckTowerStoreUI()
    {
        var text = "";
        var towersPrefabs = GetComponent<TowersSystem>().GetAllTowerPrefabs();

        TowerComponent prefab = null;

        // If player has enough money for buying a tower - make it available in towers chart.
        foreach (var button in btnTowerInStore) {
            text = button.transform.FindChild("Text").GetComponent<Text>().text;

            prefab = towersPrefabs.Find(a => a.towerParams.towerName == text ? true : false);

            if (prefab != null)
                button.interactable = prefab.towerParams.price > money ? false : true;
        }
    }
}