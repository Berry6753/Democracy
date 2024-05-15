using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    //[Header("ī�޶�")]
    //[SerializeField]
    //private CinemachineFreeLook cameraTransform;

    //[Header("ī�޶� LookAt")]
    //[SerializeField]
    //private Transform cameraLookAt;

    private Rigidbody rb;

    private Vector2 InputDir;
    private Vector3 inputMoveDir;
    private Vector3 playerMoveDir;

    private bool InputBool;
    private bool isRunning;

    [Space(10)]
    [Header("�⺻ �̵� �ӵ�")]
    [SerializeField]
    private float DefaultSpeed;

    [Space(10)]
    [Header("�޸��� �̵� �ӵ�")]
    [SerializeField]
    private float RunSpeed;

    [Space(10)]
    [Header("�׶��� Ȯ�� overlap")]
    [SerializeField]
    private Transform overlapPos;

    [Space(10)]
    [Header("���� Power")]
    [SerializeField]
    private float JumpPower;

    [Space(10)]
    [Header("�߷� ���ӵ�")]
    [SerializeField]
    private float gravityMultiplier;

    //�߷� ��
    private float gravity = -9.81f;
    //���� �߷� ���ӵ�
    private float _velocity;

    private float moveSpeed;

    private Animator animator;
    private float animationFloat;

    private float targetAngle;

    // private Animator animator;

    [Space(10)]
    [Header("ĳ���� ȸ�� �ӵ�")]
    [SerializeField]
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private readonly int hashMoveZ = Animator.StringToHash("Z_Speed");
    private readonly int hashMoveX = Animator.StringToHash("X_Speed");

    [SerializeField]
    private LayerMask gravityLayermask;

    //���� Ȯ�ο� overlapCollider
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        _velocity = 0f;
        //animator = GetComponent<Animator>();
        
    }

    private void Start()
    {
        //if (isLocalPlayer == false) return;
        //cameraTransform = GameObject.FindWithTag("PlayerCamera").GetComponent<CinemachineFreeLook>();

        //cameraTransform.Follow = this.transform;
        //cameraTransform.LookAt = cameraLookAt;
    }

    // Update is called once per frame
    void Update()
    {
        //Gravity();
        ChangeSpeed();
        Rotation();       

        //Debug.Log(cameraTransform.rotation);
        //Debug.Log(transform.forward);
    }

    private void FixedUpdate()
    {
        Gravity();
        Movement();
    }

    // InputSystem �����Ʈ
    public void OnMovement(InputAction.CallbackContext context)
    {
        InputDir = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        InputBool = context.ReadValue<float>() > 0.5f;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (colliders.Length <= 0) return;
        if (context.performed)
        {
            _velocity += JumpPower;
            Debug.Log($"���� Velocity ; {_velocity}");
        }
    }

    private void Gravity()
    {
        colliders = Physics.OverlapBox(overlapPos.position, new Vector3(0.3f,0.1f,0.3f), Quaternion.identity, gravityLayermask);
        if (colliders.Length > 0 && _velocity < 0.0f)
        {
            Debug.Log("isGrounded");
            _velocity = -1f;
        }
        else
        {
            Debug.Log("fly");
            _velocity += gravity * Time.deltaTime;
        }

        characterController.Move(new Vector3(0,_velocity,0) * Time.deltaTime);
    }

    private void Movement()
    {
        inputMoveDir = new Vector3(InputDir.x, 0, InputDir.y).normalized;

        if (inputMoveDir.magnitude >= 0.1f)
        {            
            targetAngle = Mathf.Atan2(inputMoveDir.x, inputMoveDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            playerMoveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            characterController.Move(playerMoveDir.normalized * moveSpeed * Time.deltaTime);
            //rb.AddForce(playerMoveDir.normalized * moveSpeed * Time.deltaTime);
            Debug.Log("������");
            //transform.position += playerMoveDir.normalized * moveSpeed * Time.deltaTime;
        }

        MoveAnimation();
        animator.SetFloat(hashMoveX, Mathf.Lerp(animator.GetFloat(hashMoveX), InputDir.y * animationFloat, 10f));
        animator.SetFloat(hashMoveZ, Mathf.Lerp(animator.GetFloat(hashMoveZ), InputDir.x * animationFloat, 10f));

        //animator.SetBool("Move", inputMoveDir.magnitude >= 0.1f);
    }

    private void ChangeSpeed()
    {
        if (InputBool)
        {
            moveSpeed = RunSpeed;
        }
        else
        {
            moveSpeed = DefaultSpeed;
        }
    }

    private void MoveAnimation()
    {
        if(inputMoveDir.magnitude < 0.1f)
        {
            animationFloat = 0;
        }
        else if(InputBool)
        {
            animationFloat = 5;
        }
        else
        {
            animationFloat = 3;
        }
    }

    private void Rotation()
    {
        // ī�޶��� ������ ĳ������ ȸ�������� ��ȯ
        Quaternion targetRotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        // ĳ���͸� �ش� ȸ�������� ȸ����Ŵ
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }
}
