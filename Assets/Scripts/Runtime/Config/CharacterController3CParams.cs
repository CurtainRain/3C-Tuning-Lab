using UnityEngine;


[CreateAssetMenu(menuName = "3C/CharacterControllerParams", fileName = "CharacterControllerParams")]
public class CharacterController3CParams : ScriptableObject
{
    [Header("移动设置")]
    [Tooltip("行走速度")] public float walkSpeed = 5f;
    [Tooltip("冲刺速度")] public float sprintSpeed = 8f;
    [Tooltip("跳跃高度")] public float jumpHeight = 0.5f;
    [Tooltip("重力系数")] public float gravityFactor = 1f;
    [Tooltip("位置平滑因子")] public float positionSmoothFactor = 1f;
    [Tooltip("旋转平滑因子")] public float rotationSmoothFactor = 1f;
}
