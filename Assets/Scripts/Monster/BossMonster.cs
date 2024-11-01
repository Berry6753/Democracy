using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class BossMonster : MonoBehaviour
{
    [Header("스탯")]
    [SerializeField] private float maxHp;                //체력
    public float MaxHp { get { return maxHp; } }
    [SerializeField] private float hp;
    public float Hp { get { return hp; } }
    [SerializeField] private float damage;            //공격력
    public float BossDamage { get { return damage; } }
    [SerializeField] private int hitNum;            //타격 횟수
    public int BossPower { get { return hitNum; } }
    [SerializeField] private float attackRange;       //사거리
    [SerializeField] private float specialAttackRange;//특수 공격 사거리
    private float amongRange;

    [Header("스탯 성장치")]
    [SerializeField] private float upScaleHp;         //체력 성장치

    [Header("보스 HP UI")]
    [SerializeField] private GameObject boss_HP_UI;

    [Header("사운드 클립")]
    [SerializeField] private AudioClip roar;
    [SerializeField] private AudioClip dash;

    [Header("파티클")]
    [SerializeField] private ParticleSystem smash;
    [SerializeField] private ParticleSystem dashParticle;

    private float dashSpeed;
    private float dashTime;
    private float time;

    private int _wave = 0;
    public bool isCheckJump = false;
    private bool isSkillAttacking = false;

    [HideInInspector]
    public int wave
    {
        get { return _wave; }
        set
        {
            if (_wave != value)
            {
                _wave = value;
                UpScaleHp();
            }
        }

    }
    private int lastWave;

    private Transform Core;
    private Transform Player;
    private Transform bossTr;
    private Transform chaseTarget;

    private Animator anim;
    private NavMeshAgent nav;
    private Rigidbody rb;
    private SkinnedMeshRenderer[] renderer;
    private SphereCollider[] attackC;
    private BoxCollider jumpAttackC;
    private Vector3 dashDir;
    private AudioSource audio;

    [SerializeField] private CapsuleCollider dashAttackC;
    [SerializeField] protected LayerMask turretLayer;   //터렛레이어
    [SerializeField] protected LayerMask monsterLayer;  //몬스터레이어
    [SerializeField] protected float sensingRange;      //감지 범위
    private int turretIndex = 0;                      //가장 가까운 터렛인덱스
    //private int secondTurretIndex = 0;                //두 번째 가까운 터렛인덱스
    //private int targetingIndex = 0;                   //타겟으로 삼을 터렛인덱스
    
    [Header("Monster Start Point")]
    [SerializeField] private Transform defaultPos;

    private StateMachine stateMachine;

    private readonly int hashTrace = Animator.StringToHash("isTrace");
    private readonly int hashDie = Animator.StringToHash("isDie");
    private readonly int hashJumpA = Animator.StringToHash("isJumpA");
    private readonly int hashDashA = Animator.StringToHash("isDashA");
    private readonly int hashDefaultA = Animator.StringToHash("isDefaultA");
    private readonly int hashDefaultA2 = Animator.StringToHash("isDefaultA2");
    private readonly int hashJump = Animator.StringToHash("isJump");
    private readonly int hashDash = Animator.StringToHash("isDash");
    private readonly int hashAttack = Animator.StringToHash("isAttack");

    [HideInInspector] public bool isDead = false;
    private bool isDash = false;
    private bool canAttack = true;
    private bool canJump = true;
    private bool isBackward = false;

    public enum State
    { IDLE, TRACE, JumpA, DashA, DefaultA, DefaultA2, DIE, Back }
    public State state = State.IDLE;

    private List<GameObject> targetList;

    private void Awake()
    {
        targetList = new List<GameObject>();
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        renderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        bossTr = GetComponent<Transform>();
        audio = GetComponent<AudioSource>();

        attackC = GetComponentsInChildren<SphereCollider>();
        jumpAttackC = GetComponentInChildren<BoxCollider>();
        stateMachine = gameObject.AddComponent<StateMachine>();

        stateMachine.AddState(State.IDLE, new IdleState(this));
        stateMachine.AddState(State.TRACE, new TraceState(this));
        stateMachine.AddState(State.JumpA, new JumpAState(this));
        stateMachine.AddState(State.DashA, new DashAState(this));
        stateMachine.AddState(State.DefaultA, new DefaultAState(this));
        stateMachine.AddState(State.DefaultA2, new DefaultA2State(this));
        stateMachine.AddState(State.DIE, new DieState(this));
        stateMachine.AddState(State.Back, new BackState(this));
        stateMachine.InitState(State.IDLE);

        amongRange = (attackRange + specialAttackRange) / 2;

        lastWave = GameManager.Instance.WaveSystem.LastWave;
    }
    private void OnEnable()
    {
        Core = GameObject.FindWithTag("Core").GetComponent<Transform>();
        Player = GameManager.Instance.GetPlayer.transform;

        transform.position = defaultPos.position;
        transform.rotation = defaultPos.rotation;

        stateMachine.ChangeState(State.IDLE);
        hp = maxHp;
        isDead = false;
        boss_HP_UI.SetActive(false);
        StartCoroutine(BossState());
        foreach (SkinnedMeshRenderer skin in renderer)
        {
            skin.material.color = new Color(1, 1, 1, 1);
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, Player.transform.position) <= sensingRange && !Player.gameObject.GetComponent<Player_Info>().isDead)
        {
            boss_HP_UI.SetActive(true);
        }

        foreach (var list in targetList)
        { 
            if (!list.activeSelf) targetList.Remove(list);
        }

        //일시적으로 주석처리

        if (time > 0 && !anim.GetBool("isAttack"))
        {
            time -= Time.deltaTime;
            canAttack = false;
        }
        else if (time <= 0)
        {
            canAttack = true;
        }
        if (isBackward)
        {
            BackWards();
        }
        if (anim.GetBool(hashDash))
        {
            DashAttackMove();
        }
        else
        {
            dashSpeed = 0;
            dashTime = 0;
            LookAt();
        }
        if (anim.GetBool(hashJump))
        {
            if (canJump)
            {
                StartCoroutine(JumpAttackMove());
            }
        }
        wave = GameManager.Instance.WaveSystem.currentWaveIndex - 1;
    }

    protected virtual void LookAt()
    {
        if (chaseTarget != null)
            transform.LookAt(new Vector3(chaseTarget.position.x, transform.position.y, chaseTarget.position.z));
        else
            transform.LookAt(new Vector3(GameManager.Instance.GetCore.gameObject.transform.position.x, transform.position.y, GameManager.Instance.GetCore.gameObject.transform.position.z));
    }

    private IEnumerator BossState()
    {
        while (!isDead)
        {           
            yield return new WaitForSeconds(0.3f);
            if (Player.gameObject.GetComponent<Player_Info>().isDead)
            {
                chaseTarget = null;
                boss_HP_UI.SetActive(false);
                continue;
            }

            PriorityTarget();

            if (hp <= 0)
            { 
                hp = 0;
                isDie();
                stateMachine.ChangeState(State.DIE);
                chaseTarget = null;
                yield break;
            }

            if (chaseTarget == null && !isDead)
            {
                if (Vector3.Distance(transform.position, defaultPos.position) < 10f) 
                {
                    stateMachine.ChangeState(State.IDLE);
                }
                else
                {
                    stateMachine.ChangeState(State.Back);
                    hp = maxHp;
                }
            }
            else if(chaseTarget != null && !isDead)
            {
                float distance = Vector3.Distance(chaseTarget.position, bossTr.position);

                if (distance <= attackRange)
                {
                    FreezeVelocity();
                    if (canAttack && !anim.GetBool("isAttack"))
                    {
                        CloseAttack();
                        foreach (SphereCollider coll in attackC)
                        {
                            coll.enabled = true;
                        }
                        jumpAttackC.enabled = false;
                        dashAttackC.enabled = false;
                    }
                    else
                    {
                        nav.enabled = true;
                        stateMachine.ChangeState(State.IDLE);
                    }
                }
                else if (distance <= amongRange && distance > attackRange)
                {
                    nav.enabled = true;
                    stateMachine.ChangeState(State.TRACE);
                    foreach (SphereCollider coll in attackC)
                    {
                        coll.enabled = false;
                    }
                    jumpAttackC.enabled = false;
                    dashAttackC.enabled = false;
                }
                else if (distance <= specialAttackRange && distance > amongRange)
                {
                    FreezeVelocity();
                    if (canAttack && !anim.GetBool("isAttack"))
                    {
                        StandoffAttack();
                    }
                    else
                    {
                        nav.enabled = true;
                        stateMachine.ChangeState(State.IDLE);
                    }
                }
                else if (distance > specialAttackRange && !anim.GetBool(hashJump))
                {
                    nav.enabled = true;
                    stateMachine.ChangeState(State.TRACE);
                    foreach (SphereCollider coll in attackC)
                    {
                        coll.enabled = false;
                    }
                    jumpAttackC.enabled = false;
                    dashAttackC.enabled = false;
                }
            }
        }

        yield break;
    }

    private void CloseAttack()
    {
        int pattern = Random.Range(0, 2);
        switch (pattern)
        {
            case 0:
                stateMachine.ChangeState(State.DefaultA);
                break;
            case 1:
                stateMachine.ChangeState(State.DefaultA2);
                break;
        }
    }
    private void StandoffAttack()
    {
        int pattern = Random.Range(0, 4);
        switch (pattern)
        {
            case 0:
                stateMachine.ChangeState(State.JumpA);
                SoundPlay(roar);
                break;
            case 1:
                stateMachine.ChangeState(State.DashA);
                DashAttack_backward();
                break;
            default:
                stateMachine.ChangeState(State.TRACE);
                break;
        }
    }

    private void JumpAttack()
    {
        anim.SetBool(hashJump, true);
    }
    public void CheckJump()
    {
        isCheckJump = true;
    }
    private IEnumerator JumpAttackMove()
    {
        isSkillAttacking = true;

        canJump = false;
        nav.enabled = false;
        float distance = Vector3.Distance(chaseTarget.position, bossTr.position);
        Vector3 attackPos = transform.position + transform.forward * (distance - 2);
        yield return new WaitUntil(() => isCheckJump);
        foreach (SkinnedMeshRenderer render in renderer)
        {
            render.enabled = false;
        }
        FreezeVelocity();
        yield return new WaitForSeconds(0.1f);
        transform.position = attackPos;
        yield return new WaitForSeconds(1.0f);
        foreach (SkinnedMeshRenderer render in renderer)
        {
            render.enabled = true;
        }
        anim.SetBool(hashJump, false);
        jumpAttackC.enabled = true;
        yield return new WaitForSeconds(2.0f);
        smash.gameObject.SetActive(true);
        nav.enabled = true;
        canJump = true;
        isCheckJump = false;

        isSkillAttacking = false;
    }

    private void DashAttack_backward()
    {
        isBackward = true;
        dashParticle.gameObject.SetActive(true);
    }
    private void BackWards()
    {
        transform.Translate(Vector3.back * 2f * Time.deltaTime);
    }

    private void DashAttack()
    {
        anim.SetBool(hashDash, true);
        dashDir = (chaseTarget.position - transform.position).normalized;
        
        isBackward = false;
    }

    private void DashAttackMove()
    {
        isSkillAttacking = true;
        dashParticle.Stop();
        SoundPlay(dash);
        audio.loop = true;
        dashAttackC.enabled = true;
        float speed = 0f;
        isDash = true;
        if (isDash)
        {

            //dashSpeed += 0.001f;
            //float distance = Vector3.Distance(chaseTarget.position, bossTr.position);
            //Vector3 attackPos = transform.position + transform.forward * distance;

            //transform.position = Vector3.MoveTowards(transform.position, attackPos, (speed + dashSpeed / 2) + Time.deltaTime);



            rb.AddForce(dashDir * 50f, ForceMode.Acceleration);
            dashTime += Time.deltaTime;
            if (dashTime > 2f)
            {
                Debug.Log("aa");
                anim.SetBool(hashDash, false);
                isDash = false;
                FreezeVelocity();
                audio.loop = false;

                isSkillAttacking = false;
            }
        }
    }

    private void AttackDelay()
    {
        time = 1f;
    }

    private void AttackEnd()
    {
        anim.SetBool("isAttack", false);
    }

    private void PriorityTarget()                     //타겟 우선순위 설정
    {
        Targeting();

        if (wave >= lastWave && chaseTarget == null)
        {
            if (Core.gameObject.activeSelf)
                chaseTarget = Core;
            else 
                chaseTarget = GameManager.Instance.GetPlayer.gameObject.transform;
        }
    }

    private void Targeting()
    {
        targetList.Clear();
        Collider[] Targets = Physics.OverlapSphere(transform.position, sensingRange, turretLayer);

        if (Targets.Length <= 0)
        { 
            return; 
        }
        //foreach (Collider target in Targets)
        //{
        //    if (target.gameObject.layer == LayerMask.NameToLayer("Player"))
        //    {
        //        boss_HP_UI.SetActive(true);
        //    }
        //    else
        //    {
        //        boss_HP_UI.SetActive(false);
        //    }
        //}

        foreach (Collider target in Targets)
        {
            if (target.gameObject.layer == LayerMask.NameToLayer("Turret"))
            {
                if (target.gameObject.CompareTag("Barrel")) continue;

                targetList.Add(target.gameObject);
            }
        }

        if (targetList.Count <= 0)
        {
            chaseTarget = Player;
        }
        else
        {
            turretDistance(targetList);
            chaseTarget = targetList[turretIndex].transform;
        }
    }

    protected void turretDistance(List<GameObject> array)
    {
        float[] distance = new float[array.Count];
        int minIndex = 0;
        for (int i = 0; i < array.Count; i++)
        {
            distance[i] = Vector3.Distance(array[i].transform.position, bossTr.position);
        }
        float minDistance = distance[0];
        for (int i = 0; i < distance.Length; i++)
        {
            if (distance[i] < minDistance)
            {
                minDistance = distance[i];
                minIndex = i;
            }
        }
        turretIndex = minIndex;
    }

    //protected void TargetingTurret()
    //{
    //    Collider[] monster = Physics.OverlapBox(transform.position, new Vector3(5f, 5f, 5f), transform.rotation, monsterLayer);
    //    if (monster.Length > 3)
    //    {
    //        targetingIndex = secondTurretIndex;
    //    }
    //    else
    //    {
    //        targetingIndex = turretIndex;
    //    }
    //}
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(transform.position, new Vector3(5f, 5f, 5f));
    //}

    protected void FreezeVelocity()                     //물리력 제거
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void UpScaleHp()                          //체력 증가량
    {
        if (wave >= 0)
            maxHp += upScaleHp * wave;
    }

    public void Hurt(float damage)                   //플레이어에게 데미지 입을 시
    {
        int count = 0;
        if (isSkillAttacking) return;
        foreach (var item in GameManager.Instance.MonsterCrystal)
        {
            if (!item.gameObject.activeSelf)
                count ++;
        }
        if (count >= GameManager.Instance.MonsterCrystal.Count)
        {
            hp -= damage;
            hp = Mathf.Clamp(hp, 0, maxHp);
        }
        StartCoroutine(OnDamaged());
    }

    private IEnumerator OnDamaged()
    {
        foreach (SkinnedMeshRenderer skin in renderer)
        {
            skin.material.color = new Color(0, 0, 0, 1);
        }
        yield return new WaitForSeconds(0.2f);
        foreach (SkinnedMeshRenderer skin in renderer)
        {
            skin.material.color = new Color(1, 1, 1, 1); 
        }
    }

    public void isDie()                              //죽었을 시
    {
        isDead = true;
    }

    private void SoundPlay(AudioClip clip)
    {
        audio.clip = clip;
        audio.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Turret") || other.gameObject.CompareTag("Monster"))
        {
            if (isDash)
            {
                isDash = false;
                audio.loop = false;
                anim.SetBool(hashDash, false);
                FreezeVelocity();
            }
        }
    }

    protected class BaseMonsterState : BaseState
    {
        protected BossMonster owner;
        public BaseMonsterState(BossMonster owner)
        { this.owner = owner; }
    }
    protected class IdleState : BaseMonsterState
    {
        public IdleState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.isStopped = true;
            owner.anim.SetBool(owner.hashTrace, false);
        }
    }
    protected class TraceState : BaseMonsterState
    {
        public TraceState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.SetDestination(owner.chaseTarget.position);
            owner.nav.isStopped = false;
            owner.anim.SetBool(owner.hashTrace, true);
        }
    }
    protected class JumpAState : BaseMonsterState
    {
        public JumpAState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetBool(owner.hashAttack, true);
            owner.anim.SetTrigger(owner.hashJumpA);
        }
    }
    protected class DashAState : BaseMonsterState
    {
        public DashAState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.isStopped = true;
            owner.anim.SetBool(owner.hashAttack, true);
            owner.anim.SetTrigger(owner.hashDashA);
        }
    }
    protected class DefaultAState : BaseMonsterState
    {
        public DefaultAState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetBool(owner.hashAttack, true);
            owner.anim.SetTrigger(owner.hashDefaultA);
        }
    }
    protected class DefaultA2State : BaseMonsterState
    {
        public DefaultA2State(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.anim.SetBool(owner.hashAttack, true);
            owner.anim.SetTrigger(owner.hashDefaultA2);
        }
    }
    protected class DieState : BaseMonsterState
    {
        public DieState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.enabled = true;
            owner.nav.isStopped = true;
            owner.anim.SetTrigger(owner.hashDie);
        }
    }
    protected class BackState : BaseMonsterState
    {
        public BackState(BossMonster owner) : base(owner) { }
        public override void Enter()
        {
            owner.nav.SetDestination(owner.defaultPos.position);
            owner.nav.isStopped = false;
            owner.anim.SetBool(owner.hashTrace, true);
        }
    }
}
