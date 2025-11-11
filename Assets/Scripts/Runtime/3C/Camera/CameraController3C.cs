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

    // 输入数据引用（由 InputHandler 设置）
    private InputData _inputData;

    private InputHandler _inputHandler;

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

        _inputHandler = FindObjectOfType<InputHandler>();
        if (_inputHandler == null)
        {
            Debug.LogError("CameraController3C: 未找到 InputHandler，将无法接收输入！");
            return;
        }

        // 初始化摄像机旋转和位置
        currentZoom = 20f;
        transform.rotation = target.rotation;
        transform.position = target.position - transform.forward * currentZoom;
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;


        // 查找 InputHandler 并订阅输入更新事件
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

    private void LateUpdate()
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;

        // 处理视角旋转
        HandleRotation();

        // 处理摄像机跟随
        HandleFollow();
    }

    private void HandleRotation()
    {
        // 水平旋转（Y轴）
        var finalTargetYaw = currentYaw;
        if (Mathf.Abs(_inputData.lookInput.x) > 0.01f)
        {
            var targetYaw = currentYaw + _inputData.lookInput.x * _cameraController3CParams.mouseSensitivity;
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
        if (Mathf.Abs(_inputData.lookInput.y) > 0.01f)
        {
            var targetPitch = currentPitch - _inputData.lookInput.y * _cameraController3CParams.mouseSensitivity;
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

    private void HandleFollow()
    {
        if (target == null) return;
        if (_cameraController3CParams == null) return;


        if(Mathf.Abs(_inputData.zoomInput) > 0.01f)
        {
            currentTargetZoom -= _inputData.zoomInput * _cameraController3CParams.zoomSensitivity;
            currentTargetZoom = Mathf.Clamp(currentTargetZoom, _cameraController3CParams.minZoom, _cameraController3CParams.maxZoom);
        }

        currentZoom = Mathf.Lerp(currentZoom, currentTargetZoom, Time.deltaTime * _cameraController3CParams.zoomSmoothFactor);

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
}
