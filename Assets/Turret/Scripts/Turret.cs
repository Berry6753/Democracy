using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Turret : MonoBehaviour
{
    private float searchTime = 0.5f;
    private float checkSearchTime;
    private float attackTime;

    [SerializeField]
    private Mesh[] turretHeadMesh;
    [SerializeField]
    private Mesh[] turretBodyMesh;
    private MeshFilter headMeshFilter;
    private MeshFilter bodyMeshFilter;

    [SerializeField]
    private GameObject turretHead;
    [SerializeField]
    private GameObject turretBody;

    [SerializeField]
    protected GameObject firePos;
    [SerializeField]
    private GameObject spinPos;

    protected Transform targetTransform;
    protected LayerMask monsterLayer = 6;

    private int nowUpgradeCount;
    private int nowHp;
    private int maxHp;
    private int hpRise;
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


    public bool isUpgrade;
    public bool isRepair;
    public bool isTarget;

    
    public float turretRepairCost { get { return repairCost; } }
    public float turretUpgradCost { get { return upgradeCost; } }
    public float turretMakingCost { get { return makingCost; } }


    private void Awake()
    {
        bodyMeshFilter = turretBody.GetComponent<MeshFilter>();
        headMeshFilter = turretHead.GetComponent<MeshFilter>();
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        if (targetTransform != null)
        {
            attackTime += Time.time;
            spinPos.transform.LookAt(targetTransform);
            if (attackTime >= attackSpeed)
            {
                Attack();
            }
        }
        else
        {
            attackTime = 0;
        }
    }
    protected abstract void Attack();
    protected void SetTurret(float mainkgTime, float makingCost, float attackDamge, float attackSpeed, float attackRange, int maxHp, int hpRise, float upgradeCost, float upgradeTime, float repairTime, float repairCost, float attackRise, float attackSpeedRise, float upgradCostRise, float maxUpgradeCount)
    {
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
    }
                                                                                                                                                                
    //�ڷ�ƾ�� �������÷��Ͱ� ���� �Ҹ���                                                                                                                       
    //�޸𸮸� ���� �Դ´ٴ� ���̴�                                                                                                                             
    //��ž�� ������ �����̴� �ڷ�ƾ ���� �����غ���                                                                                                             
    //������ �Լ��� ������Ʈ���� ȣ���� ���ϸ鼭 �ϴ°ͺ��� ����                                                                                              
    //������ �̺�Ʈ�� �̿��� ����� ����
    //���� ������ �̿��ؼ� �����ϴ°Ŷ� �����̷��� �����غ���
    protected IEnumerator SearchEnemy()
    {
        while (true)
        {
            yield return new WaitUntil(() => targetTransform == null);
            yield return new WaitForSeconds(1);

            Collider[] enemyCollider = Physics.OverlapSphere(transform.position, attackRange, monsterLayer);//���̾� ����ũ ���� �߰�
            Transform nierTargetTransform = null;
            if (enemyCollider.Length > 0)
            {
                float nierTargetDistance = Mathf.Infinity;
                foreach (Collider collider in enemyCollider)
                {
                    float distance = Vector3.SqrMagnitude(transform.position - collider.transform.position);

                    if (/*collider.GetComponent<Monster>().isDead!=null&&*/distance < nierTargetDistance)
                    {
                        nierTargetDistance = distance;
                        nierTargetTransform = collider.transform;
                    }
                }
            }

            targetTransform = nierTargetTransform;

        }

    }

    public void Hurt(int damge)
    {
        nowHp -= damge;
    }

    public virtual void Upgrade()
    {
        nowUpgradeCount++;
        upgradeCost *= upgradCostRise;
        attackDamge *= attackRise;
        attackSpeed += attackSpeedRise;
        maxHp += hpRise;
        nowHp += hpRise;

        //���׷��̵�� ��������
        headMeshFilter.mesh = turretHeadMesh[nowUpgradeCount];
        bodyMeshFilter.mesh = turretBodyMesh[nowUpgradeCount];
    }

    public void Repair()
    {
        nowHp = maxHp;
    }
}
