using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FireTurret : Turret
{

    [SerializeField]
    private GameObject attackZone;

    private Vector3 attackBoxPos;
    private int nowFireTurretUpgradeCount = 0;
    private int nowFireTurretHp;
    private int maxFireTurretHp = 15;
    private int fireTurretHpRise = 5;
    private float attackRangeX = 2;
    private float fireTurretMakingTime = 15;
    private float fireTurretAttackDamge = 5;
    private float fireTurretAttackSpeed = 5f;
    private float fireTurretAttackRange = 8;
    private float fireTurretUpgradeTime = 15;
    private float fireTurretRepairTime = 15;
    private float fireTurretAttackRise = 1.5f;
    private float fireTurretAttackSpeedRise = 0.5f;
    private float fireTurretUpgradCostRise = 2f;
    private float fireTurretMaxUpgradeCount = 5;
    private float fireTurretRepairCost = 5;
    private float fireTurretUpgradeCost = 10;
    private float fireTurretMakingCost = 15;
    private Vector3 attackBoxRange;

    

    protected override void Awake()
    {
        base.Awake();
        attackBoxRange = new Vector3(attackRangeX, 3, fireTurretAttackRange);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        base.SetTurret(fireTurretMakingTime, fireTurretMakingCost, fireTurretAttackDamge, fireTurretAttackSpeed, fireTurretAttackRange, maxFireTurretHp, fireTurretHpRise, fireTurretUpgradeCost, fireTurretUpgradeTime, fireTurretRepairTime, fireTurretRepairCost, fireTurretAttackRise, fireTurretAttackSpeedRise, fireTurretUpgradCostRise, fireTurretMaxUpgradeCount);
        attackZone.SetActive(false);
    }

    public override void Attack()
    {
        if (targetIndex >= turretTargetList.Count)
        {
            return;
        }

        targetTransform = targetList[targetIndex].transform;

        if (Physics.Raycast(firePos.transform.position, targetTransform.position - firePos.transform.position, out RaycastHit hit, fireTurretAttackRange, ~(ignoreLayer))) 
        {
            if (!hit.collider.CompareTag("Monster") && !hit.collider.CompareTag("Boss"))
            {
                targetIndex++;
                return;
            }
            else
            {
                //attackBoxPos = firePos.transform.position + firePos.transform.forward * fireTurretAttackRange;
                //����Ʈ ����
                fireEfect.SetActive(true);
                attackZone.SetActive(true);
                if (!fireAudio.isPlaying)
                {
                    fireAudio.Play();
                }
                //Collider[] colliders = Physics.OverlapBox(attackBoxPos, attackBoxRange, firePos.transform.rotation);
               
                //foreach (Collider collider in colliders)
                //{
                //    if (collider.CompareTag("Monster"))
                //    {
                        
                           
                        
                       
                //    }
                //    //else if ()//드럼통 공격시
                //    //{

                //    //}

                //}
                //������ �ִºκ�
                
            }
        }

        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
    }
}
