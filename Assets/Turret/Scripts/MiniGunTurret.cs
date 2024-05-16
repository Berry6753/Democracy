using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGunTurret : Turret
{

    private int nowMiniGunUpgradeCount = 0;
    private int nowMiniGunHp;
    private int maxMiniGunHp = 5;
    private int miniGunhpRise = 3;
    private float miniGunMakingTime = 15;
    private float miniGunAttackDamge = 25;
    private float miniGunAttackSpeed = 2;
    private float miniGunAttackRange = 12;
    private float miniGunUpgradeTime = 15;
    private float miniGunRepairTime = 15;
    private float miniGunAttackRise = 5;
    private float miniGunAttackSpeedRise = 0.5f;
    private float miniGunUpgradCostRise = 2;
    private float miniGunMaxUpgradeCount = 5;
    private float miniGunRepairCost = 5;
    private float miniGunUpgradeCost = 10;
    private float miniGunMakingCost = 15;


    protected override void OnEnable()
    {
        base.OnEnable();
        base.SetTurret(miniGunMakingTime, miniGunMakingCost, miniGunAttackDamge, miniGunAttackSpeed, miniGunAttackRange, maxMiniGunHp, miniGunhpRise, miniGunUpgradeCost
            , miniGunUpgradeTime, miniGunRepairTime, miniGunRepairCost, miniGunAttackRise, miniGunAttackSpeedRise, miniGunUpgradCostRise, miniGunMaxUpgradeCount);
    }

    public override void Attack()
    {
        if (Physics.Raycast(firePos.transform.position, firePos.transform.forward, out RaycastHit hit, miniGunAttackRange))
        {
            
            if (!hit.collider.CompareTag("Monster"))
            {
                return;
            }
            else if (hit.collider.CompareTag("Monster"))
            {
                fireEfect.SetActive(true);
                //���� ������ �ִ� �κ�
                //���� �Լ� �ҷ��´� �Ҹ�
                Debug.Log("���� �ͷ� ����");

            }
            //else if ()
            //{
            ////�巳�� ���߽�Ű�⵵ �־����

            //}
        }
    }

}
