using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Info : MonoBehaviour
{
    //Level
    private int Level;

    [Header("�ִ� ü��")]
    [SerializeField]
    private float maxHp;

    public float GetmaxHP { get { return maxHp; } }

    public float HP {  get; private set; }
    public int GearCount { get; private set; }

    [Header("���ݷ�")]
    [SerializeField]
    private float ATKDamage;

    public float GetATKDamage { get {  return ATKDamage; } }

    [Header("�ִ� ź ��")]
    [SerializeField]
    private float maxBullet;

    [Header("�ִ� źâ ��")]
    [SerializeField]
    private float maxMagazine;

    [Space(10)]
    [Header("�⺻ �̵� �ӵ�")]
    [SerializeField]
    private float DefaultSpeed;
    public float defaultSpeed { get { return DefaultSpeed; } }

    [Space(10)]
    [Header("�޸��� �̵� �ӵ�")]
    [SerializeField]
    private float RunSpeed;

    [Space(10)]
    [Header("LevelUp Cost")]
    [SerializeField]
    private float LevelUpCost;

    public float GetLevelCost {  get { return LevelUpCost; } }

    [Space(10)]
    [Header("LevelUp Cost ���� ��ġ")]
    [SerializeField]
    private float upCostValue;

    public float runSpeed { get { return RunSpeed; } }

    public float Attack { get { return ATKDamage; } }

    public float maxEquipedBulletCount {  get; private set; }
    [HideInInspector]
    public float equipedBulletCount;

    public float maxMagazineCount {  get; private set; }
    [HideInInspector]
    public float magazineCount;

    private Animator animator;

    public bool isDead {  get; private set; }

    private Player_Info_UI UI;

    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDead = Animator.StringToHash("Die");

    private float timer;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        UI = GetComponent<Player_Info_UI>();        
    }

    private void OnEnable()
    {
        maxEquipedBulletCount = maxBullet;
        maxMagazineCount = maxMagazine;

        equipedBulletCount = maxEquipedBulletCount;
        magazineCount = maxMagazineCount;

        Level = 0;
        HP = maxHp;

        GearCount = 100;
        UI.InitGearText(GearCount);

        Spawn();
    }

    private void Update()
    {
        if (isDead)
        {
            timer += Time.deltaTime;
            if(timer >= 8f)
            {                               
                Respawn();
            }
        }
    }

    public void AddGearCount(int gearCount)
    {
        GearCount += gearCount;
        UI.ChangeGearText(GearCount, gearCount);
    }

    public void UseGear(int gearCount)
    {
        GearCount -= gearCount;
        UI.ChangeGearText(GearCount, -gearCount);
    }

    public void Heal(float healValue)
    {
        HP = Mathf.Clamp(HP + healValue, 0, maxHp);
        UI.PrintPlayerHPBar(HP, maxHp);
    }

    public void Hurt(float damage)
    {
        if (isDead) return;
        HP -= damage;
        UI.PrintPlayerHPBar(HP, maxHp);
        if (HP <= 0)
        {
            Dead();
        }
        else
        {
            if(Random.Range(0,100) < 20)
            {
                animator.SetTrigger(hashHurt);
            }
        }
    }

    public void Spawn()
    {
        transform.position = GameManager.Instance.GetSpawnPoint.position;
        transform.rotation = GameManager.Instance.GetSpawnPoint.rotation;
    }

    private void Dead()
    {
        isDead = true;
        animator.SetBool(hashDead, true);
        //StartCoroutine(Respawn());
    }

    private void Respawn()
    {
        if (GameManager.Instance.GetCore.gameObject.activeSelf)
        {
            Debug.Log("������ ��...");
            animator.SetBool(hashDead, false);

            HP = maxHp;
            equipedBulletCount = maxEquipedBulletCount;
            magazineCount = maxMagazineCount / 2;

            GearCount -= 20;

            Spawn();
            timer = 0;
            isDead = false;
        }
        else
        {
            //���� ����
            GameOver();
        }
    }

    //IEnumerator Respawn()
    //{
    //    yield return new WaitForSeconds(3f);
    //    if (GameManager.Instance.GetCore.gameObject.activeSelf)
    //    {
    //        Spawn();
    //    }
    //    else
    //    {
    //        //���� ����

    //    }  
    //    yield break;
    //}

    //Dead Animation�� ������ ����Ǵ� �޼���
    public void GameOver()
    {
        //���� ���� ����
        //���� ���� �޴� ����
    }

    public void OnDebug_Dead(InputAction.CallbackContext context)
    {
        if (context.started) return;
        if (context.performed)
        {
            Debug.Log("������ ����");
            Hurt(50);
        }
    }

    private void PlayerUpgradeComplete()
    {
        Debug.Log("���׷��̵� �Ϸ�");
        Level++;
        maxHp += 20;
        HP = maxHp;
        ATKDamage *= 1.2f;
        //�̵� �ӵ� ����
        //DefaultSpeed += 1f;
        //RunSpeed += 1f;
        //cost ����
        LevelUpCost *= upCostValue;
    }

    public bool UpgradePlayer()
    {
        if(GearCount < LevelUpCost)
        {
            return false;
        }
        else
        {
            UseGear((int)Mathf.Round(LevelUpCost));
            PlayerUpgradeComplete();
            return true;
        }
    }
}
