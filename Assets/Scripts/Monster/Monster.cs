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
    [SerializeField] protected float startSpwanNum;     //�ʱ� ���� ��
    [SerializeField] protected float upScaleSpwanNum;   //���� ���� �� 
    [SerializeField] protected float spawnTiming;       //���� ����
 

    [SerializeField] protected float sensingRange;      //���� ����
    protected int turretIndex = 0;
    protected Collider attack;                          //���� �ݶ��̴�

    protected Transform monsterTr;                      //���� ��ġ
    protected Transform defaltTarget;                   //�⺻ Ÿ��
    protected Transform chaseTarget;
    [SerializeField] protected LayerMask turretLayer;   //�ͷ����̾�
    [SerializeField] protected LayerMask monsterLayer;  //���ͷ��̾�

    protected int wave;                   
    
    protected Rigidbody rb;
    protected NavMeshAgent nav;
    protected Animator anim;
    protected StateMachine stateMachine;

    protected readonly int hashTrace = Animator.StringToHash("isTrace");
    protected readonly int hashAttack = Animator.StringToHash("isAttack");
    protected readonly int hashGetHit = Animator.StringToHash("isGetHit");
    protected readonly int hashDie = Animator.StringToHash("isDie");

    public enum State
    { IDLE, TRACE, ATTACK, DIE, GETHIT, SPAWN }
    public State state = State.IDLE;

    protected bool canAttack = true;
    [HideInInspector] public bool isDead = false;                    
    //���� ����
 
    protected virtual void Awake()
    {
        monsterTr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        stateMachine = gameObject.AddComponent<StateMachine>();


        stateMachine.AddState(State.IDLE, new IdleState(this));
        stateMachine.AddState(State.TRACE, new TraceState(this));
        stateMachine.AddState(State.ATTACK, new AttackState(this));
        stateMachine.AddState(State.GETHIT, new GetHitState(this));
        stateMachine.AddState(State.DIE, new DieState(this));
        stateMachine.InitState(State.IDLE);
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
                    attack.enabled = true;
                }
                else
                {
                    stateMachine.ChangeState(State.IDLE);
                    attack.enabled = false;
                }
            }
            else
            {
                stateMachine.ChangeState(State.TRACE);
                attack.enabled = false;
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
            chaseTarget = turret[turretIndex].transform;
        }
        else
        {
            chaseTarget = defaltTarget;
        }
    }

    protected void NextTarget()
    {
        Collider[] monster = Physics.OverlapBox(transform.position, new Vector3(5f, 5f, 5f), transform.rotation, monsterLayer);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(5f, 5f, 5f));
    }

    protected void turretDistance(Collider[] array)
    {
        float[] distance = new float[array.Length];
        int minIndex = 0;
        for (int i = 0; i < array.Length; i++)
        {
            distance[i] = Vector3.Distance(array[i].transform.position, monsterTr.position);
        }
        float minDistance = distance[0];
        for (int i = 0; i < distance.Length; i++)
        {
            if (distance[i] < minDistance)
            {
                minDistance = distance[i];
                minIndex = i;
            }
        }
        turretIndex = minIndex;
    }


    protected void FreezeVelocity()                     //������ ����
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    protected abstract void SpawnTiming();              //���� Ÿ�̹�

    protected void UpScaleHp()                          //ü�� ������
    {
        hp += upScaleHp * wave;
    }

    protected virtual void UpScaleDamage()              //������ ������
    {
        damage += upScaleDamage * wave;
    }

    protected void UpScaleSpawn()                       //���� ������
    {
        if (wave % 10 == 0 && wave / 10 > 0)
        {
            startSpwanNum += upScaleSpwanNum;
        }
    }

    public void Hurt(float damage)                   //�÷��̾�� ������ ���� ��
    { 
        hp -= damage;
        stateMachine.ChangeState(State.GETHIT);
    }

    public void isDie()                              //�׾��� ��
    { 
        isDead = true;
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
    protected class GetHitState : BaseMonsterState
    { 
        public GetHitState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetTrigger(owner.hashGetHit);
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