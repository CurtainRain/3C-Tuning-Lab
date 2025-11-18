using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class PlayerInputHandler
{
    private PlayerInputData _currentInput;
    private GameRuntimeContext _gameRuntimeContext;

    public void Init(GameRuntimeContext gameRuntimeContext)
    {
        _gameRuntimeContext = gameRuntimeContext;
        if(_gameRuntimeContext == null){
            Debug.LogError("PlayerInputHandler: 未找到 GameRuntimeContext，将无法接收模式切换！");
            return;
        }
    }

    public void Destroy()
    {
        _gameRuntimeContext = null;
    }


    public void Tick()
    {
        // 每帧更新输入数据
        if (_gameRuntimeContext.inputProvider == null) return;

        // 收集所有输入
        _currentInput = new PlayerInputData
        {
            moveInput = _gameRuntimeContext.inputProvider.GetMoveInput(),
            lookInput = _gameRuntimeContext.inputProvider.GetLookInput(),
            zoomInput = _gameRuntimeContext.inputProvider.GetZoomInput(),
            jumpPressed = _gameRuntimeContext.inputProvider.GetJumpInput(),
            sprintHeld = _gameRuntimeContext.inputProvider.GetSprintInput(),
            interactPressed = _gameRuntimeContext.inputProvider.GetInteractInput(),
        };
    }


    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public PlayerInputData GetInputData()
    {
        return _currentInput;
    }
}
