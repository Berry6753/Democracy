using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public enum PlayerSkillName
{
    BARRICATE,
    HEALLING,
    NUKE,

    LAST
}


public class Core : MonoBehaviour
{
    public static Core instance;

    public int repairCost = 100;
    public int upgradeCost = 50;
    public bool isUpgrade = true;
    public bool isUpgrading = false;
    public bool isReloading = false;

    [SerializeField]
    private GameObject itemSpawnPos;

    private int upgradeCoolTimeRise = 10;
    private int nowHp;
    private int maxHp = 100;
    private int hpRise = 30;
    private int realoadCoolTime = 30;
    private int upgradeCoolTime = 30;
    private int upgradeCostRise = 2;
    private int nowUpgradeCount;
    private int maxUpgradeCount = 5;
    private float checkReloadingTime;
    private float checkUpgradeTime;
    private int checkCount;
    public Player_Info player;
    public Player_Command playerCommand;
    public bool isPlayer;

    private Dictionary<int, int[]> commandDic = new Dictionary<int, int[]>();
    [SerializeField]
    private GameObject[] skillObj;

    private Queue<GameObject>[] skillObjQue = new Queue<GameObject>[(int)PlayerSkillName.LAST];


    private int itemKey;

    private void Awake()
    {
        instance = this;
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
        player = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Player_Info>();
        playerCommand = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Player_Command>();
        InitCommandDic();
        isReloading = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        nowHp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isReloading)
        {
            checkReloadingTime += Time.deltaTime;
            if (checkReloadingTime >= realoadCoolTime)
            {
                isReloading = true;
                checkReloadingTime = 0;
            }
        }

        if (isUpgrading)
        {
            Debug.Log("���׷��̵� ��...");
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

        //�ı��ɽ� ���ӸŴ����� �ھ������ ���ϴ� ������ false��
    }

    public void Reloading()
    {
        if (isReloading && player != null)
        {
            //�÷��̾��� źâ�� ä����
            player.equipedBulletCount = player.maxEquipedBulletCount;
            player.magazineCount = player.maxMagazineCount;
            player.GetComponent<Player_Info_UI>().Reload(player.equipedBulletCount, player.magazineCount);
            isReloading = false;
        }
    }

    public void Hurt(int damge)
    {
        nowHp -= damge;
    }

    public void Repair()
    {
        nowHp = maxHp;
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
        commandDic.Add((int)PlayerSkillName.NUKE, new int[4] { 1, 1, 1, 2 });
        
    }

    public bool CheckeCommand()
    {
        foreach (int i in commandDic.Keys)
        {
            commandDic.TryGetValue(i, out int[] Value);
            if (!Enumerable.SequenceEqual(Value, playerCommand.commandQueue.ToArray()))
            {
                itemKey = -1;
            }
            else
            {
                itemKey = i;
                break;
            }
            //for (int j = 0; j < Value.Length; j++) 
            //{
                
            //    if(!(Value[j] == playerMovement.commandQueue.ToArray()[j]))
            //    {
            //        itemKey = -1;
            //        checkCount = 0;
            //        break;
            //    }
            //    else
            //    {
            //        checkCount++;
                    
            //    }

               

            //}
            //if (checkCount == 4)
            //{
            //    itemKey = i;
            //    checkCount = 0;
            //    break;
            //}
            //if (playerMovement.commandQueue.ToArray()==Value)
            //{
            //    itemKey = i;
            //    Debug.Log(itemKey + "asdad");
            //    break;
            //}
            //else
            //{
            //    Debug.Log((playerMovement.commandQueue.ToArray() == Value) + "gggggggggg");
            //    for (int j = 0; j < Value.Length; j++)
            //    {
            //        Debug.Log(Value[j] + $"aaa{j}aaa");
            //        Debug.Log(playerMovement.commandQueue.ToArray()[j] + $"sssss{j}sssss");

            //    }
            //    Debug.Log(itemKey + "a");
                
            //}
        }

        if (itemKey == -1 || itemKey == (int)PlayerSkillName.LAST)
        {
            Debug.Log("Ŀ�ǵ� Ʋ��");
            return false;
        }
        else if(player.GearCount < skillObj[itemKey].GetComponent<Skill_Item_Info>().Count)
        {
            Debug.Log("��� �� ���ڶ�");
            return false;
        }
        else
        {
            player.UseGear(skillObj[itemKey].GetComponent<Skill_Item_Info>().Count);            
            GameObject gameObject = ItemObjectPool.SpawnFromPool(skillObj[itemKey].name, itemSpawnPos.transform.position);
            //GameObject gameObject = skillObjQue[itemKey].Dequeue();
            //skillObjQue[itemKey].Enqueue(gameObject);
            //gameObject.SetActive(true);
            //gameObject.transform.position = itemSpawnPos.transform.position;
            gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 10, 10));
            return true;
        }
    }

}
