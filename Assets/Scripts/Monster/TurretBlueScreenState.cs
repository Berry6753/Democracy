using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBlueScreenState : TurretBaseState
{

    public TurretBlueScreenState(Turret turret) : base(turret) { }

    public override void Enter()
    {
        turret.turretStateName = TurretStateName.BLUESCREEN;
        //üũ�� Ʈ���� on
        //�ͷ��� �ݶ��̴� off
        //�ͷ��� �±׿� ���̾� ���ֱ�
    }

    public override void Update()
    {
        turret.ChangeColor();
    }

    public override void Exit() 
    {
        turret.ResetColor();
        //üũ�� Ʈ���� off
        //�ͷ��� �ݶ��̴� on
        //�ͷ��� �±׿� ���̾� ����
    }
}
