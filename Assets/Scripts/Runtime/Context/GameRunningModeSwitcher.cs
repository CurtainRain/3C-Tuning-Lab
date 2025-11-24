using UnityEngine;
using Runtime.Const.Enums;

/// <summary>
/// 游戏模式切换器
/// 负责处理游戏模式切换逻辑
/// </summary>
public class GameRunningModeSwitcher
{
    private SystemInputHandler _systemInputHandler;
    public GameRunningMode currentRunningMode { get; private set; } = GameRunningMode.PlayMode;

    public void Init(SystemInputHandler systemInputHandler)
    {
        _systemInputHandler =systemInputHandler ;


        if (_systemInputHandler == null)
        {
            Debug.LogError("GameRunningModeSwitcher: 未找到 SystemInputHandler，将无法接收输入！");
            return;
        }
    }

    public void Destroy()
    {
        // 取消订阅
        if (_systemInputHandler != null)
        {
            _systemInputHandler = null;
        }
    }

    public void SwitchToMode(GameRunningMode gameRunningMode)
    {
        Debug.Log("GameRunningModeSwitcher: 退出模式: " + currentRunningMode);
        currentRunningMode = gameRunningMode;
        Debug.Log("GameRunningModeSwitcher: 进入模式: " + gameRunningMode);
    }

    /// <summary>
    /// 接收输入数据（由 InputHandler 调用）
    /// </summary>
    public void FixedUpdate()
    {
        var inputData = _systemInputHandler.GetInputData();

        if(inputData.recordModePressed)
        {
            if(currentRunningMode == GameRunningMode.PlayMode)
            {
                SwitchToMode(GameRunningMode.RecordMode);
                return;
            }
            else if(currentRunningMode == GameRunningMode.RecordMode)
            {
                SwitchToMode(GameRunningMode.PlayMode);
                return;
            }
            else
            {
                Debug.Log("当前所处模式互斥，无法切换到录制模式，当前模式为：" + currentRunningMode);
                return;
            }
        }

        if(inputData.recordPlaybackPressed)
        {
            if(currentRunningMode == GameRunningMode.PlayMode)
            {
                SwitchToMode(GameRunningMode.RecordPlaybackMode);
                return;
            }
            else if(currentRunningMode == GameRunningMode.RecordPlaybackMode)
            {
                SwitchToMode(GameRunningMode.PlayMode);
                return;
            }
            else
            {
                Debug.Log("当前所处模式互斥，无法切换到录制回放模式，当前模式为：" + currentRunningMode);
                return;
            }
        }
    }
}
