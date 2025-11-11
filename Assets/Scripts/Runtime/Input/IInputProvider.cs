using UnityEngine;

/// <summary>
/// 输入提供者接口，用于解耦输入读取逻辑
/// 可以实现不同的输入源：键盘鼠标、手柄、触摸屏等
/// </summary>
public interface IInputProvider
{
    /// <summary>
    /// 获取移动输入（归一化的方向向量）
    /// </summary>
    Vector2 GetMoveInput();

    /// <summary>
    /// 获取视角输入（鼠标/手柄右摇杆）
    /// </summary>
    Vector2 GetLookInput();

    /// <summary>
    /// 获取缩放输入（鼠标滚轮）
    /// </summary>
    float GetZoomInput();

    /// <summary>
    /// 是否按下跳跃键
    /// </summary>
    bool GetJumpInput();

    /// <summary>
    /// 是否按下冲刺键
    /// </summary>
    bool GetSprintInput();

    /// <summary>
    /// 是否按下交互键
    /// </summary>
    bool GetInteractInput();
}
