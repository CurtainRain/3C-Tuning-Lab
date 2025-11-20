using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class SystemInputHandler
{
    private IInputProvider _inputProvider;
    private SystemInputData _currentInput;

    private bool _recordModePressed;
    private bool _recordPlaybackPressed;

    public void Init(IInputProvider inputProvider)
    {
        if(inputProvider == null){
            Debug.LogError("InputHandler: 未找到输入提供者！请添加 KeyboardMouseInputProvider 或其他实现了 IInputProvider 的组件。");
            return;
        }

        _inputProvider = inputProvider;
    }

    public void Destroy()
    {
        _inputProvider = null;
    }

    public void Update()
    {
        // 瞬时输入需要每帧记录状态等待fixedUpdate更新
        _recordModePressed |= _inputProvider.GetRecordModeInput();
        _recordPlaybackPressed |= _inputProvider.GetRecordPlaybackInput();
    }

    public void FixedUpdate()
    {
        // 每帧更新输入数据
        if (_inputProvider == null) return;

        // 收集所有输入
        _currentInput = new SystemInputData
        {
            recordModePressed = _recordModePressed,
            recordPlaybackPressed = _recordPlaybackPressed
        };

        // 消耗输入
        _recordModePressed = false;
        _recordPlaybackPressed = false;
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public SystemInputData GetInputData()
    {
        return _currentInput;
    }
}
