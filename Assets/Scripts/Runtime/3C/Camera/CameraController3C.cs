using Runtime.Const.Enums;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 3C 摄像机控制器
/// 负责处理摄像机跟随和旋转逻辑
/// 不直接读取输入，而是通过 InputHandler 获取输入数据
/// </summary>
public class CameraController3C : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target; // 跟随的角色

    [Header("摄像机设置")]
    [SerializeField] private CameraController3CParams _cameraController3CParams;

    private float _verticalRotationVelocity;
    private float _horizontalRotationVelocity;

    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float currentZoom = 20f;
    private float currentTargetZoom = 20f;


    private PlayerInputHandler _inputHandler;
    private RecordPlaybackInputHandler _recordPlaybackInputHandler;

    private void Start()
    {
        // 如果没有指定目标，报错并返回
        Debug.Log(gameObject.name + " CameraController3C: 开始初始化");
        if (target == null)
        {
           Debug.LogError("CameraController3C: 未指定跟随目标，将无法进行跟随！");
           return;
        }

        if(_cameraController3CParams == null)
        {
            Debug.LogError("CameraController3C: 未设置 CameraController3CParams，将无法进行跟随！");
            return;
        }

        var comp = target.GetComponent<CharacterController3C>();
        if(comp != null)
        {
            comp.cameraTransform = transform;
        }

        _inputHandler = GameRuntimeContext.Instance.playerInputHandler;
        if (_inputHandler == null)
        {
            Debug.LogError("CameraController3C: 未找到 InputHandler，将无法接收输入！");
            return;
        }

        _recordPlaybackInputHandler = GameRuntimeContext.Instance.recordPlaybackInputHandler;
        if (_recordPlaybackInputHandler == null)
        {
            Debug.LogError("CameraController3C: 未找到 RecordPlaybackInputHandler，将无法接收输入！");
            return;
        }

        // 初始化摄像机旋转和位置
        currentZoom = 20f;
        transform.rotation = target.rotation;
        transform.position = target.position - transform.forward * currentZoom;
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;


        // 注册回放开始监听
        _recordPlaybackInputHandler.OnPlaybackStart += OnPlaybackStartReceived;
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (_inputHandler != null)
        {
            _inputHandler = null;
        }

        if (_recordPlaybackInputHandler != null)
        {
            _recordPlaybackInputHandler.OnPlaybackStart -= OnPlaybackStartReceived;
            _recordPlaybackInputHandler = null;
        }
    }

    private void OnPlaybackStartReceived(PlayerInputDataCollection collection){
        var initialCameraData = collection.initialCameraData;
        ApplyData(initialCameraData);
    }

    private void FixedUpdate()
    {
        if (GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode != GameRunningMode.PlayMode){
            var inputData = GameRuntimeContext.Instance.GetInputData();
            var lookInput = inputData.lookInput;

            HandleRotation(lookInput);
        }
    }


    private void LateUpdate()
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;

        var inputData = GameRuntimeContext.Instance.GetInputData();
        var lookInput = inputData.lookInput;
        var zoomInput = inputData.zoomInput;

        // 处理视角旋转
        if (GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode == GameRunningMode.PlayMode){
            HandleRotation(lookInput);
        }

        // 处理摄像机跟随
        HandleFollow(Time.deltaTime, zoomInput);
    }

    private void HandleRotation(Vector3 lookInput)
    {
        // 水平旋转（Y轴）
        var finalTargetYaw = currentYaw;
        if (Mathf.Abs(lookInput.x) > 0.01f)
        {
            var targetYaw = currentYaw + lookInput.x * _cameraController3CParams.mouseSensitivity;
            targetYaw = Mathf.SmoothDampAngle(
                currentYaw,
                targetYaw,
                ref _horizontalRotationVelocity,
                _cameraController3CParams.rotationSmoothTime
            );
            finalTargetYaw = targetYaw;
        }

        // 垂直旋转（X轴）
        var finalTargetPitch = currentPitch;
        if (Mathf.Abs(lookInput.y) > 0.01f)
        {
            var targetPitch = currentPitch - lookInput.y * _cameraController3CParams.mouseSensitivity;
            targetPitch = Mathf.SmoothDampAngle(
                currentPitch,
                targetPitch,
                ref _verticalRotationVelocity,
                _cameraController3CParams.rotationSmoothTime
            );

            finalTargetPitch = Mathf.Clamp(targetPitch, _cameraController3CParams.minVerticalAngle, _cameraController3CParams.maxVerticalAngle);
        }

        // 应用旋转到摄像机
        currentYaw = finalTargetYaw;
        currentPitch = finalTargetPitch;
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
    }

    private void HandleFollow(float dt, float zoomInput)
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;


        if(Mathf.Abs(zoomInput) > 0.01f)
        {
            currentTargetZoom -= zoomInput * _cameraController3CParams.zoomSensitivity;
            currentTargetZoom = Mathf.Clamp(currentTargetZoom, _cameraController3CParams.minZoom, _cameraController3CParams.maxZoom);
        }

        currentZoom = Mathf.Lerp(currentZoom, currentTargetZoom, dt * _cameraController3CParams.zoomSmoothFactor);

        // 计算目标位置
        Vector3 targetPosition = target.position - transform.forward * currentZoom;
        transform.position = targetPosition;
    }

    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        var comp = target.GetComponent<CharacterController3C>();

        if(comp != null)
        {
            comp.cameraTransform = transform;
        }


        // 设置摄像机旋转和位置
        currentZoom = 20f;
        transform.rotation = target.rotation;
        transform.position = target.position - transform.forward * currentZoom;
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
    }

    public DataOf3C_Camera GetData(){
        return new DataOf3C_Camera{
            position = transform.position,
            rotation = transform.rotation,
            zoom = currentZoom
        };
    }

    private void ApplyData(DataOf3C_Camera data){
        // 更新旋转相关的内部状态
        currentYaw = data.rotation.eulerAngles.y;
        currentPitch = data.rotation.eulerAngles.x;

        // 更新缩放相关的内部状态
        currentZoom = data.zoom;
        currentTargetZoom = data.zoom;

        // 应用旋转和位置
        transform.rotation = data.rotation;
        transform.position = data.position;
    }

    public string getPresetName(){
        return _cameraController3CParams.name;
    }
}
