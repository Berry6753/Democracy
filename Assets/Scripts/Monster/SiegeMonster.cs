using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SiegeMonster : Monster
{
    protected override void Awake()
    {
        base.Awake();
        defaultTarget = GameObject.FindWithTag("Core").GetComponent<Transform>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ChaseTarget();
    }


    protected override void Update()
    {
        base.Update();
        PriorityTarget();
        LookAt();
    }

    protected override void ChaseTarget()
    {
        StartCoroutine(MonsterState());
    }
}
