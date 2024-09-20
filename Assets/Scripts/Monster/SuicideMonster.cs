using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SuicideMonster : Monster
{
    private float explosionTime = 3f;
    private float duringExplosion = 3f;
    [SerializeField] private ParticleSystem exlosionEffect;
    [SerializeField] private GameObject form;

    private float defaultSpeed;

    private bool isExplosionAble;

    protected override void Awake()
    {
        base.Awake();
        defaultTarget = GameObject.FindWithTag("Player").GetComponent<Transform>();
        defaultSpeed = 7f;
    }
    private void Start()
    {
        chaseTarget = defaultTarget;
        ChaseTarget();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        nav.speed = defaultSpeed;
        isExplosionAble = true;
    }

    protected override void Update()
    {
        base.Update();
        LookAt();
    }

    protected override void ChaseTarget()
    {
        StartCoroutine(MonsterState());
    }

    protected override IEnumerator MonsterState()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(0.3f);
            if (hp <= 0)
            {
                stateMachine.ChangeState(State.DIE);
                isDie();
                yield break;
            }

            float distance = Vector3.Distance(chaseTarget.position, monsterTr.position);
            if (distance <= attackRange)
            {
                //FreezeVelocity();
                stateMachine.ChangeState(State.IDLE);
                //yield return new WaitForSeconds(explosionTime);
                //stateMachine.ChangeState(State.ATTACK);                
                //Destroy(gameObject);
                if (isExplosionAble)
                {
                    StartCoroutine(AttackingPlayer());
                }                
            }
            else
            {
                stateMachine.ChangeState(State.TRACE);
            }
        }
    }

    IEnumerator AttackingPlayer()
    {
        isExplosionAble = false;
        nav.speed = 4f;
        yield return new WaitForSeconds(explosionTime);
        stateMachine.ChangeState(State.ATTACK);
        yield break;
    }

    public void MonsterExplosionAttack()
    {
        form.SetActive(false);

        foreach (Collider c in attack)
        {
            c.enabled = true;
        }
        exlosionEffect.Play();
        //yield return new WaitForSeconds(duringExplosion);
        //form.SetActive(false);
        StartCoroutine(ExplosionEnd());
    }

    IEnumerator ExplosionEnd()
    {
        yield return new WaitForSeconds(duringExplosion);
        gameObject.SetActive(false);
        yield break;
    }

    private void OnDisable()
    {
        MonsterObjectPool.ReturnToPool(gameObject);
    }

    protected override void UpScaleDamage()
    {
        if (wave % 5 == 0 && wave / 5 > 2)
        {
            damage *= 2;
        }
    }
}
