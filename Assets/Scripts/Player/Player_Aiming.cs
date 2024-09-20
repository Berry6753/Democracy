using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Windows;

public class Player_Aiming : MonoBehaviour
{
    private Animator animator;
    public bool isAiming;

    public bool isFire {  get; private set; }

    private readonly int hashAiming = Animator.StringToHash("Aiming");
    private readonly int hashZoomOn = Animator.StringToHash("ZoomOn");
    private readonly int hashFire = Animator.StringToHash("Fire");
    private readonly int hashReload = Animator.StringToHash("Reload");

    [Header("Aiming 카메라")]
    [SerializeField]
    private GameObject Camera2;

    [Header("카메라 LookAt")]
    [SerializeField]
    private Transform CameraLookAt;

    [Header("총")]
    [SerializeField]
    private Transform gun;

    [Header("총구")]
    [SerializeField]
    private Transform GunFireStartPoint;

    [Header("조준점")]
    [SerializeField]
    private Transform aim;

    public Cinemachine.AxisState x_Axis;
    public Cinemachine.AxisState y_Axis;

    [Header("조준 LayerMask")]
    [SerializeField]
    private LayerMask aimColliderLayerMask = new LayerMask();

    [Header("총구 LayerMask")]
    [SerializeField]
    private LayerMask FireColliderLayerMask = new LayerMask();

    [Header("Rig의 Target")]
    [SerializeField]
    private Transform debugTransform;

    private float AttackTimer;
    private bool AttackAble;

    [Header("공격 딜레이")]
    [SerializeField]
    private float AttackDelayTime;

    private float notAimingTimer;
    private float notAimingDelayTime = 1.5f;

    [Header("총기 탄착 오차")]
    [SerializeField]
    private float attackAccuracy;

    [Header("총기 카메라 반동")]
    [SerializeField]
    private float cameraLerftime;

    [Header("일반 반동")]
    [SerializeField]
    float recoilback;

    [Header("조준 반동")]
    [SerializeField]
    float aimingrecoilback;

    //총기 반동
    private float recoilBackForce;

    [Header("공격 파티클")]
    [SerializeField]
    private Transform ParticleSystem;

    private Player_BuildSystem buildSystem;

    private Player_Info info;
    private Player_Info_UI UI;

    private Quaternion mouseRotation;

    [Header("총 오디오 클립")]
    [SerializeField]
    private List<AudioClip> shootAudioClip;

    [Header("총 피격 Effect")]
    [SerializeField]
    private GameObject effect;

    private AudioSource audioSource;

    private float stopGame;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        AttackTimer = 0;
        cameraLerftime = 1f;
        notAimingTimer = notAimingDelayTime;
        info = GetComponent<Player_Info>();

        buildSystem = GetComponent<Player_BuildSystem>();
        UI = GetComponent<Player_Info_UI>();
        audioSource = GetComponent<AudioSource>();
        stopGame = GameManager.Instance.isGameStop;
    }

    private Quaternion initRotation;

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        InitRotation();
    }

    private void InitRotation()
    {
        x_Axis.Value = 0;
        y_Axis.Value = 0;

        initRotation = CameraLookAt.rotation;

        Vector3 initEulerAngle = initRotation.eulerAngles;
        x_Axis.Value = initEulerAngle.y;
        y_Axis.Value = initEulerAngle.x;

        mouseRotation = initRotation;
    }

    public void OnAiming(InputAction.CallbackContext context)
    {
        if (stopGame > 0) return;
        if (buildSystem.BuildModeOn > 0f) return;
        isAiming = context.ReadValue<float>() > 0.5f;
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (stopGame > 0) return;
        if (buildSystem.BuildModeOn > 0f) return;

        if (context.performed)
        {
            if(info.magazineCount > 0)
            {
                //재장전 모션 실행
                animator.SetBool(hashReload, true);                
            }            
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started) return;
        if (stopGame > 0) return;
        if (buildSystem.BuildModeOn > 0f)
        {
            if (context.performed)
            {
                isFire = true;
                buildSystem.BuildTurret();                
            }
            if (context.canceled)
            {
                isFire = false;
            }
        }
        else
        {
            if (context.performed)
            {
                isFire = true;
                //FireGun();
            }
            if (context.canceled)
            {
                isFire = false;
            }
        }                  
    }

    private void Update()
    {
        if (info.isDead) return;
        //GameStopping();

        //실험 -> 바로 활성화할거임
        CameraRotation();

        AimingOnOff();
        AimingCamera();

    }

    private void FixedUpdate()
    {
        if (info.isDead) return;
        DecideRecoilBack();
        FireGun();

        AttackDelay();
        ChangeNotAimingDelay();
    }

    //초기 카메라 회전 값이 이상한 이유 중 하나로 추정됨
    private void CameraRotation()
    {
        if (GameManager.Instance.isGameStop > 0)
        {
            return;
        }
        x_Axis.Update(Time.fixedDeltaTime);
        y_Axis.Update(Time.fixedDeltaTime);

        mouseRotation = Quaternion.Euler(y_Axis.Value, x_Axis.Value, 0f);

        CameraLookAt.rotation = Quaternion.Lerp(CameraLookAt.rotation, mouseRotation, cameraLerftime);        
    }
    /*mouseRotation;*/
    //Quaternion.Lerp(CameraLookAt.rotation, mouseRotation, cameraLerfTime);

    //총기 격발 시 카메라 흔들림
    // 총을 격발하면 cameralerftime 가 
    private void DecideRecoilBack()
    {
        if (animator.GetBool(hashZoomOn))   //조준 중
        {
            recoilBackForce = aimingrecoilback;
        }
        else    //조준 상태 아님
        {
            recoilBackForce = recoilback;
        }
    }

    private void FirearmRecoil()
    {       
        CameraLookAt.rotation = Quaternion.Euler(CameraLookAt.eulerAngles.x - recoilBackForce, CameraLookAt.eulerAngles.y, 0);
    }

    //private void GameStopping()
    //{
    //    if (isGameStop > 0)
    //    {
    //        Cursor.visible = true;
    //        Cursor.lockState = CursorLockMode.None;
    //        Time.timeScale = 0f;
    //    }
    //    else
    //    {
    //        Cursor.visible = false;
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Time.timeScale = 1f;
    //    }
    //}

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
        if (info.isDead) return;
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
            float errorFloat;
            if (!animator.GetBool(hashZoomOn)) errorFloat = attackAccuracy;
            else errorFloat = 0;

            float errorRange_x = Random.Range(-errorFloat, errorFloat);
            float errorRange_y = Random.Range(-errorFloat, errorFloat);
            float errorRange_z = Random.Range(-errorFloat, errorFloat);

            debugTransform.position = hit.point;

            if (Physics.Raycast(GunFireStartPoint.position, (hit.point - GunFireStartPoint.position + new Vector3(errorRange_x, errorRange_y, errorRange_z)).normalized, out RaycastHit hits, 30f, FireColliderLayerMask))
            {                
                //Debug.DrawLine(GunFireStartPoint.position, hits.point, Color.red);

                //Debug.Log(hits.transform.gameObject.name);
                //데미지 부여
                if (hits.transform.gameObject.layer == LayerMask.NameToLayer("Monster"))
                {
                    if (hits.transform.CompareTag("Monster"))
                    {
                        if (hits.transform.CompareTag("MonsterHead"))
                        {
                            hits.transform.GetComponent<Monster>().Hurt(info.Attack);
                        }

                        hits.transform.GetComponent<Monster>().Hurt(info.Attack);
                    }
                    else if (hits.transform.CompareTag("Boss"))
                    {
                        if (hits.transform.CompareTag("MonsterHead"))
                        {
                            hits.transform.GetComponent<BossMonster>().Hurt(info.Attack);
                        }

                        hits.transform.GetComponent<BossMonster>().Hurt(info.Attack);
                    }

                    BloodEffectManger.Instance.UsingEffect(hits.point);

                }
                else if (hits.transform.CompareTag("Barrel"))
                {
                    Debug.Log(hit.transform.gameObject.name);
                    hits.transform.GetComponent<Barrel>().Hurt();
                }
                else if (hits.transform.CompareTag("MonsterSpawner"))
                {
                    hits.transform.GetComponent<MonsterSpawnCrystal>().Hurt(info.Attack);
                }
            }

        }
        else
        {
            var cameraTransform = Camera.main.transform;
            debugTransform.position = cameraTransform.position + cameraTransform.forward * 30f;
        }
    }

    private void FireGun()
    {
        if (buildSystem.BuildModeOn > 0f) return;
        if (animator.GetBool(hashReload)) return;
        
        if (isFire && AttackAble)
        {            
            AttackAble = false;
            AttackTimer = AttackDelayTime;
            notAimingTimer = 0;
            animator.SetBool(hashFire, true);
            cameraLerftime = 0.5f;
        }
        //else
        //{
        //    cameralerftime = 0.1f;
        //}
    }

    public void CameraLerftimeEnd()
    {
        cameraLerftime = 1f;
    }

    public void FireEnd()
    {        
        animator.SetBool(hashFire, false);
        cameraLerftime = 1f;
        //cameralerftime = 1f;
    }

    public void Fire()
    {        
        if(info.equipedBulletCount > 0)
        {            
            //데미지 부여
            ShootRay();

            ////카메라 반동
            FirearmRecoil();

            //섬광 파티클 재생
            ParticleSystem.GetComponent<ParticleSystem>().Play();

            //격발 소리 재생
            audioSource.clip = shootAudioClip[0];
            audioSource.Play();

            //탄의 수 감소
            info.equipedBulletCount--;

            //UI 반영
            UI.ChangeFireText(info.equipedBulletCount);
        }
        else
        {
            //빈 탄창 소리 재생
            audioSource.clip = shootAudioClip[1];
            audioSource.Play();
        }
    }


    public void ReloadEnd()
    {
        animator.SetBool(hashReload, false);
        notAimingTimer = 0;
        UI.Reload(info.equipedBulletCount, info.magazineCount);
        //Debug.Log("장전 종료...");
        //Debug.Log($"탄 수 : {info.equipedBulletCount}, 탄창 수 : {info.magazineCount}");
    }

    public void Reloading()
    {
        if(info.magazineCount > 0)
        {
            info.magazineCount--;
            info.equipedBulletCount = info.maxEquipedBulletCount;            
        }
    }

    public void ReloadSoundPlay()
    {
        //재장전 오디오 실행
        audioSource.clip = shootAudioClip[2];
        audioSource.Play();
    }

    private void AttackDelay()
    {
        if(AttackTimer > 0)
        {
            AttackTimer -= Time.fixedDeltaTime;
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
