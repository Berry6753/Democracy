using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum TurretStateName
{
    BLUESCREEN,MAKING,SEARCH,ATTACK,UPGRADE,REPAIR,DESTROIY
}


public abstract class Turret : MonoBehaviour
{
    [SerializeField]
    private GameObject hpEffects;

    private float attackTime;

    [SerializeField]
    private Mesh[] turretHeadMesh;
    [SerializeField]
    private Mesh[] turretBodyMesh;
    private MeshFilter headMeshFilter;
    private MeshFilter bodyMeshFilter;
    private MeshRenderer headRenderer;
    private MeshRenderer bodyRenderer;

    private Player_BuildSystem player;

    [SerializeField]
    private GameObject turretHead;
    [SerializeField]
    private GameObject turretBody;

    [SerializeField]
    protected GameObject fireEffectPos;
    [SerializeField]
    protected GameObject firePos;

    protected Transform targetTransform;

    protected LayerMask monsterLayer;
    private LayerMask turretLayer;

    protected Dictionary<GameObject, float> targetCollider = new Dictionary<GameObject, float>();
    protected List<GameObject> targetList = new List<GameObject>();
    public int targetIndex;

    private int nowUpgradeCount;
    private int nowHp;
    private int maxHp;
    private int hpRise;
    private int nowMaxHp;
    private float makingTime;
    private float attackDamge;
    private float attackSpeed;
    private float attackRange;
    private float upgradeTime;
    private float repairTime;
    private float attackRise;
    private float attackSpeedRise;
    private float upgradCostRise;
    private float maxUpgradeCount;
    private float repairCost;
    private float upgradeCost;
    private float makingCost;


    private float nowUpgradeCost;
    private float nowAttackDamge;
    private float nowAttackSpeed;

    private bool isUpgrade;
    private bool isRepair;

    public Slider sliderGage;
    public AudioSource fireAudio;
    [HideInInspector]
    public AudioSource makeAudio;
    [HideInInspector]
    public AudioSource repairAudio;

    [HideInInspector]
    public SphereCollider turretCollider;

    public GameObject spinPos;
    [HideInInspector]
    public StateMachine turretStatemachine;

    public bool isTarget;
    public bool isMake;

    public TurretStateName turretStateName;
    public GameObject fireEfect;
    public GameObject makingEfect;
    public GameObject deathEffect;
    [HideInInspector]
    public ParticleSystem firePaticle;
    protected LayerMask ignoreLayer;
    public Transform turretTargetTransform { get { return targetTransform; } set { targetTransform = value; } }
    public Transform snipeTurretFirePos { get { return firePos.transform; } }
    public List<GameObject> turretTargetList { get { return targetList; } }
    public float turretAttackDamge { get { return nowAttackDamge; } }
    public float turretAttackRange { get { return attackRange; } }
    public float turretAttackSpeed { get { return nowAttackSpeed; } }
    public float turretMakingTime { get { return makingTime; } }
    public float turretRepairTime { get { return repairTime; } }
    public float turretUpgradeTime { get { return upgradeTime; } }
    public float turretUpgradeCount { get { return nowUpgradeCount; } }
    public float turretRepairCost { get { return repairCost; } }
    public float turretUpgradCost { get { return nowUpgradeCost; } }
    public float turretMakingCost { get { return makingCost; } }
    public bool isTurretUpgrade { get { return isUpgrade; } }
    public bool isTurretRepair { get { return isRepair; } }


    protected virtual void Awake()
    {
        player=GameManager.Instance.GetPlayer.transform.GetComponent<Player_BuildSystem>();
        bodyMeshFilter = turretBody.GetComponent<MeshFilter>();
        headMeshFilter = turretHead.GetComponent<MeshFilter>();
        bodyRenderer = turretBody.GetComponent<MeshRenderer>();
        headRenderer = turretHead.GetComponent<MeshRenderer>();
        turretCollider = transform.GetComponent<SphereCollider>();
        gameObject.AddComponent<StateMachine>();
        turretStatemachine = GetComponent<StateMachine>();
        SetState();
        turretStatemachine.InitState(TurretStateName.BLUESCREEN);
        firePaticle= fireEfect.GetComponent<ParticleSystem>();
        makeAudio = makingEfect.GetComponent<AudioSource>();
        repairAudio = GetComponent<AudioSource>();
        turretLayer = LayerMask.NameToLayer("Turret");
        monsterLayer = LayerMask.NameToLayer("Monster");
        ignoreLayer = 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("CheckZone") | 1 << LayerMask.NameToLayer("Gear");
    }

    protected virtual void OnEnable()
    {
        turretStatemachine.ChangeState(TurretStateName.BLUESCREEN);
        makingEfect.SetActive(false);
        fireEfect.SetActive(false);
        sliderGage.gameObject.SetActive(false);
        deathEffect.SetActive(false);
        HpEffect();
    }

   

    private void Update()
    {
        if(turretStateName != TurretStateName.DESTROIY)
        {
            UpgradeCheck();
            RepairCheck();
            HpEffect();

            if (nowHp <= 0)
            {
                turretStatemachine.ChangeState(TurretStateName.DESTROIY);
            }
        }   
    }
    private void HpEffect()
    {

        if(turretStateName==TurretStateName.ATTACK|| turretStateName == TurretStateName.SEARCH|| turretStateName == TurretStateName.REPAIR)
        {
            if(nowHp > maxHp * 0.75f)
            {
                for (int i = 0; i < hpEffects.transform.childCount; i++)
                {
                    hpEffects.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = hpEffects.transform.childCount; i > 0; i--)
                {
                    if (nowHp <= maxHp * (0.25f * i))
                    {
                        hpEffects.transform.GetChild(i - 1).gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < hpEffects.transform.childCount; i++)
            {
                hpEffects.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
    private void SetState()
    {
        turretStatemachine.AddState(TurretStateName.BLUESCREEN,new TurretBlueScreenState(this));
        turretStatemachine.AddState(TurretStateName.SEARCH, new TurretSearchState(this));
        turretStatemachine.AddState(TurretStateName.ATTACK, new TurretAttackState(this));
        turretStatemachine.AddState(TurretStateName.UPGRADE, new TurretUpgradeState(this));
        turretStatemachine.AddState(TurretStateName.MAKING, new TurretMakingState(this));
        turretStatemachine.AddState(TurretStateName.REPAIR, new TurretRepairState(this));
        turretStatemachine.AddState(TurretStateName.DESTROIY, new TurretDestroyState(this));

    }

    private void RepairCheck()
    {
        if (turretStateName == TurretStateName.MAKING || turretStateName == TurretStateName.REPAIR || turretStateName == TurretStateName.UPGRADE) 
        {
            isRepair = false;
        }
        else
        {
            isRepair = true;
        }
    }

    private void UpgradeCheck()
    {
        if (!isRepair || nowUpgradeCount >= maxUpgradeCount-1)
        {
            isUpgrade = false;
        }
        else
        {
            isUpgrade = true;
        }
    }


    public abstract void Attack();
    protected void SetTurret(float mainkgTime, float makingCost, float attackDamge, float attackSpeed, float attackRange, int maxHp, int hpRise, float upgradeCost, float upgradeTime, float repairTime, float repairCost, float attackRise, float attackSpeedRise, float upgradCostRise, float maxUpgradeCount)
    {
        this.makingTime = mainkgTime;
        this.maxHp = maxHp;
        this.nowHp = maxHp;
        this.hpRise = hpRise;
        this.makingCost = makingCost;
        this.attackDamge = attackDamge;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.upgradeCost = upgradeCost;
        this.upgradeTime = upgradeTime;
        this.repairTime = repairTime;
        this.repairCost = repairCost;
        this.attackRise = attackRise;
        this.attackSpeedRise = attackSpeedRise;
        this.upgradCostRise= upgradCostRise;
        this.maxUpgradeCount = maxUpgradeCount;

        nowUpgradeCount = 0;
        nowUpgradeCost = upgradeCost;
        nowAttackDamge = attackDamge;
        nowAttackSpeed = attackSpeed;
        nowMaxHp = maxHp;

        headMeshFilter.mesh = turretHeadMesh[nowUpgradeCount];
        bodyMeshFilter.mesh = turretBodyMesh[nowUpgradeCount];
    }

    public void ChangeColor()
    {
        Collider[] turretColliders = Physics.OverlapSphere(transform.position, 5, (1 << turretLayer));

        if (turretColliders.Length > 0 || !player.isGearCountOk)  
        {
            isMake = false;
            bodyRenderer.material.color = Color.red;
            headRenderer.material.color = Color.red;
        }
        else
        {

            isMake = true;
            bodyRenderer.material.color = Color.blue;
            headRenderer.material.color = Color.blue;
        }
    }

    public void ResetColor()
    {
        bodyRenderer.material.color = Color.white;
        headRenderer.material.color = Color.white;
    }

    public void OnRenderer()
    {
        bodyRenderer.enabled = true;
        headRenderer.enabled = true;
    }
    public void OffRenderer()
    {
        bodyRenderer.enabled = false;
        headRenderer.enabled = false;
    }

    //코루틴은 가비지컬렉터가 많이 불린다                                                                                                                       
    //메모리를 많이 먹는다는 뜻이다                                                                                                                             
    //포탑이 많아질 예정이니 코루틴 없이 구현해보자                                                                                                             
    //하지만 함수를 업데이트에서 호출해 비교하면서 하는것보단 좋다                                                                                              
    //공격은 이벤트를 이용해 만들어 보자
    //상태 패턴을 이용해서 삭제하는거랑 공격이런거 정리해보자
    public void SearchEnemy()
    {
        targetCollider.Clear();
        targetList.Clear();
        targetIndex = 0;
        Collider[] enemyCollider = Physics.OverlapSphere(transform.position, attackRange, (1 << monsterLayer));//레이어 마스크 몬스터 추가
        //Transform nierTargetTransform = null;
        if (enemyCollider.Length > 0)
        {
            //float nierTargetDistance = Mathf.Infinity;
            foreach (Collider collider in enemyCollider)
            {
                if ((collider.CompareTag("Monster")|| collider.CompareTag("Boss")) && !targetCollider.ContainsKey(collider.gameObject))
                {
                    float distance = Vector3.SqrMagnitude(transform.position - collider.transform.position);

                    targetCollider.Add(collider.gameObject, distance);
                    //if (/*!collider.GetComponent<Monster>().isDead&&*/distance < nierTargetDistance)
                    //{
                    //    nierTargetDistance = distance;
                    //    nierTargetTransform = collider.transform;
                    //}
                }
                
            }

            var soltDic = targetCollider.OrderBy(x => x.Value);


            foreach (var nearTarget in soltDic)
            {
                targetList.Add(nearTarget.Key);
            }

            if (targetList.Count > 0)
            {
                targetTransform = targetList[targetIndex].transform;
                turretStatemachine.ChangeState(TurretStateName.ATTACK);

            }

        }

        

    }

    public void TurretMake()
    {
        turretStatemachine.ChangeState(TurretStateName.MAKING);
    }

    public void TurretRepair()
    {
        turretStatemachine.ChangeState(TurretStateName.REPAIR);
    }

    public void TurretUpgrade()
    {
        //상태 업그레이드로
        if (isUpgrade)
            turretStatemachine.ChangeState(TurretStateName.UPGRADE);
    }

    public void Hurt(int damge)
    {
        nowHp -= damge;
    }

    public virtual void Upgrade()
    {
        nowUpgradeCount++;
        nowUpgradeCost *= upgradCostRise;
        nowAttackDamge *= attackRise;
        nowAttackSpeed += attackSpeedRise;
        nowMaxHp += hpRise;
        nowHp += hpRise;

        //업그레이드시 외형변경
        headMeshFilter.mesh = turretHeadMesh[nowUpgradeCount];
        bodyMeshFilter.mesh = turretBodyMesh[nowUpgradeCount];

        Debug.Log($"{transform.name} : {nowUpgradeCount} : {maxUpgradeCount}");
    }

    public void Repair()
    {
        nowHp = nowMaxHp;
    }

   

}
