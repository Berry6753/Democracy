using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMakingState : TurretBaseState
{
    private float checkTime;
    public TurretMakingState(Turret turret) : base(turret) { }

    public override void Enter()
    {
        turret.turretStateName = TurretStateName.MAKING;
        //����� ����Ʈ ����
        turret.makingEfect.SetActive(true);
    }

    public override void Update()
    {
        checkTime += Time.deltaTime;

        if(checkTime > turret.turretMakingTime)
        {
            //��ã�� ���·� ��ȯ
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }
    }

    public override void Exit()
    {
        checkTime = 0;
        turret.OnRenderer();
        //����� ����Ʈ ����
        turret.makingEfect.SetActive(false);
    }
}
