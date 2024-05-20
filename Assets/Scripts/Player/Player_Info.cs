using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Info : MonoBehaviour
{
    [Header("�ִ� ü��")]
    [SerializeField]
    private float maxHp;

    public float HP {  get; private set; }
    public int GearCount { get; private set; }

    [Header("���ݷ�")]
    [SerializeField]
    private float ATKDamage;
    public float Attack { get { return ATKDamage; } }

    [Header("�ִ� ź ��")]
    [SerializeField]
    private float maxBullet;
    [Header("�ִ� źâ ��")]
    [SerializeField]
    private float maxMagazine;

    public float maxEquipedBulletCount {  get; private set; }
    public float equipedBulletCount;

    public float maxMagazineCount {  get; private set; }
    public float magazineCount;

    private Animator animator;
    private bool isDead;

    private Player_Info_UI UI;

    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDead = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        UI = GetComponent<Player_Info_UI>();

        maxEquipedBulletCount = maxBullet;
        maxMagazineCount = maxMagazine;

        equipedBulletCount = maxEquipedBulletCount;
        magazineCount = maxMagazineCount;

        HP = maxHp;

        GearCount = 100;
        UI.InitGearText(GearCount);
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
        HP -= damage;
        UI.PrintPlayerHPBar(HP, maxHp);
        if (HP <= 0)
        {
            Dead();
        }
        else
        {
            animator.SetTrigger(hashHurt);
        }
    }

    private void Dead()
    {
        isDead = true;
        animator.SetBool(hashDead, true);
    }

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
            Hurt(50);
        }
    }

    public void UpgradeAttack()
    {

    }
}
