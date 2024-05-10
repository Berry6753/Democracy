using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Monster : MonoBehaviour
{
    [SerializeField] protected string monsterName;      //���� ����
    [SerializeField] protected float attactSpeed;       //���� �ӵ�
    [SerializeField] protected float hp;                //ü��
    [SerializeField] protected float damage;            //���ݷ�
    [SerializeField] protected float speed;             //�̵� �ӵ�
    [SerializeField] protected float hitNum;            //Ÿ�� Ƚ��
    [SerializeField] protected float upScaleHp;         //ü�� ����ġ
    [SerializeField] protected float upScaleDamage;     //���ݷ� ����ġ
    [SerializeField] protected float startSpwanNum;     //�ʱ� ���� ��
    [SerializeField] protected float upScaleSpwanNum;   //���� ���� �� 
    [SerializeField] protected float spawnTiming;       //���� ����
    [SerializeField] protected float sensingRange;      //���� ����

    [SerializeField] protected Transform[] target;      //������ Ÿ��
    
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
        if (isChase)
        {
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] != null)
                {
                    nav.SetDestination(target[i].position);
                    break;
                }
            }
        }
    }

    protected void FreezeVelocity()                     //������ ����
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    protected void Hurt(float damage)                   //�÷��̾�� ������ ���� ��
    { 
        hp -= damage;
    }

    protected void isDie()                              //�׾��� ��
    { 
        isDead = true;
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
