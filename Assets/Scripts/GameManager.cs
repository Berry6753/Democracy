using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    [Header("Player")]
    [SerializeField]
    private GameObject Player;

    [Header("Core")]
    [SerializeField]
    private GameObject core;

    [Header("Player_UI")]
    [SerializeField]
    private GameObject playerUI;

    [Header("Command_UI")]
    [SerializeField]
    private GameObject coreUI;

    [Header("PlayerSpawnPoint")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("시작 메뉴")]
    [SerializeField] private GameObject startUI;

    [Header("Play Object")]
    [SerializeField] private GameObject playOj;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseUI;

    [Header("WaveSystem")]
    [SerializeField]
    private WaveSystem waveSystem;

    [Header("MonsterSpawner")]
    [SerializeField]
    private MonsterSpawner monsterSpawner;  

    [Header("Record Text")]
    [SerializeField]
    private List<TMP_Text> RecordText;

    [Header("BossMonster")]
    [SerializeField]
    private BossMonster boss;

    [Header("Sound Manager")]
    [SerializeField] private GameObject sound;

    [Header("Game Clear")]
    [SerializeField] private GameObject clear;

    [Header("Game Over")]
    [SerializeField] private GameObject over;

    [Header("몬스터 스포너")]
    [SerializeField] private MonsterSpawnerCrystal_Last monsterCrystalLast;
    [SerializeField] private List<MonsterSpawnCrystal> monsterCrystal;

    public float isGameStop;
    public bool isGameOver { get; private set; }
    public bool isGameClear { get; private set; }
    [HideInInspector] public Player_Info playerInfo;
    private Player_Command Player_Command;

    public GameObject GetPlayer {  get { return Player; } }
    public GameObject GetCore {  get { return core; } }
    public GameObject GetCoreUI { get {  return coreUI; } }
    public GameObject GetPlayerUI { get { return playerUI; } }
    public Transform GetSpawnPoint {  get { return spawnPoint; } }
    public WaveSystem WaveSystem { get { return waveSystem; } }
    public BossMonster Boss { get { return boss; } }
    public GameObject Sound { get { return sound; } }
    public MonsterSpawner MonsterSpawner { get { return monsterSpawner; } }

    public List<MonsterSpawnCrystal> MonsterCrystal {  get { return monsterCrystal; } }

    float time = 0.0f;
    bool isGameEnd;
    private void Awake()
    {
        isGameStop = 1;
        isGameOver = false;
        playerInfo = Player.GetComponent<Player_Info>();
        Player_Command = Player.GetComponent<Player_Command>();
        isGameEnd = true;
    }

    public void OnGameStop(InputAction.CallbackContext context)
    {
        if (startUI.activeSelf == true) return;
        if (context.started) return;
        if (context.performed)
        {
            if (Player_Command.isCommand) return;
            isGameStop *= -1;
        }        
    }

    public void OnGameStart()
    {
        isGameStop = -1;
        isGameEnd = false;
        core.gameObject.SetActive(true);
        monsterCrystalLast.gameObject.SetActive(true);

        foreach (var item in monsterCrystal)
        { 
            item.gameObject.SetActive(true);
        }
    }

    public void OnContinue()
    {
        isGameStop = -1;
    }

    public void ReturnToStartUI()
    {
        isGameEnd = true;
    }

    private void Update()
    {
        GameStopping();

        //if (isGameOver)
        //{
        //    //데이터 저장
        //    SaveDataToJson();

        //    //게임 오버 화면

        //}
        CheckGaemClear();
        CheckGameOver();
    }

    public void ShowRecord()
    {
        LoadRankingData();

        int index = 0;
        foreach (var texts in RecordText)
        {
            if (dataList.Count < index+1) continue;
            texts.text = $"Wave : {dataList[index].waveCount}, timer : {dataList[index].playTime}";
            index++;
        }
    }

    private void Start()
    {
        path = Path.Combine(Application.persistentDataPath, "waveData.json");

        LoadRankingData();
    }

    string path;
    
    class SaveData
    {
        public int waveCount;
        public float playTime;
    }
    
    List<SaveData> dataList = new List<SaveData>();

    public void SaveDataToJson()
    {
        if(File.Exists(path))
        {
            LoadRankingData();
        }

        SaveData data = new SaveData();
        data.waveCount = waveSystem.currentWaveIndex;
        data.playTime = waveSystem.PlayTimer;

        dataList.Add(data);
        dataList = dataList.OrderByDescending(x => x.waveCount).ThenBy(x => x.playTime).ToList();

        Debug.Log(dataList.Count);

        string jsonData = JsonConvert.SerializeObject(dataList);/*JsonUtility.ToJson(dataList);*/

        File.WriteAllText(path, jsonData);
    }

    public void LoadRankingData()
    {
        if(!File.Exists(path))
        {
            SaveDataToJson();
        }

        string jsonData = File.ReadAllText(path);
        dataList = JsonConvert.DeserializeObject<List<SaveData>>(jsonData);/*JsonUtility.FromJson<List<SaveData>>(jsonData);*/
        

        //int index = 0;
        //foreach(var texts in RecordText)
        //{
        //    texts.text = $"{dataList[index]}";
        //    index++;
        //}
    }

    private void GameStopping()
    {        
        if (isGameStop > 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (!Player_Command.isCommand)
            {
                pauseUI.SetActive(true);
            }            
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (!Player_Command.isCommand)
            {
                pauseUI.SetActive(false);
            }
            Time.timeScale = 1f;
        }
    }

    #region Exit

    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    private void CheckGaemClear()
    {
        if (isGameEnd) return;
        if (!monsterCrystalLast.gameObject.activeSelf)
        {
            isGameClear = true;
            clear.SetActive(true);
            Time.timeScale = 0.0f;
            time += Time.unscaledDeltaTime;
            if (time >= 3)
            {
                Time.timeScale = 1.0f;
                clear.SetActive(false);
                playOj.SetActive(false);
                startUI.SetActive(true);
                time = 0;
                isGameClear = false;
                isGameEnd = true;

                isGameStop = 1;
            }
        }
    }

    private void CheckGameOver()
    {
        if (isGameEnd) return;
        if (!core.activeSelf && playerInfo.isDead)
        {
            isGameOver = true;
            over.SetActive(true);
            time += Time.unscaledDeltaTime;
            if (time >= 3)
            {
                over.SetActive(false);
                playOj.SetActive(false);
                startUI.SetActive(true);
                time = 0.0f;
                isGameOver = false;
                isGameEnd = true;

                isGameStop = 1;
            }
        }
    }

    public void OnWaveSkip(InputAction.CallbackContext context)
    {
        if (context.started) return;
        if (context.canceled) return;
        if (waveSystem.currentWaveIndex >= 30) return;
        if( context.performed)
        {
            //waveSystem.currentWaveIndex++;
            //waveSystem.WaveCount_Text.text = $"{waveSystem.currentWaveIndex} Wave";
            StopCoroutine(waveSystem.monsterSpawner.SpawnMonster());
            foreach (var monster in waveSystem.monsterSpawner.MonsterList)
            {
                monster.GetComponent<Monster>().Hurt(1000);
            }
            waveSystem.monsterSpawner.MonsterList.Clear();
            waveSystem.currentWaveIndex = Mathf.Clamp(waveSystem.currentWaveIndex += 10, 0, 29);
            waveSystem.isWave = true;
            waveSystem.checkTime = 1.0f;
            waveSystem.StartWave();
            Debug.Log(waveSystem.currentWaveIndex + "asdfasfesafasefasef");
        }
    }
}
