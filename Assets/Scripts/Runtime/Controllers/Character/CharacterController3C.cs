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
    [SerializeField] private Transform _meshTransform;

    private Vector3 _lastPosition;
    private Vector3 _currPosition;

    private Quaternion _lastRotation;
    private Quaternion _currRotation;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _currentSpeed;

    [HideInInspector] public CameraController3C cameraController3C;

    private float _currentYaw;
    private float _currentYawSpeedVelocity = 0f;


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
            enabled = false; // 禁用组件
            return;
        }

        if(_recordPlaybackInputHandler == null)
        {
            Debug.LogError("CharacterController3C: 未找到 RecordPlaybackInputHandler，将无法接收输入！");
            enabled = false; // 禁用组件
            return;
        }

        if(_characterController3CParams == null)
        {
            Debug.LogError("CharacterController3C: 未设置 CharacterController3CParams，将无法进行控制！");
            enabled = false; // 禁用组件
            return;
        }

        if(_meshTransform == null)
        {
            Debug.LogError("CharacterController3C: 未设置 MeshTransform，将无法移动网格！");
            enabled = false; // 禁用组件
            return;
        }

        // 订阅回放开始事件
        _recordPlaybackInputHandler.OnPlaybackStart += OnPlaybackStartReceived;

        // 初始化位置和旋转
        _currPosition =_lastPosition = transform.position;
        _currRotation = _lastRotation = transform.rotation;
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

    private void FixedUpdate()
    {
        if (_characterController3CParams == null) return;

        var inputData = GameRuntimeContext.Instance.GetInputData();

        var dt = Time.fixedDeltaTime;

        // 处理移动
        HandleMovement(dt, inputData);

        // 处理跳跃
        HandleJump(inputData);

        // 应用重力
        ApplyGravity();

        _lastPosition = _currPosition;
        _lastRotation = _currRotation;

        // 应用移动
        _characterController.Move(_velocity * dt);

        // 更新新的位置和旋转
        _currPosition = transform.position;
        _currRotation = transform.rotation;
    }

    private void Update()
    {
        if(_meshTransform != null)
        {
            var timeRatio = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            // 利用fixedUpdate的帧间隔计算插值比例
            var alpha = Mathf.Clamp01(timeRatio * _characterController3CParams.positionSmoothFactor);
            _meshTransform.position = Vector3.Lerp(_lastPosition, _currPosition, alpha);

            alpha = Mathf.Clamp01(timeRatio * _characterController3CParams.rotationSmoothFactor);
            _meshTransform.rotation = Quaternion.Slerp(_lastRotation, _currRotation, alpha);
        }
    }

    private void HandleMovement(float dt, PlayerInputData inputData)
    {
        // 根据是否冲刺选择速度
        _currentSpeed = inputData.sprintHeld ? _characterController3CParams.sprintSpeed : _characterController3CParams.walkSpeed;

        // 走动的时候 以摄像机面朝平面方向转换为世界空间方向
        Vector3 moveDirection = Vector3.zero;
        float targetYaw = _currentYaw;
        if(cameraController3C != null && (Mathf.Abs(inputData.moveInput.x) > 0.01f || Mathf.Abs(inputData.moveInput.y) > 0.01f))
        {
            // 调整朝向
            targetYaw = cameraController3C.GetLogicYaw();

            // 计算移动方向
            moveDirection = transform.right * inputData.moveInput.x + transform.forward * inputData.moveInput.y;
        }

        _currentYaw = Mathf.SmoothDampAngle(_currentYaw, targetYaw, ref _currentYawSpeedVelocity, 0.1f, float.PositiveInfinity, dt);
        transform.rotation = Quaternion.Euler(0, _currentYaw, 0);

        // 应用速度
        _velocity.x = moveDirection.x * _currentSpeed;
        _velocity.z = moveDirection.z * _currentSpeed;
    }

    private void HandleJump(PlayerInputData inputData)
    {
        if (inputData.jumpPressed)
        {
            _velocity.y = Mathf.Sqrt(_characterController3CParams.jumpHeight * -2f * Physics.gravity.y * _characterController3CParams.gravityFactor);
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += Physics.gravity.y * _characterController3CParams.gravityFactor * Time.fixedDeltaTime;
    }

    public SnapShot_Player GetData(){
        return new SnapShot_Player{
            position = transform.position,
            rotation = transform.rotation,
            velocity = _velocity
        };
    }

    private void ApplyData(SnapShot_Player data){
        var wasEnabled = _characterController.enabled;
        _characterController.enabled = false;

        // 更新旋转相关的内部状态
        _currentYaw = data.rotation.eulerAngles.y;
        _currentYawSpeedVelocity = 0f; // 重置插值速度，避免从旧值插值

        // 更新位置
        _lastPosition = _currPosition = data.position;
        _lastRotation = _currRotation = data.rotation;

        // 更新速度状态
        _velocity = data.velocity;

        // 应用逻辑位置和旋转
        transform.position = data.position;
        transform.rotation = data.rotation;

        // 更新渲染位置和旋转
        _meshTransform.position = _lastPosition;
        _meshTransform.rotation = _lastRotation;

        _characterController.enabled = wasEnabled;
    }

    public string GetPresetName(){
        return _characterController3CParams.name;
    }
}
