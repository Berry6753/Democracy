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
        turret.sliderGage.gameObject.SetActive(true);
        turret.repairAudio.Play();
        turret.sliderGage.maxValue = turret.turretRepairTime;
    }

    public override void Update()
    {
        checkTime += Time.deltaTime;
        turret.sliderGage.value = checkTime;
        turret.sliderGage.transform.parent.forward = Camera.main.transform.forward;
        
        turret.repairAudio.pitch = Time.timeScale;
        if (checkTime >= turret.turretRepairTime)
        {
            //��ã��� ����
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }
    }

    public override void Exit()
    {
        checkTime = 0;
        turret.repairAudio.Stop();
        turret.sliderGage.gameObject.SetActive(false);
        turret.Repair();
    }
}
