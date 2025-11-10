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
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _currentSpeed;

    [HideInInspector] public Transform cameraTransform;

    private float _carrentYawSpeed;
    private float _carrentYawSpeedVelocity;

    // 输入数据引用（由 InputHandler 设置）
    private InputData _inputData = InputData.Empty;

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
        _inputData = inputData;
    }

    private void Update()
    {
        // 处理移动
        HandleMovement();

        // 处理跳跃
        HandleJump();

        // 应用重力
        ApplyGravity();

        // 应用移动
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // 根据是否冲刺选择速度
        _currentSpeed = _inputData.sprintHeld ? sprintSpeed : walkSpeed;

        // 走动的时候 以摄像机面朝平面方向转换为世界空间方向
        Vector3 moveDirection = Vector3.zero;
        float targetYaw = _carrentYawSpeed;
        if(cameraTransform != null && (Mathf.Abs(_inputData.moveInput.x) > 0.01f || Mathf.Abs(_inputData.moveInput.y) > 0.01f))
        {
            // 调整朝向
            targetYaw = cameraTransform.eulerAngles.y;

            // 计算移动方向
            moveDirection = transform.right * _inputData.moveInput.x + transform.forward * _inputData.moveInput.y;
        }

        _carrentYawSpeed = Mathf.SmoothDampAngle(_carrentYawSpeed, targetYaw, ref _carrentYawSpeedVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0, _carrentYawSpeed, 0);

        // 应用速度
        _velocity.x = moveDirection.x * _currentSpeed;
        _velocity.z = moveDirection.z * _currentSpeed;
    }

    private void HandleJump()
    {
        if (_inputData.jumpPressed)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
    }
}
