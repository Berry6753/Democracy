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
        turret.sliderGage.gameObject.SetActive(true);
        turret.sliderGage.maxValue = turret.turretMakingTime;
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
        //turret.sliderGage.transform.LookAt(Camera.main.transform.position); //�÷��̾� �ٶ󺸰� �ƴϸ� ī�޶�
        if (checkTime > turret.turretMakingTime)
        {
            //��ã�� ���·� ��ȯ
            turret.turretStatemachine.ChangeState(TurretStateName.SEARCH);
        }
    }

    public override void Exit()
    {
        checkTime = 0;
        turret.OnRenderer();
        turret.sliderGage.gameObject.SetActive(false);
        //����� ����Ʈ ����
        turret.makingEfect.SetActive(false);
    }
}
