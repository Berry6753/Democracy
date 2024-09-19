using UnityEngine;
using TMPro;

public class WaveSystem : MonoBehaviour
{
    private static int maxWave = 6;
    public float breakTime = 60.0f;
    [SerializeField] private Wave[] waves = new Wave[maxWave];               //현재 웨이브 정보
    public MonsterSpawner monsterSpawner;

    //[HideInInspector]

    [Header("Wave Count")]
    [SerializeField]
    public int currentWaveIndex = 0;

    [SerializeField]
    private GameObject[] monster = new GameObject[5];

    NormalMonster normal;
    AerialMonster aerial;
    SiegeMonster siege;
    SuicideMonster suicide;

    public bool isWave = false;

    public float checkTime;

    [Header("Wave Count UI")]
    public TMP_Text WaveCount_Text;

    [Header("Wave Timer UI")]
    [SerializeField]
    private TMP_Text WaveTimer;

    public float PlayTimer { get; private set; }

    public int LastWave { get { return monster.Length; } }

    private int normalMonsterCount = 20;
    private int AerialMonsterCount = 10;
    private int SiegeMonsterCount = 4;
    private int SuicideMonsterCount = 5;

    public bool isEndWave = false;

    private void OnEnable()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].spawnTime = 1;
            waves[i].maxMonsterCount = new int[4];
            waves[i].monsterPrefab = new GameObject[4];
        }
        normal = monster[0].GetComponent<NormalMonster>();
        aerial = monster[1].GetComponent<AerialMonster>();
        siege = monster[2].GetComponent<SiegeMonster>();
        suicide = monster[3].GetComponent<SuicideMonster>();
        for (int i = 0; i < monster.Length; i++)
        {
            for (int k = 0; k < waves.Length; k++)
            {
                waves[k].monsterPrefab[i] = monster[i];

                waves[k].maxMonsterCount[0] = normalMonsterCount + k * 8;
                waves[k].maxMonsterCount[1] = k >= 1 ? AerialMonsterCount + (k * 5) : 0;
                waves[k].maxMonsterCount[2] = k >= 2 ? SiegeMonsterCount + (k - 1) : 0;
                waves[k].maxMonsterCount[3] = k >= 3 ? SuicideMonsterCount + (k - 2) * 5 : 0;

                //if (k >= 0 && k < 10)
                //{
                //    waves[k].maxMonsterCount[0] = normal.startSpawnNum;
                //    waves[k].maxMonsterCount[1] = aerial.startSpawnNum;
                //    waves[k].maxMonsterCount[2] = 0;
                //    waves[k].maxMonsterCount[3] = suicide.startSpawnNum;
                //    if (k >= 5)
                //    {
                //        waves[k].maxMonsterCount[2] = siege.startSpawnNum;
                //    }
                //}
                //else if (k >= 10 && k < 20)
                //{
                //    waves[k].maxMonsterCount[0] = normal.startSpawnNum + normal.upScaleSpwanNum;
                //    waves[k].maxMonsterCount[1] = aerial.startSpawnNum + aerial.upScaleSpwanNum;
                //    waves[k].maxMonsterCount[2] = siege.startSpawnNum + siege.upScaleSpwanNum;
                //    waves[k].maxMonsterCount[3] = suicide.startSpawnNum + siege.upScaleSpwanNum;
                //}
                //else if (k >= 20 && k < waves.Length)
                //{
                //    waves[k].maxMonsterCount[0] = normal.startSpawnNum + normal.upScaleSpwanNum * 2;
                //    waves[k].maxMonsterCount[1] = aerial.startSpawnNum + aerial.upScaleSpwanNum * 2;
                //    waves[k].maxMonsterCount[2] = siege.startSpawnNum + siege.upScaleSpwanNum * 2;
                //    waves[k].maxMonsterCount[3] = suicide.startSpawnNum + siege.upScaleSpwanNum * 2;
                //}
            }
        }
        currentWaveIndex = 0;
        checkTime = breakTime;
        PlayTimer = 0;
        isWave = true;
        monsterSpawner.MonsterList.Clear();
        WaveCount_Text.text = $"{currentWaveIndex + 1} Wave";
    }

    private void OnDisable()
    {
        GameManager.Instance.SaveDataToJson();
    }

    //private void Start()
    //{
    //    normal = monster[0].GetComponent<NormalMonster>();
    //    aerial = monster[1].GetComponent<AerialMonster>();
    //    siege = monster[2].GetComponent<SiegeMonster>();
    //    suicide = monster[3].GetComponent<SuicideMonster>();
    //    for (int i = 0; i < monster.Length; i++)
    //    {
    //        for (int k = 0; k < waves.Length; k++)
    //        { 
    //            waves[k].monsterPrefab[i] = monster[i];
    //            if (k >= 0 && k < 10)
    //            {
    //                waves[k].maxMonsterCount[0] = normal.startSpawnNum;
    //                waves[k].maxMonsterCount[1] = aerial.startSpawnNum;
    //                waves[k].maxMonsterCount[2] = 0;
    //                waves[k].maxMonsterCount[3] = suicide.startSpawnNum;
    //                if (k >= 5)
    //                {
    //                    waves[k].maxMonsterCount[2] = siege.startSpawnNum;
    //                }
    //            }
    //            else if (k >= 10 && k < 20)
    //            {
    //                waves[k].maxMonsterCount[0] = normal.startSpawnNum + normal.upScaleSpwanNum;
    //                waves[k].maxMonsterCount[1] = aerial.startSpawnNum + aerial.upScaleSpwanNum;
    //                waves[k].maxMonsterCount[2] = siege.startSpawnNum + siege.upScaleSpwanNum;
    //                waves[k].maxMonsterCount[3] = suicide.startSpawnNum + siege.upScaleSpwanNum;
    //            }
    //            else if (k >= 20 && k < waves.Length)
    //            {
    //                waves[k].maxMonsterCount[0] = normal.startSpawnNum + normal.upScaleSpwanNum * 2;
    //                waves[k].maxMonsterCount[1] = aerial.startSpawnNum + aerial.upScaleSpwanNum * 2;
    //                waves[k].maxMonsterCount[2] = siege.startSpawnNum + siege.upScaleSpwanNum * 2;
    //                waves[k].maxMonsterCount[3] = suicide.startSpawnNum + siege.upScaleSpwanNum * 2;
    //            }
    //        }
    //    }
    //    checkTime = breakTime;
    //    waveCount = 0;
    //    isWave = true;
    //}

    private void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            //게임 오버 화면

            return;
        }

        PlayTimer += Time.deltaTime;

        if (!isWave)
        {
            StartWave();
        }
        else
        {
            if (monsterSpawner.MonsterList.Count == 0 && currentWaveIndex < waves.Length)
            {
                WaveTimer.transform.parent.gameObject.SetActive(true);
                if (checkTime == breakTime)
                {
                    GameManager.Instance.playerInfo.AddGearCount(300);
                }
                checkTime -= Time.deltaTime;
                if (checkTime <= 0)
                {
                    checkTime = breakTime;
                    isWave = false;
                }
                else WaveTimer.text = $"{(int)checkTime}";
            }
        }
    }


    public void StartWave()
    {
        WaveTimer.transform.parent.gameObject.SetActive(false);
        isWave = true;
        WaveCount_Text.text = $"{currentWaveIndex + 1} Wave";
        if (monsterSpawner.MonsterList.Count == 0 && currentWaveIndex < waves.Length)
        {
            monsterSpawner.StartWave(waves[currentWaveIndex]);
            currentWaveIndex++;
        }
    }



    //private IEnumerator BreakWave(float time)
    //{
    //    if (monsterSpawner.MonsterList.Count == 0 && currentWaveIndex < waves.Length)
    //    {
    //        yield return new WaitForSeconds(time);
    //        isWave = false;
    //    }
    //}
}

[System.Serializable]
public class Wave
{
    public float spawnTime;               //현재 웨이브 적 생성 주기
    public int[] maxMonsterCount;         //현재 웨이브 적 등장 숫자
    public GameObject[] monsterPrefab;    //몬스터 프리팹  
}