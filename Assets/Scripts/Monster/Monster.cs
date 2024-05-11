using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Monster : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] protected string monsterName;      //���� ����
    [SerializeField] protected float attactSpeed;       //���� �ӵ�
    [SerializeField] protected float hp;                //ü��
    [SerializeField] protected float damage;            //���ݷ�
    [SerializeField] protected float hitNum;            //Ÿ�� Ƚ��
    [Header("���� ����ġ")]
    [SerializeField] protected float upScaleHp;         //ü�� ����ġ
    [SerializeField] protected float upScaleDamage;     //���ݷ� ����ġ
    [Header("���� ����")]
    [SerializeField] protected float startSpwanNum;     //�ʱ� ���� ��
    [SerializeField] protected float upScaleSpwanNum;   //���� ���� �� 
    [SerializeField] protected float spawnTiming;       //���� ����
    
    //[SerializeField] protected float sensingRange;      //���� ����

    [SerializeField] protected Transform defaltTarget;          //�⺻ Ÿ��
    [SerializeField] protected Transform[] priorityTarget;      //Ÿ�� �켱����
    
    protected int wave;                   
    
    protected Rigidbody rb;
    protected NavMeshAgent nav;

    protected bool isDead = false;                      //���� ����
    protected bool isChase = false;

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
    }

    protected abstract void ChaseTarget();              //Ÿ�� ����

    protected void PriorityTarget()                     //Ÿ�� �켱���� ����
    {
        for (int i = 0; i < priorityTarget.Length; i++)
        {
            if (priorityTarget[i] != null)
            {
                nav.SetDestination(priorityTarget[i].position);
                break;
            }
        }
    }

    protected void FreezeVelocity()                     //������ ����
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    protected void UpScaleHp()
    {
        hp += upScaleHp * wave;
    }

    protected void UpScaleDamage()
    {
        damage += upScaleDamage * wave;
    }

    protected void UpScaleSpawn()
    {
        if (wave % 10 == 0 && wave / 10 > 0)
        {
            startSpwanNum += upScaleSpwanNum;
        }
    }

    protected void Hurt(float damage)                   //�÷��̾�� ������ ���� ��
    { 
        hp -= damage;
    }

    protected void isDie()                              //�׾��� ��
    { 
        isDead = true;
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Turret"))
        { 
            isChase = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Turret"))
        { 
            isChase = false;
        }
    }
}
