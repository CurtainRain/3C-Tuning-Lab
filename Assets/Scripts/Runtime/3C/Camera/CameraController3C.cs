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
    // c-mark:加一下射线追踪 避免在碰撞体内/地形下穿过去 此时需要在碰撞点前
    [Header("跟随目标")]
    [SerializeField] private Transform target; // 跟随的角色

    [Header("摄像机设置")]
    [SerializeField] private CameraController3CParams _cameraController3CParams;

    private float _verticalRotationVelocity;
    private float _horizontalRotationVelocity;

    private float targetYaw = 0f;
    private float targetPitch = 0f;
    private float targetZoom = 20f;

    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float currentZoom = 20f;


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
        var inputData = GameRuntimeContext.Instance.GetInputData();
        var lookInput = inputData.lookInput;
        CalculateRotation(lookInput);
        CalculateZoom(inputData.zoomInput);
    }


    private void LateUpdate()
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;

        var dt = Time.deltaTime;

        // 处理视角旋转
        HandleRot(dt);
        HandleFollowAndZoom(dt);
    }

    private void CalculateRotation(Vector3 lookInput)
    {
        // 水平旋转（Y轴）
        if (Mathf.Abs(lookInput.x) > 0.01f)
        {
            targetYaw = currentYaw + lookInput.x * _cameraController3CParams.mouseSensitivity;
        }

        // 垂直旋转（X轴）
        if (Mathf.Abs(lookInput.y) > 0.01f)
        {
            targetPitch = currentPitch - lookInput.y * _cameraController3CParams.mouseSensitivity;
        }
    }

    private void HandleRot(float dt){
        var finalTargetYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref _horizontalRotationVelocity,
            _cameraController3CParams.rotationSmoothTime,
            float.PositiveInfinity,
            dt
        );

        var finalTargetPitch = Mathf.SmoothDampAngle(
            currentPitch,
            targetPitch,
            ref _verticalRotationVelocity,
            _cameraController3CParams.rotationSmoothTime,
            float.PositiveInfinity,
            dt
        );
        finalTargetPitch = Mathf.Clamp(finalTargetPitch, _cameraController3CParams.minVerticalAngle, _cameraController3CParams.maxVerticalAngle);

        // 应用旋转到摄像机
        currentYaw = finalTargetYaw;
        currentPitch = finalTargetPitch;
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
    }

    private void CalculateZoom(float zoomInput)
    {
        if(Mathf.Abs(zoomInput) > 0.01f)
        {
            targetZoom -= zoomInput * _cameraController3CParams.zoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, _cameraController3CParams.minZoom, _cameraController3CParams.maxZoom);
        }
    }

    private void HandleFollowAndZoom(float dt)
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, dt * _cameraController3CParams.zoomSmoothFactor);

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
        // c-mark:这里需要核查一下

        // 更新旋转相关的内部状态
        targetYaw = data.rotation.eulerAngles.y;
        targetPitch = data.rotation.eulerAngles.x;
        currentYaw = data.rotation.eulerAngles.y;
        currentPitch = data.rotation.eulerAngles.x;

        // 更新缩放相关的内部状态
        targetZoom = data.zoom;
        currentZoom = data.zoom;

        // 清空插值过程状态
        _horizontalRotationVelocity = 0f;
        _verticalRotationVelocity = 0f;

        // 应用旋转和位置
        transform.rotation = data.rotation;
        transform.position = data.position;
    }

    public string getPresetName(){
        return _cameraController3CParams.name;
    }
}
