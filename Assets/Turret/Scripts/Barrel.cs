using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    //private int nowUpgradeCount;
    //private int nowHp;
    //private int maxHp;
    //private int hpRise;
    //private float makingTime;
    //private float makingCost;
    //private float attackDamge;
    //private float attackSpeed;
    //private float attackRange;
    //private float upgradeCost;
    //private float upgradeTime;
    //private float repairTime;
    //private float repairCost;
    //private float attackRise;
    //private float attackSpeedRise;
    //private float upgradCostRise;
    //private float maxUpgradeCount;

    //private void Awake()
    //{
    //    base.SetTurret(makingTime,makingCost,attackDamge,attackRange,maxHp,hpRise,upgradeCost,upgradeTime,)
    //}

    private int attackDamge = 300;
    //private int hp = 1;
    private int range = 5;
    private LayerMask monsterLayer;
    private MeshRenderer meshRenderer;

    public int maikngCost = 5;

    //���� �±׿� ���̾� �������� 0512����

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        meshRenderer.enabled = true;
        
    }

    public void Hurt()
    {

        Collider[] enemyCollider = Physics.OverlapSphere(transform.position, range, monsterLayer);

        meshRenderer.enabled = false;
        //����Ʈ ����


        if (enemyCollider.Length > 0)
        {
            foreach (Collider collider in enemyCollider)
            {
                //������ hurt�Լ��� ��� ������ �ִ±��
            }
        }

        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
