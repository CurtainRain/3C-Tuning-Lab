using UnityEngine;


[CreateAssetMenu(menuName = "3C/CameraController3CParams", fileName = "CameraController3CParams")]
public class CameraController3CParams : ScriptableObject
{
    [Header("摄像机设置")]
    [Tooltip("旋转平滑时间，单位：秒")] public float rotationSmoothTime = 0.1f;
    [Tooltip("缩放平滑因子，越小延迟感越强")] public float zoomSmoothFactor = 6f;

    [Header("视角调整")]
    [Tooltip("鼠标灵敏度")] public float mouseSensitivity = 24f;
    [Tooltip("垂直视角最小角度，单位：度")] public float minVerticalAngle = -90f;
    [Tooltip("垂直视角最大角度，单位：度")] public float maxVerticalAngle = 90f;
    [Tooltip("缩放灵敏度")] public float zoomSensitivity = 50f;
    [Tooltip("最小缩放距离")] public float minZoom = 1f;
    [Tooltip("最大缩放距离")] public float maxZoom = 30f;
}
