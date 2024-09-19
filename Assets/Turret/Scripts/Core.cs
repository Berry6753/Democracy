using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum PlayerSkillName
{
    BARRICATE,
    HEALLING,
    NUKE,

    LAST
}


public class Core : MonoBehaviour
{
    public int repairCost = 100;
    public int upgradeCost = 50;
    public bool isUpgrade = true;
    public bool isUpgrading = false;
    public bool isReloading = true;

    [SerializeField]
    private GameObject chargeEffect;
    [SerializeField]
    private GameObject destroyEffect;

    [SerializeField]
    private MeshRenderer coreRederer;
    [SerializeField]
    private GameObject itemSpawnPos;

    private int upgradeCoolTimeRise = 10;
    public int nowHp { get; private set; }
    private int hp = 100;
    private int maxHp;
    public int GetMaxHP { get { return maxHp; } }
    private int hpRise = 30;
    public int GetHPRise { get { return hpRise; } }
    private int realoadCoolTime = 30;
    private int upgradeCoolTime = 30;
    private int upgradeCostRise = 2;
    public int getupgradeCostRise { get { return upgradeCostRise; } }
    private int nowUpgradeCount;
    private int maxUpgradeCount = 5;
    private float checkReloadingTime;
    private float checkUpgradeTime;
    private int checkCount;
    public Player_Info player;
    public Player_Command player_Command;
    private Command_Input_Mark commandInput;
    public bool isPlayer;

    private Dictionary<int, int[]> commandDic = new Dictionary<int, int[]>();
    [SerializeField]
    private GameObject[] skillObj;

    private Queue<GameObject>[] skillObjQue = new Queue<GameObject>[(int)PlayerSkillName.LAST];

    [Header("Core 체력 UI")]
    [SerializeField]
    private Image CoreHPBar;

    //[Header("Core 체력 Text")]
    //[SerializeField]
    //private TMP_Text CoreHPText;

    [Header("쿨타임 확인용 Icon")]
    [SerializeField]
    private Image ReloadCoolTimeIcon;

    [Header("Command Fail Text")]
    [SerializeField]
    private TMP_Text FailText;

    [Header("Command UI")]
    [SerializeField]
    private Canvas tabUI;
    public float upgradeTime { get { return checkUpgradeTime; } }

    private int itemKey;

    private bool isDestroy;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Player_Info>();
        player_Command = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Player_Command>();
        commandInput = GameManager.Instance.GetCoreUI.GetComponentInChildren<Command_Input_Mark>(includeInactive: true);
        InitCommandDic();
        //for (int i = 0; i < (int)PlayerSkillName.LAST; i++)
        //{
        //    skillObjQue[i] = new Queue<GameObject>();
        //    for (int j = 0; j < 10; j++)
        //    {
        //        GameObject gameObject = Instantiate(skillObj[i]);
        //        skillObjQue[i].Enqueue(gameObject);
        //        gameObject.SetActive(false);

        //    }
        //}
    }

    private void OnEnable()
    {
        nowHp = hp;
        maxHp = hp;
        checkReloadingTime = realoadCoolTime;
        ReloadCoolTimeIcon.fillAmount = 0;
        CoreHPBar.fillAmount = 1;
        coreRederer.enabled = true;
        chargeEffect.SetActive(false);
        destroyEffect.SetActive(false);
        isDestroy = false;
        tabUI.enabled = false;
        StartCoroutine(Repair());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDestroy)
        {
            if (!isReloading)
            {
                checkReloadingTime -= Time.deltaTime;
                if (checkReloadingTime <= 0)
                {
                    isReloading = true;
                    checkReloadingTime = realoadCoolTime;
                    ReloadCoolTimeIcon.fillAmount = 0;
                }
                else ReloadCoolTimeIcon.fillAmount = checkReloadingTime / realoadCoolTime;
            }

            if (isUpgrading)
            {
                // 보여주는 HP

                checkUpgradeTime += Time.deltaTime;
                if (checkUpgradeTime >= upgradeCoolTime)
                {
                    Upgrade();
                    checkUpgradeTime = 0;
                    isUpgrading = false;
                }
            }

            if (nowUpgradeCount >= maxUpgradeCount)
            {
                isUpgrade = false;
            }

            if (isPlayer)
            {
                tabUI.enabled = true;
            }
            else
            {
                tabUI.enabled = false;
            }
            //파괴될시 게임매니저의 코어가있음을 뜻하는 변수를 false로

            if (nowHp <= 0)
            {
                StopCoroutine(Repair());
                chargeEffect.SetActive(true);
                isDestroy = true;
            }
        }
        else
        {
            if (chargeEffect.activeSelf == false)
            {
                coreRederer.enabled = false;
                destroyEffect.SetActive(true);
                if (destroyEffect.GetComponent<ParticleSystem>().time >= 1.0f)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

        }
    }

    public void Reloading()
    {
        if (isReloading && player != null)
        {
            //플레이어의 탄창수 채워줌
            player.equipedBulletCount = player.maxEquipedBulletCount;
            player.magazineCount = player.maxMagazineCount;
            isReloading = false;
            player.GetComponent<Player_Info_UI>().Reload(player.equipedBulletCount, player.magazineCount);
        }
    }

    public void Hurt(int damge)
    {
        nowHp -= damge;
        CoreHPBar.fillAmount = (float)((float)nowHp / (float)maxHp);
        //CoreHPText.text = $"{nowHp} / {maxHp}";
    }

    public IEnumerator Repair()
    {
        while (true)
        {
            yield return new WaitUntil(() => nowHp < maxHp);
            yield return new WaitForSeconds(10);
            nowHp += 1;
            CoreHPBar.fillAmount = (float)((float)nowHp / (float)maxHp);
        }
    }

    public void Upgrade()
    {
        nowHp += hpRise;
        maxHp += hpRise;
        upgradeCost *= upgradeCostRise;
        upgradeCoolTime += upgradeCoolTimeRise;
        nowUpgradeCount++;
    }

    private void InitCommandDic()
    {
        commandDic.Add((int)PlayerSkillName.BARRICATE, new int[4] { 2, 4, 2, 4 });
        commandDic.Add((int)PlayerSkillName.HEALLING, new int[4] { 2, 2, 2, 1 });
        commandDic.Add((int)PlayerSkillName.NUKE, new int[4] { 1, 1, 1, 4 });

    }

    public bool CheckeCommand()
    {
        foreach (int i in commandDic.Keys)
        {
            commandDic.TryGetValue(i, out int[] Value);
            if (!Enumerable.SequenceEqual(Value, commandInput.commandQueue.ToArray()))
            {
                itemKey = -1;
            }
            else
            {
                itemKey = i;
                break;
            }
        }

        if (itemKey == -1 || itemKey == (int)PlayerSkillName.LAST)
        {
            FailText.text = "Invalid input value.";
            return false;
        }
        else
        {
            if (skillObj[itemKey].GetComponent<Skill_Item_Info>().Count <= player.GearCount)
            {
                GameObject gameObject = ItemObjectPool.SpawnFromPool(skillObj[itemKey].name, itemSpawnPos.transform.position);
                gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 10, 10));
                player.UseGear(gameObject.GetComponent<Skill_Item_Info>().Count);
                return true;
            }
            else
            {
                FailText.text = "Be short of gear.";
                return false;
            }
        }
    }


    public void OpenCommandUI()
    {
        //애니메이션 이벤트로 처리
        // Command UI On
        GameManager.Instance.GetCoreUI.SetActive(true);
        GameManager.Instance.isGameStop = 1f;
    }


    public void EndCommandMode()
    {
        GameManager.Instance.GetPlayerUI.SetActive(true);
        player_Command.isCommand = false;
    }
}
