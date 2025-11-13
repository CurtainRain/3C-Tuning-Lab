using System;
using UnityEngine;

/// <summary>
/// 3C 角色控制器
/// 负责处理角色移动、跳跃等逻辑
/// 不直接读取输入，而是通过 InputHandler 获取输入数据
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CharacterController3C : MonoBehaviour
{
    [Header("角色设置")]
    [SerializeField] private CharacterController3CParams _characterController3CParams;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _currentSpeed;

    [HideInInspector] public Transform cameraTransform;

    private float _carrentYawSpeed;
    private float _carrentYawSpeedVelocity;

    // 可供消费的输入状态
    private Vector2 moveInput;      // 移动输入（WASD/左摇杆）
    private bool jumpPressed;       // 跳跃按下
    private bool sprintHeld;        // 冲刺按住


    private InputHandler _inputHandler;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // 查找 InputHandler
        _inputHandler = FindObjectOfType<InputHandler>();
        if (_inputHandler == null)
        {
            Debug.LogError("CharacterController3C: 未找到 InputHandler，将无法接收输入！");
            return;
        }

        if(_characterController3CParams == null)
        {
            Debug.LogError("CharacterController3C: 未设置 CharacterController3CParams，将无法进行控制！");
            return;
        }

        // 订阅输入更新事件
        Debug.Log("CharacterController3C: 找到 InputHandler，订阅输入更新事件");
        _inputHandler.OnInputUpdated += OnInputReceived;
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (_inputHandler != null)
        {
            _inputHandler.OnInputUpdated -= OnInputReceived;
            _inputHandler = null;
        }
    }

    /// <summary>
    /// 接收输入数据（由 InputHandler 调用）
    /// </summary>
    private void OnInputReceived(InputData inputData)
    {
        moveInput = inputData.moveInput;
        sprintHeld  = inputData.sprintHeld;

        // 瞬时输入需要累加 并在FixedUpdate中消费
        jumpPressed |= inputData.jumpPressed;
    }

    private void FixedUpdate()
    {
        if (_characterController3CParams == null) return;

        // 处理移动
        HandleMovement();

        // 处理跳跃
        HandleJump();

        // 应用重力
        ApplyGravity();

        // 应用移动
        _characterController.Move(_velocity * Time.fixedDeltaTime);
    }

    private void HandleMovement()
    {
        // 根据是否冲刺选择速度
        _currentSpeed = sprintHeld ? _characterController3CParams.sprintSpeed : _characterController3CParams.walkSpeed;

        // 走动的时候 以摄像机面朝平面方向转换为世界空间方向
        Vector3 moveDirection = Vector3.zero;
        float targetYaw = _carrentYawSpeed;
        if(cameraTransform != null && (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f))
        {
            // 调整朝向
            targetYaw = cameraTransform.eulerAngles.y;

            // 计算移动方向
            moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        }

        _carrentYawSpeed = Mathf.SmoothDampAngle(_carrentYawSpeed, targetYaw, ref _carrentYawSpeedVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0, _carrentYawSpeed, 0);

        // 应用速度
        _velocity.x = moveDirection.x * _currentSpeed;
        _velocity.z = moveDirection.z * _currentSpeed;
    }

    private void HandleJump()
    {
        if (jumpPressed)
        {
            _velocity.y = Mathf.Sqrt(_characterController3CParams.jumpHeight * -2f * Physics.gravity.y * _characterController3CParams.gravityFactor);
            jumpPressed = false;
        }

    }

    private void ApplyGravity()
    {
        _velocity.y += Physics.gravity.y * _characterController3CParams.gravityFactor * Time.fixedDeltaTime;
    }
}
