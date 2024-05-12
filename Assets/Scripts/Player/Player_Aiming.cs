using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Player_Aiming : MonoBehaviour
{
    private Animator animator;
    private bool isAiming;

    public bool isFire {  get; private set; }

    private float isGameStop = -1f;

    private readonly int hashAiming = Animator.StringToHash("Aiming");
    private readonly int hashZoomOn = Animator.StringToHash("ZoomOn");
    private readonly int hashFire = Animator.StringToHash("Fire");
    private readonly int hashReload = Animator.StringToHash("Reload");

    [Header("Aiming ī�޶�")]
    [SerializeField]
    private GameObject Camera2;

    [Header("ī�޶� LookAt")]
    [SerializeField]
    private Transform CameraLookAt;

    [Header("�ѱ�")]
    [SerializeField]
    private Transform GunFireStartPoint;

    public Cinemachine.AxisState x_Axis;
    public Cinemachine.AxisState y_Axis;

    [Header("���� LayerMask")]
    [SerializeField]
    private LayerMask aimColliderLayerMask = new LayerMask();
    [Header("Rig�� Target")]
    [SerializeField]
    private Transform debugTransform;

    private float AttackTimer;
    private bool AttackAble;

    [Header("���� ������")]
    [SerializeField]
    private float AttackDelayTime;

    private float notAimingTimer;
    private float notAimingDelayTime = 1.5f;

    [Header("���� ��ƼŬ")]
    [SerializeField]
    private Transform ParticleSystem;

    private Player_BuildSystem buildSystem;

    private float equipedBulletCount;
    [Header("�ִ� ź ��")]
    [SerializeField]
    private float maxEquipedBulletCount;

    private float magazineCount;
    [Header("�ִ� źâ ��")]
    [SerializeField]
    private float maxMagazineCount;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        AttackTimer = AttackDelayTime;
        notAimingTimer = notAimingDelayTime;

        buildSystem = GetComponent<Player_BuildSystem>();

        equipedBulletCount = maxEquipedBulletCount;
        magazineCount = maxMagazineCount;
    }

    public void OnAiming(InputAction.CallbackContext context)
    {
        isAiming = context.ReadValue<float>() > 0.5f;
    }

    public void OnGameStop(InputAction.CallbackContext context)
    {
        isGameStop *= -1;
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (buildSystem.BuildModeOn > 0f) return;

        if (context.performed)
        {
            if(magazineCount > 0)
            {
                //������ ��� ����
                animator.SetBool(hashReload, true);

                Debug.Log("���� ����");
                Debug.Log($"ź �� : {equipedBulletCount}, źâ �� : {magazineCount}");
            }            
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (buildSystem.BuildModeOn > 0f)
        {
            if (context.performed)
            {
                isFire = true;
                buildSystem.BuildTurret();
            }            
        }
        else
        {
            if (context.performed)
            {
                isFire = true;
            }
            if (context.canceled)
            {
                isFire = false;
            }
        }                  
    }

    private void Update()
    {
        CameraRotation();
        GameStopping();
        AimingCamera();
        AimingOnOff();

        FireGun();
        //ShootRay();
    }

    private void FixedUpdate()
    {
        AttackDelay();
        ChangeNotAimingDelay();        
    }

    private void CameraRotation()
    {
        x_Axis.Update(Time.fixedDeltaTime);
        y_Axis.Update(Time.fixedDeltaTime);

        CameraLookAt.eulerAngles = new Vector3(y_Axis.Value, x_Axis.Value, 0);
    }

    private void GameStopping()
    {
        if (isGameStop > 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }

    private void AimingOnOff()
    {
        if (isAiming || notAimingTimer < notAimingDelayTime)
        {
            animator.SetBool(hashAiming, true);     
        }
        else
        {
            animator.SetBool(hashAiming, false);
            AttackTimer = AttackDelayTime;
        }
    }

    private void AimingCamera()
    {
        if (isAiming)
        {
            animator.SetBool(hashZoomOn, true);
        }
        else
        {
            animator.SetBool(hashZoomOn, false);
        }
    }

    private void ShootRay()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if(Physics.Raycast(ray, out RaycastHit hit, 30f, aimColliderLayerMask))
        {              
            if (Physics.Raycast(GunFireStartPoint.position, (hit.point - GunFireStartPoint.position).normalized, out RaycastHit hits, 30f, aimColliderLayerMask))
            {
                debugTransform.position = hits.point;
                Debug.DrawLine(GunFireStartPoint.position, hits.point, Color.red);
                Debug.Log("����!!!");
            }
                //if (hit.transform.CompareTag("Monster"))
                //{
                //    Debug.Log("���� ����");
                //}
        }
    }

    private void FireGun()
    {
        if (buildSystem.BuildModeOn > 0f) return;
        if (animator.GetBool(hashReload)) return;
        if (isFire && AttackAble)
        {
            AttackAble = false;
            AttackTimer = 0;
            notAimingTimer = 0;
            animator.SetBool(hashFire, true);
        }
        else
        {
            animator.SetBool(hashFire, false);
        }
    }

    public void Fire()
    {        
        if(equipedBulletCount > 0)
        {
            ShootRay();
            //���� ��ƼŬ ���
            ParticleSystem.GetComponent<ParticleSystem>().Play();
            //�ݹ� �Ҹ� ���

            //ź�� �� ����
            equipedBulletCount--;
        }
        else
        {
            //�� źâ �Ҹ� ���
        }
    }

    public void ReloadEnd()
    {
        animator.SetBool(hashReload, false);
        notAimingTimer = 0;
        Debug.Log("���� ����...");
        Debug.Log($"ź �� : {equipedBulletCount}, źâ �� : {magazineCount}");
    }

    public void Reload()
    {
        if(magazineCount > 0)
        {
            magazineCount--;
            equipedBulletCount = maxEquipedBulletCount;
        }
    }

    private void AttackDelay()
    {
        if(AttackTimer < AttackDelayTime)
        {
            AttackTimer += Time.fixedDeltaTime;
        }
        else
        {
            AttackAble = true;
        }
    }

    private void ChangeNotAimingDelay()
    {
        if(notAimingTimer < notAimingDelayTime)
        {
            notAimingTimer += Time.fixedDeltaTime;
        }
    }
}
