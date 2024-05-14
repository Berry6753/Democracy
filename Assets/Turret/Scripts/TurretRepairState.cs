using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretRepairState : TurretBaseState
{
    private float checkTime;

    public TurretRepairState(Turret turret) : base(turret) { }

    public override void Enter()
    {
        turret.turretStateName = TurretStateName.REPAIR;
        //������ ǥ��
    }

    public override void Update()
    {
        checkTime += Time.time;
        if (checkTime >= turret.turretRepairTime)
        {
            //��ã��� ����
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }
    }

    public override void Exit()
    {
        checkTime = 0;
        turret.Repair();
    }
}
