using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAttackState : TurretBaseState
{
    private float attackCheckTime;

    public TurretAttackState(Turret turret) : base(turret) { }

    public override void Enter()
    {
        turret.turretStateName = TurretStateName.ATTACK;
        if (attackCheckTime >= 1 / turret.turretAttackSpeed)
        {
            turret.Attack();
        }
    }

    public override void Update()
    {
        attackCheckTime += Time.deltaTime;

        turret.spinPos.transform.LookAt(turret.turretTargetTransform);

        //���� ���� �� �Ҹ��� ����
        //Audio Manager���� ó�� 
        //���� ����׿�
        turret.fireAudio.pitch = Time.timeScale;

        if (attackCheckTime >= 1/turret.turretAttackSpeed)
        {
            
            turret.Attack();
            
            attackCheckTime = 0;
        }

        if (turret.turretTargetTransform.gameObject.activeSelf == false || Vector3.Distance(turret.transform.position, turret.turretTargetTransform.transform.position) > turret.turretAttackRange/*||turret.turretTargetTransform.gameObject.GetComponent<Monster>().isDead*/) 
        {
            //��ã�� ���·� ����
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }


    }

    public override void Exit()
    {
        turret.turretTargetTransform = null;
        turret.fireAudio.Stop();
        turret.fireEfect.SetActive(false);
    }


}
