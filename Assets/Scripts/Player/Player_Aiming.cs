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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        AttackTimer = AttackDelayTime;
        notAimingTimer = notAimingDelayTime;

        buildSystem = GetComponent<Player_BuildSystem>();
    }

    public void OnAiming(InputAction.CallbackContext context)
    {
        isAiming = context.ReadValue<float>() > 0.5f;
    }

    public void OnGameStop(InputAction.CallbackContext context)
    {
        isGameStop *= -1;
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
        ShootRay();
        ParticleSystem.GetComponent<ParticleSystem>().Play();
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
