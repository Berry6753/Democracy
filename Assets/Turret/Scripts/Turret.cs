using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


public enum TurretStateName
{
    BLUESCREEN,MAKING,SEARCH,ATTACK,UPGRADE,REPAIR,DESTROIY
}


public abstract class Turret : MonoBehaviour
{
    
    private float attackTime;

    [SerializeField]
    private Mesh[] turretHeadMesh;
    [SerializeField]
    private Mesh[] turretBodyMesh;
    private MeshFilter headMeshFilter;
    private MeshFilter bodyMeshFilter;
    private MeshRenderer headRenderer;
    private MeshRenderer bodyRenderer;



    [SerializeField]
    private GameObject turretHead;
    [SerializeField]
    private GameObject turretBody;

    [SerializeField]
    protected GameObject firePos;

    protected Transform targetTransform;

    protected LayerMask monsterLayer;
    private LayerMask turretLayer;

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
    [HideInInspector]
    public AudioSource fireAudio;
    [HideInInspector]
    public AudioSource makeAudio;

    [HideInInspector]
    public CapsuleCollider turretCollider;

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
        bodyMeshFilter = turretBody.GetComponent<MeshFilter>();
        headMeshFilter = turretHead.GetComponent<MeshFilter>();
        bodyRenderer = turretBody.GetComponent<MeshRenderer>();
        headRenderer = turretHead.GetComponent<MeshRenderer>();
        turretCollider = transform.GetComponent<CapsuleCollider>();
        gameObject.AddComponent<StateMachine>();
        turretStatemachine = GetComponent<StateMachine>();
        SetState();
        turretStatemachine.InitState(TurretStateName.BLUESCREEN);
        firePaticle= fireEfect.GetComponent<ParticleSystem>();
        fireAudio = fireEfect.GetComponent<AudioSource>();
        makeAudio = makingEfect.GetComponent<AudioSource>();
        turretLayer = LayerMask.NameToLayer("Turret");
        monsterLayer = LayerMask.NameToLayer("Monster");
        ignoreLayer = 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Player");
        
    }

    protected virtual void OnEnable()
    {
        turretStatemachine.ChangeState(TurretStateName.BLUESCREEN);
        makingEfect.SetActive(false);
        fireEfect.SetActive(false);
        sliderGage.gameObject.SetActive(false);
        deathEffect.SetActive(false);
    }

   

    private void Update()
    {
        if(turretStateName != TurretStateName.DESTROIY)
        {
            UpgradeCheck();
            RepairCheck();
        }
        if (nowHp <= 0)
        {
            turretStatemachine.ChangeState(TurretStateName.DESTROIY);
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
        if (!isRepair || nowUpgradeCount >= maxUpgradeCount)
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

        if (turretColliders.Length > 0) 
        {
            isMake = false;
            bodyRenderer.material.color = Color.red;
            headRenderer.material.color = Color.red;
            Debug.Log(turretColliders[0].name);
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

    //�ڷ�ƾ�� �������÷��Ͱ� ���� �Ҹ���                                                                                                                       
    //�޸𸮸� ���� �Դ´ٴ� ���̴�                                                                                                                             
    //��ž�� ������ �����̴� �ڷ�ƾ ���� �����غ���                                                                                                             
    //������ �Լ��� ������Ʈ���� ȣ���� ���ϸ鼭 �ϴ°ͺ��� ����                                                                                              
    //������ �̺�Ʈ�� �̿��� ����� ����
    //���� ������ �̿��ؼ� �����ϴ°Ŷ� �����̷��� �����غ���
    public void SearchEnemy()
    {

        Collider[] enemyCollider = Physics.OverlapSphere(transform.position, attackRange, (1 << monsterLayer));//���̾� ����ũ ���� �߰�
        Transform nierTargetTransform = null;
        if (enemyCollider.Length > 0)
        {
            float nierTargetDistance = Mathf.Infinity;
            foreach (Collider collider in enemyCollider)
            {
                float distance = Vector3.SqrMagnitude(transform.position - collider.transform.position);

                if (/*!collider.GetComponent<Monster>().isDead&&*/distance < nierTargetDistance)
                {
                    nierTargetDistance = distance;
                    nierTargetTransform = collider.transform;
                }
            }
        }


        targetTransform = nierTargetTransform;


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
        //���� ���׷��̵��
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

        //���׷��̵�� ��������
        headMeshFilter.mesh = turretHeadMesh[nowUpgradeCount];
        bodyMeshFilter.mesh = turretBodyMesh[nowUpgradeCount];
    }

    public void Repair()
    {
        nowHp = nowMaxHp;
    }

   

}
