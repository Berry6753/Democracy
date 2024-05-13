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

    protected float distance;                           //�÷��̾���� �Ÿ� 
    
    //[SerializeField] protected float sensingRange;    //���� ����

    [SerializeField] protected Transform defaltTarget;  //�⺻ Ÿ��
    //protected List<Transform> turretTarget = new List<Transform>();       //�ͷ� Ÿ��
    protected Collider[] turret;
    protected int turretIndex = 0;


    protected int wave;                   
    
    protected Rigidbody rb;
    protected NavMeshAgent nav;

    protected bool isDead = false;                      //���� ����

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
    }

    protected abstract void ChaseTarget();              //Ÿ�� ����

    //protected void PriorityTarget()                     //Ÿ�� �켱���� ����
    //{
    //    if (turretTarget.Count > 0)
    //    {
    //        for (int i = 0; i < turretTarget.Count; i++)
    //        {
    //            nav.SetDestination(turretTarget[i].transform.position);
    //            break;
    //        }
    //    }
    //    else
    //    {
    //        nav.SetDestination(defaltTarget.position);
    //    }
    //}

    protected void PriorityTarget()
    {
        turret = Physics.OverlapSphere(transform.position, 100f, 8);
        Debug.Log(turret);
        if (turret.Length > 0)
        {           
            nav.SetDestination(turret[turretIndex].transform.position);
        }
        else
        {
            nav.SetDestination(defaltTarget.transform.position);
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

    //protected void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Turret"))
    //    {
    //        turretTarget.Add(other.gameObject.transform);
    //        if (other.gameObject.activeSelf == false)
    //        {
    //            turretTarget.Remove(other.gameObject.transform);
    //        }
    //    }
    //}

    //protected void OnTriggerExit(Collider other)
    //{
    //    if(other.gameObject.CompareTag("Turret"))
    //    {
    //        turretTarget.Remove(other.gameObject.transform);
    //    }
    //}
}