using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public abstract class Monster : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] protected string monsterName;

    public string GetMonsterName {  get { return monsterName; } }

    [Header("���� ����")]
    protected float time = 0;
    [SerializeField] protected float attackSpeed;       //���� �ӵ�
    [SerializeField] protected float hp;                //ü��
    [SerializeField] protected float damage;            //���ݷ�
    [SerializeField] protected float hitNum;            //Ÿ�� Ƚ��
    [SerializeField] protected float attackRange;       //��Ÿ�
    [Header("���� ����ġ")]
    [SerializeField] protected float upScaleHp;         //ü�� ����ġ
    [SerializeField] protected float upScaleDamage;     //���ݷ� ����ġ
    [Header("���� ����")]
    [SerializeField] public int startSpawnNum;        //�ʱ� ���� ��
    [SerializeField] public int upScaleSpwanNum;      //���� ���� �� 
    [SerializeField] protected int spawnTiming;       //���� ����

    [SerializeField] protected float sensingRange;      //���� ����
    protected int turretIndex = 0;                      //���� ����� �ͷ��ε���
    protected int secondTurretIndex = 0;                //�� ��° ����� �ͷ��ε���
    protected int targetingIndex = 0;                   //Ÿ������ ���� �ͷ��ε���
    protected Collider[] attack;                          //���� �ݶ��̴�

    protected Transform monsterTr;                      //���� ��ġ
    protected Transform defaultTarget;                  //�⺻ Ÿ��
    protected Transform chaseTarget;
    [SerializeField] protected LayerMask turretLayer;   //�ͷ����̾�
    [SerializeField] protected LayerMask monsterLayer;  //���ͷ��̾�
    [SerializeField] protected LayerMask dieLayer;

    protected int probabilityGetGear;
    protected int probabilityNum;
    public int dropGearNum = 0;
    protected int _wave = 0;
    [HideInInspector]
    public int wave
    { 
        get { return _wave; }
        set 
        {
            if (_wave != value)
            { 
                _wave = value;
                UpScaleHp();
                UpScaleDamage();
            }  
        }
        
    }

    
    protected Rigidbody rb;
    protected NavMeshAgent nav;
    protected Animator anim;
    protected StateMachine stateMachine;

    protected readonly int hashTrace = Animator.StringToHash("isTrace");
    protected readonly int hashAttack = Animator.StringToHash("isAttack");
    protected readonly int hashDie = Animator.StringToHash("isDie");

    public enum State
    { IDLE, TRACE, ATTACK, DIE}
    public State state = State.IDLE;

    protected bool canAttack = true;
    [HideInInspector] public bool isDead = false;
    //���� ����

    public event Action<Monster> OnDeath;
    protected WaveSystem waveSystem;
 
    protected virtual void Awake()
    {
        monsterTr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        attack = GetComponentsInChildren<SphereCollider>();
        foreach (Collider c in attack)
        {
            c.enabled = false;
        }
        GameObject waveObject = GameObject.Find("WaveSystem");
        waveSystem = waveObject.GetComponent<WaveSystem>();

        stateMachine = gameObject.AddComponent<StateMachine>();

        stateMachine.AddState(State.IDLE, new IdleState(this));
        stateMachine.AddState(State.TRACE, new TraceState(this));
        stateMachine.AddState(State.ATTACK, new AttackState(this));
        stateMachine.AddState(State.DIE, new DieState(this));
        stateMachine.InitState(State.IDLE);
    }

    protected void OnEnable()
    {
        gameObject.layer = monsterLayer;
    }

    protected virtual void Update()
    {
        if (time > 0 && !anim.GetBool("isAttack"))
        {
            time -= Time.deltaTime;
            canAttack = false;
        }
        else if(time <= 0)
        {
            canAttack = true;
        }
        wave = waveSystem.currentWaveIndex - 1;
    }

    protected virtual void LookAt()
    {
        transform.LookAt(new Vector3(chaseTarget.position.x, transform.position.y, chaseTarget.position.z));
    }

    protected abstract void ChaseTarget();              //Ÿ�� ����

    protected virtual IEnumerator MonsterState()        //���� �ൿ ����
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(0.3f);
            if ( hp <= 0 /*state == State.DIE*/)
            {
                stateMachine.ChangeState(State.DIE);
                isDie();
                yield break;
            }

            float distance = Vector3.Distance(chaseTarget.position, monsterTr.position);
            if (distance <= attackRange)
            {
                FreezeVelocity();
                if (canAttack)
                {
                    stateMachine.ChangeState(State.ATTACK);
                    foreach (Collider c in attack)
                    {
                        c.enabled = true;
                    }
                }
                else
                {
                    stateMachine.ChangeState(State.IDLE);
                    foreach (Collider c in attack)
                    {
                        c.enabled = false;
                    }
                }
            }
            else
            {
                stateMachine.ChangeState(State.TRACE);
                foreach (Collider c in attack)
                {
                    c.enabled = false;
                }
            }
        }
    }

    protected void AttackEnd()
    {
        anim.SetBool("isAttack", false);
    }

    protected void AttackDelay()
    {
        time = attackSpeed;       
    }

    protected void PriorityTarget()                     //Ÿ�� �켱���� ����
    {
        Collider[] turret = Physics.OverlapSphere(transform.position, sensingRange, turretLayer);
        if (turret.Length > 0)
        {   
            turretDistance(turret);
            TargetingTurret();
            if (targetingIndex != -1)
            {
                chaseTarget = turret[targetingIndex].transform;
            }
            else if (targetingIndex == -1)
            {
                chaseTarget = defaultTarget;
            }
        }
        else
        {
            chaseTarget = defaultTarget;
        }
    }

    protected void turretDistance(Collider[] array)
    {
        float[] distance = new float[array.Length];
        int minIndex = 0;
        int secondMinIndex = -1;
        for (int i = 0; i < array.Length; i++)
        {
            distance[i] = Vector3.Distance(array[i].transform.position, monsterTr.position);
        }
        float minDistance = distance[0];
        float secondDistance = distance.Max();
        for (int i = 0; i < distance.Length; i++)
        {
            if (distance[i] < minDistance)
            {
                secondDistance = minDistance;
                secondMinIndex = minIndex;
                minDistance = distance[i];
                minIndex = i;
            }
            else if(distance[i] < secondDistance)
            {
                secondDistance = distance[i];
                secondMinIndex = i;
            }
        }
        turretIndex = minIndex;
        secondTurretIndex = secondMinIndex;
    }

    protected void TargetingTurret()
    {
        Collider[] monster = Physics.OverlapBox(transform.position + Vector3.forward * 2.5f, new Vector3(5f, 5f, 5f), transform.rotation, monsterLayer);
        if (monster.Length > 3)
        {
            targetingIndex = secondTurretIndex;
        }
        else
        {
            targetingIndex = turretIndex;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.forward * 2.5f, new Vector3(5f, 5f, 5f));
    }

    protected void FreezeVelocity()                     //������ ����
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    //protected abstract void SpawnTiming();              //���� Ÿ�̹�

    protected void UpScaleHp()                          //ü�� ������
    {
        hp += upScaleHp * wave;
    }

    protected virtual void UpScaleDamage()              //������ ������
    {
        damage += upScaleDamage * wave;
    }

    public void Hurt(float damage)                   //�÷��̾�� ������ ���� ��
    { 
        hp -= damage;
    }

    public virtual void isDie()                              //�׾��� ��
    { 
        isDead = true;
        RandomGear();
        gameObject.layer = dieLayer;
        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        MonsterObjectPool.ReturnToPool(gameObject);
    }

    protected void RandomGear()
    {
        probabilityGetGear = UnityEngine.Random.Range(0, 100);
        if (probabilityGetGear >= 0 && probabilityGetGear < 65)
        {
            probabilityNum = UnityEngine.Random.Range(0, 100);
            if (probabilityNum >= 0 && probabilityNum < 60)
            {
                dropGearNum = 1;
            }
            else if (probabilityNum >= 60 && probabilityNum < 80)
            {
                dropGearNum = 2;
            }
            else if (probabilityNum >= 80 && probabilityNum < 92)
            {
                dropGearNum = 3;
            }
            else if (probabilityNum >= 92 && probabilityNum < 97)
            {
                dropGearNum = 4;
            }
            else
            {
                dropGearNum = 5;
            }
        }
        else
        {
            dropGearNum = 0;
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            FreezeVelocity();
            ChaseTarget();
        }
    }
    protected void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            FreezeVelocity();
            ChaseTarget();
        }
    }

    protected class BaseMonsterState : BaseState
    {
        protected Monster owner;
        public BaseMonsterState(Monster owner)
        { this.owner = owner; }
    }
    protected class IdleState : BaseMonsterState
    {
        public IdleState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.isStopped = true;
            owner.anim.SetBool(owner.hashTrace, false);
        }
    }
    protected class TraceState : BaseMonsterState
    {
        public TraceState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.SetDestination(owner.chaseTarget.position);
            owner.nav.isStopped = false;
            owner.anim.SetBool(owner.hashTrace, true);
            owner.anim.SetBool(owner.hashAttack, false);
        }
    }
    protected class AttackState : BaseMonsterState
    {
        public AttackState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetBool(owner.hashAttack, true);
        }
    }
    protected class DieState : BaseMonsterState
    {
        public DieState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetTrigger(owner.hashDie);
        }
    }
}