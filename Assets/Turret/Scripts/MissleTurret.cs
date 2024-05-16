using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleTurret : Turret
{
    private int nowMissleTurretUpgradeCount = 0;
    private int nowMissleTurretHp;
    private int maxMissleTurretHp = 5;
    private int missleTurretHpRise = 2;
    private float missleTurretAttackRadius = 3;
    private float missleTurretNowAttackRadius;
    private float missleTurretAttackRadiusRise = 0.5f;
    private float missleTurretMakingTime = 15;
    private float missleTurretAttackDamge = 50;
    private float missleTurretAttackSpeed = 0.5f;
    private float missleTurretAttackRange = 15;
    private float missleTurretUpgradeTime = 15;
    private float missleTurretRepairTime = 15;
    private float missleTurretAttackRise = 20;
    private float missleTurretAttackSpeedRise = 0.3f;
    private float missleTurretUpgradCostRise = 1.3f;
    private float missleTurretMaxUpgradeCount = 4;
    private float missleTurretRepairCost = 8;
    private float missleTurretUpgradeCost = 15;
    private float missleTurretMakingCost = 20;

    [SerializeField]
    private GameObject explosionEffect;

    protected override void OnEnable()
    {
        base.OnEnable();
        base.SetTurret(missleTurretMakingTime, missleTurretMakingCost, missleTurretAttackDamge, missleTurretAttackSpeed, missleTurretAttackRange, maxMissleTurretHp, missleTurretHpRise, 
            missleTurretUpgradeCost, missleTurretUpgradeTime, missleTurretRepairTime, missleTurretRepairCost, missleTurretAttackRise, missleTurretAttackSpeedRise, missleTurretUpgradCostRise, missleTurretMaxUpgradeCount);
        missleTurretNowAttackRadius = missleTurretAttackRadius;
        explosionEffect.SetActive(false);
    }

    public override void Attack()
    {
        if(Physics.Raycast(firePos.transform.position, firePos.transform.forward,out RaycastHit hit, missleTurretAttackRange))
        {
            if (!hit.collider.CompareTag("Monster"))
            {
                return;
            }
            else
            {
                Collider[] targets = Physics.OverlapSphere(targetTransform.position, missleTurretNowAttackRadius, (1 << monsterLayer));
                //����Ʈ ����
                fireEfect.SetActive(true);
                explosionEffect.SetActive(true);
                explosionEffect.transform.position = targets[0].gameObject.transform.position;
                explosionEffect.GetComponent<ParticleSystem>().Play();
                fireEfect.GetComponent<ParticleSystem>().Play();
                foreach (Collider target in targets)
                {
                    if (target.CompareTag("Monster"))
                    {
                        //���� �������ִ� �κ�
                        Debug.Log("���� �ͷ� ����");


                    }
                    //�巳�� ������ �κ�
                }
            }
        }
        

    }

    public override void Upgrade()
    {
        base.Upgrade();
        missleTurretNowAttackRadius += missleTurretAttackRadiusRise;
    }

}
