using System;
using Runtime.Const.Enums;
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
    private float _carrentYawSpeedVelocity = 0f;

    // 可供消费的输入状态
    private Vector2 moveInput;      // 移动输入（WASD/左摇杆）
    private bool jumpPressed;       // 跳跃按下
    private bool sprintHeld;        // 冲刺按住
    private GameRunningMode lastFrameMode;


    private PlayerInputHandler _inputHandler;
    private RecordPlaybackInputHandler _recordPlaybackInputHandler;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // 查找 InputHandler
        _inputHandler = GameRuntimeContext.Instance.playerInputHandler;
        _recordPlaybackInputHandler = GameRuntimeContext.Instance.recordPlaybackInputHandler;

        if (_inputHandler == null)
        {
            Debug.LogError("CharacterController3C: 未找到 InputHandler，将无法接收输入！");
            return;
        }

        if(_recordPlaybackInputHandler == null)
        {
            Debug.LogError("CharacterController3C: 未找到 RecordPlaybackInputHandler，将无法接收输入！");
            return;
        }

        if(_characterController3CParams == null)
        {
            Debug.LogError("CharacterController3C: 未设置 CharacterController3CParams，将无法进行控制！");
            return;
        }

        // 订阅回放开始事件
        _recordPlaybackInputHandler.OnPlaybackStart += OnPlaybackStartReceived;
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (_inputHandler != null)
        {
            _inputHandler = null;
        }

        if(_recordPlaybackInputHandler != null)
        {
            _recordPlaybackInputHandler.OnPlaybackStart -= OnPlaybackStartReceived;
            _recordPlaybackInputHandler = null;
        }
    }

    private void OnPlaybackStartReceived(PlayerInputDataCollection collection){
        var initialPlayerData = collection.initialPlayerData;
        ApplyData(initialPlayerData);
    }


    private void Update(){
        PlayerInputData inputData = GameRuntimeContext.Instance.GetInputData();

        if(lastFrameMode != GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode){
            // 更换模式 所以连续记录的状态需要重置
            jumpPressed = false;
        }


        lastFrameMode = GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode;
        moveInput = inputData.moveInput;
        sprintHeld = inputData.sprintHeld;
        jumpPressed |= inputData.jumpPressed;
    }

    //c-mark:现在回放是不稳定的 虽然这里用了fix 但输入仍然在update的dt不一致 导致回放的时候速度不一致
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

    public DataOf3C_Player GetData(){
        return new DataOf3C_Player{
            position = transform.position,
            rotation = transform.rotation,
            velocity = _velocity
        };
    }

    private void ApplyData(DataOf3C_Player data){
        var wasEnabled = _characterController.enabled;
        _characterController.enabled = false;

        // 更新旋转相关的内部状态
        _carrentYawSpeed = data.rotation.eulerAngles.y;
        _carrentYawSpeedVelocity = 0f; // 重置插值速度，避免从旧值插值

        // 更新速度状态
        _velocity = data.velocity;

        // 应用位置和旋转
        transform.position = data.position;
        transform.rotation = data.rotation;

        _characterController.enabled = wasEnabled;
    }

    public string getPresetName(){
        return _characterController3CParams.name;
    }
}
