using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class SystemInputHandler
{
    private IInputProvider _inputProvider;
    private SystemInputData _currentInput;

    // 事件：当输入更新时触发
    public System.Action<SystemInputData> OnInputUpdated;

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
        OnInputUpdated = null;
    }

    public void Tick()
    {
        // 每帧更新输入数据
        if (_inputProvider == null) return;

        // 收集所有输入
        _currentInput = new SystemInputData
        {
            recordModePressed = _inputProvider.GetRecordModeInput(),
            recordPlaybackPressed = _inputProvider.GetRecordPlaybackInput()
        };

        // 通知输入更新
        OnInputUpdated?.Invoke(_currentInput);
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public SystemInputData GetInputData()
    {
        return _currentInput;
    }
}
