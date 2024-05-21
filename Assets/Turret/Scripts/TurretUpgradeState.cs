using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretUpgradeState : TurretBaseState
{
    private float checkTime;
    public TurretUpgradeState(Turret turret) : base(turret) { }

    public override void Enter()
    {
        turret.turretStateName = TurretStateName.UPGRADE;
        turret.OffRenderer();
        turret.sliderGage.gameObject.SetActive(true);
        turret.sliderGage.maxValue = turret.turretUpgradeTime;
        turret.sliderGage.transform.position = turret.transform.position;
        //����� ����Ʈ ����
        turret.makingEfect.SetActive(true);
    }

    public override void Update()
    {
        checkTime += Time.deltaTime;
        turret.sliderGage.value = checkTime;
        turret.sliderGage.transform.parent.forward = Camera.main.transform.forward;
        turret.makeAudio.pitch = Time.timeScale;
        if (checkTime >= turret.turretUpgradeTime)
        {
            //��ã�� ���·�  ����
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }
    }

    public override void Exit()
    {
        checkTime = 0;
        turret.Upgrade();
        turret.OnRenderer();
        turret.sliderGage.gameObject.SetActive(false);
        turret.makingEfect.SetActive(false);
    }
}
