using GameRuntime.CustomUtils;
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

    private float _targetYaw;
    private float _targetPitch;
    private float _targetZoom;

    private float _currentYaw;
    private float _currentPitch;
    private float _currentZoom;

    private LayerMask _thinLayerMask;


    private PlayerInputHandler _inputHandler;
    private RecordPlaybackInputHandler _recordPlaybackInputHandler;

    public float GetLogicYaw()
    {
        return _currentYaw;
    }
    public float GetLogicPitch()
    {
        return _currentPitch;
    }
    public float GetLogicZoom()
    {
        return _currentZoom;
    }

    private void Start()
    {
        // 如果没有指定目标，报错并返回
        Debug.Log(gameObject.name + " CameraController3C: 开始初始化");
        if (target == null)
        {
            Debug.LogError("CameraController3C: 未指定跟随目标，将无法进行跟随！");
            enabled = false; // 禁用组件
            return;
        }

        if(_cameraController3CParams == null)
        {
            Debug.LogError("CameraController3C: 未设置 CameraController3CParams，将无法进行跟随！");
            enabled = false; // 禁用组件
            return;
        }

        var comp = target.GetComponentInParent<CharacterController3C>();
        if(comp != null)
        {
            comp.cameraController3C = this;
        }else{
            Debug.LogError("CameraController3C: 未找到 CharacterController3C，target将无法运动！");
            enabled = false; // 禁用组件
            return;
        }

        _inputHandler = GameRuntimeContext.Instance.playerInputHandler;
        if (_inputHandler == null)
        {
            Debug.LogError("CameraController3C: 未找到 InputHandler，将无法接收输入！");
            enabled = false; // 禁用组件
            return;
        }

        _recordPlaybackInputHandler = GameRuntimeContext.Instance.recordPlaybackInputHandler;
        if (_recordPlaybackInputHandler == null)
        {
            Debug.LogError("CameraController3C: 未找到 RecordPlaybackInputHandler，将无法接收输入！");
            enabled = false; // 禁用组件
            return;
        }

        int thinLayer = LayerMask.NameToLayer("Thin");// 纤细物体需要排除
        _thinLayerMask  = thinLayer >= 0 ? (1 << thinLayer) : 0;

        // 初始化摄像机旋转和位置
        transform.rotation = Quaternion.Euler(_cameraController3CParams.initialPitch, _cameraController3CParams.initialYaw, 0);
        transform.position = target.position - transform.forward * _cameraController3CParams.initialZoom;
        _currentYaw = _targetYaw = _cameraController3CParams.initialYaw;
        _currentPitch = _targetPitch = _cameraController3CParams.initialPitch;
        _currentZoom = _targetZoom = _cameraController3CParams.initialZoom;

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
            _targetYaw = _currentYaw + lookInput.x * _cameraController3CParams.mouseSensitivity;
        }

        // 垂直旋转（X轴）
        if (Mathf.Abs(lookInput.y) > 0.01f)
        {
            _targetPitch = _currentPitch - lookInput.y * _cameraController3CParams.mouseSensitivity;
        }
    }

    private void HandleRot(float dt){
        var finalTargetYaw = Mathf.SmoothDampAngle(
            _currentYaw,
            _targetYaw,
            ref _horizontalRotationVelocity,
            _cameraController3CParams.rotationSmoothTime,
            float.PositiveInfinity,
            dt
        );

        var finalTargetPitch = Mathf.SmoothDampAngle(
            _currentPitch,
            _targetPitch,
            ref _verticalRotationVelocity,
            _cameraController3CParams.rotationSmoothTime,
            float.PositiveInfinity,
            dt
        );
        finalTargetPitch = Mathf.Clamp(finalTargetPitch, _cameraController3CParams.minVerticalAngle, _cameraController3CParams.maxVerticalAngle);

        // 应用旋转到摄像机
        _currentYaw = finalTargetYaw;
        _currentPitch = finalTargetPitch;
        transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
    }

    private void CalculateZoom(float zoomInput)
    {
        if(Mathf.Abs(zoomInput) > 0.01f)
        {
            _targetZoom -= zoomInput * _cameraController3CParams.zoomSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom, _cameraController3CParams.minZoom, _cameraController3CParams.maxZoom);
        }
    }

    private void HandleFollowAndZoom(float dt)
    {
        // 计算zoom
        var cameraPosition = target.position - transform.forward * _targetZoom;

        var ratioOfGround = GetRatioByCollisionWithNormal(target.position, cameraPosition, _cameraController3CParams.cameraRadius);
        var ratioOfThin = GetRatioByCollisionWithThin(target.position, cameraPosition, _cameraController3CParams.cameraRadius);
        var finalRatio = Mathf.Min(ratioOfGround, ratioOfThin);
        var finalZoom = _targetZoom * finalRatio;
        const float maxRatio = 1f;


        if(finalRatio < maxRatio && finalZoom < _currentZoom){
            // 发生了碰撞 且摄像机当前位置已经处于遮挡中 直接赋值避免穿模
            _currentZoom = finalZoom;
        }else{
            // 没有发生碰撞 或者发生了碰撞但摄像机当前位置仍在外部 可以插值
            _currentZoom = Mathf.Lerp(_currentZoom, finalZoom, dt * _cameraController3CParams.zoomSmoothFactor);
        }

        Vector3 position = target.position - transform.forward * _currentZoom;
        transform.position = position;
    }

    private float GetRatioByCollisionWithNormal(Vector3 targetPos, Vector3 cameraPos, float cameraRadius){
        var camToTarDis = Vector3.Distance(cameraPos, targetPos);
        var ratio = 1f;

        // 几乎重合时直接返回
        if (camToTarDis <= Mathf.Epsilon)
        {
            return ratio;
        }

        // 检测起点到终点的射线是否与触发器有碰撞
        var layerMask = ~0 & ~(1 << target.gameObject.layer) & ~_thinLayerMask; // 排除对象所在层和Thin层
        if(Physics.SphereCast(targetPos, cameraRadius, (cameraPos - targetPos).normalized, out var hit, camToTarDis, layerMask))
        {
            ratio = hit.distance / camToTarDis;
        }

        return ratio;
    }

    private float GetRatioByCollisionWithThin(Vector3 targetPos, Vector3 cameraPos, float cameraRadius){
        var camToTarDis = Vector3.Distance(cameraPos, targetPos);
        var ratio = 1f;

        // 几乎重合时直接返回
        if (camToTarDis <= Mathf.Epsilon)
        {
            return ratio;
        }

        // 检测起点到终点的射线是否与普通碰撞体有碰撞
        var layerMask = _thinLayerMask; // 排除对象所在层和地面层
        var isCollision = MathUtil.TryGetEntryPoint(targetPos, cameraPos, layerMask, out var entryPoint, cameraRadius);
        if(isCollision)
        {
            var dis = Vector3.Distance(entryPoint, targetPos);
            dis -= cameraRadius;
            ratio = dis / camToTarDis;
        }

        return ratio;
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
            comp.cameraController3C = this;
        }

        // 设置摄像机旋转和位置
        transform.rotation = target.rotation;
        transform.position = target.position - transform.forward * _targetZoom;
        _currentYaw = transform.eulerAngles.y;
        _currentPitch = transform.eulerAngles.x;
        _currentZoom = _targetZoom = 20f;
    }

    public SnapShot_Camera GetData(){
        return new SnapShot_Camera{
            position = transform.position,
            rotation = transform.rotation,
            zoom = _targetZoom
        };
    }

    private void ApplyData(SnapShot_Camera data){
        // 更新旋转相关的内部状态
        _targetYaw = data.rotation.eulerAngles.y;
        _targetPitch = data.rotation.eulerAngles.x;
        _currentYaw = data.rotation.eulerAngles.y;
        _currentPitch = data.rotation.eulerAngles.x;

        // 更新缩放相关的内部状态
        _currentZoom = _targetZoom = data.zoom;

        // 清空插值过程状态
        _horizontalRotationVelocity = 0f;
        _verticalRotationVelocity = 0f;

        // 应用旋转和位置
        transform.rotation = data.rotation;
        transform.position = data.position;
    }

    public string GetPresetName(){
        return _cameraController3CParams.name;
    }
}
