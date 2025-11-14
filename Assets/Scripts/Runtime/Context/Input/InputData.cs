using UnityEngine;

/// <summary>
/// 输入数据容器，用于在输入层和逻辑层之间传递数据
/// </summary>
[System.Serializable]
public struct PlayerInputData
{
    public Vector2 moveInput;      // 移动输入（WASD/左摇杆）
    public Vector2 lookInput;      // 视角输入（鼠标/右摇杆）
    public float zoomInput;        // 缩放输入（鼠标滚轮）
    public bool jumpPressed;       // 跳跃按下
    public bool sprintHeld;        // 冲刺按住
    public bool interactPressed;   // 交互按下

    public static PlayerInputData Empty => new PlayerInputData
    {
        moveInput = Vector2.zero,
        lookInput = Vector2.zero,
        zoomInput = 0f,
        jumpPressed = false,
        sprintHeld = false,
        interactPressed = false,
    };
}

public struct SystemInputData
{
    public bool recordModePressed; // 录制开始/结束按下
    public bool recordPlaybackPressed; // 录制回放按下

    public static SystemInputData Empty => new SystemInputData
    {
        recordModePressed = false,
        recordPlaybackPressed = false,
    };
}
