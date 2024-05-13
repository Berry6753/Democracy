using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


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
    protected LayerMask monsterLayer = 9;

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

    public GameObject spinPos;


    public bool isUpgrade;
    public bool isRepair;
    public bool isTarget;
    public bool isMake;

    public TurretStateName turretStateName;

    public Transform turretTargetTransform { get { return targetTransform; } }
    public float turretAttackSpeed { get { return nowAttackSpeed; } }
    public float turretMakingTime { get { return makingTime; } }
    public float turretRepairCost { get { return repairCost; } }
    public float turretUpgradCost { get { return upgradeCost; } }
    public float turretMakingCost { get { return makingCost; } }


    private void Awake()
    {
        bodyMeshFilter = turretBody.GetComponent<MeshFilter>();
        headMeshFilter = turretHead.GetComponent<MeshFilter>();
        bodyRenderer = turretBody.GetComponent<MeshRenderer>();
        headRenderer = turretHead.GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Turret"))
        {
            isMake = false;
        }
        else
        {
            isMake = true;
        }
    }

    public abstract void Attack();
    protected void SetTurret(float mainkgTime, float makingCost, float attackDamge, float attackSpeed, float attackRange, int maxHp, int hpRise, float upgradeCost, float upgradeTime, float repairTime, float repairCost, float attackRise, float attackSpeedRise, float upgradCostRise, float maxUpgradeCount)
    {
        this.makingTime = mainkgTime * 60;
        this.maxHp = maxHp;
        this.nowHp = maxHp;
        this.hpRise = hpRise;
        this.makingCost = makingCost;
        this.attackDamge = attackDamge;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.upgradeCost = upgradeCost;
        this.upgradeTime = upgradeTime * 60;
        this.repairTime = repairTime * 60;
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
        if (isMake)
        {
            bodyRenderer.material.color = Color.blue;
            headRenderer.material.color = Color.blue;
        }
        else
        {
            bodyRenderer.material.color = Color.red;
            headRenderer.material.color = Color.red;
        }
    }

    public void ResetColor()
    {
        bodyRenderer.material.color = Color.white;
        headRenderer.material.color = Color.white;
    }
                                                                                                                                                                
    //�ڷ�ƾ�� �������÷��Ͱ� ���� �Ҹ���                                                                                                                       
    //�޸𸮸� ���� �Դ´ٴ� ���̴�                                                                                                                             
    //��ž�� ������ �����̴� �ڷ�ƾ ���� �����غ���                                                                                                             
    //������ �Լ��� ������Ʈ���� ȣ���� ���ϸ鼭 �ϴ°ͺ��� ����                                                                                              
    //������ �̺�Ʈ�� �̿��� ����� ����
    //���� ������ �̿��ؼ� �����ϴ°Ŷ� �����̷��� �����غ���
    public void SearchEnemy()
    {

        Collider[] enemyCollider = Physics.OverlapSphere(transform.position, attackRange, monsterLayer);//���̾� ����ũ ���� �߰�
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
