using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class PlayerInputHandler
{
    private PlayerInputData _currentInput;
    private GameRuntimeContext _gameRuntimeContext;

    private bool _jumpPressed;
    private bool _interactPressed;
    private float _zoomInput;

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

    public void Update()
    {
        if (_gameRuntimeContext.inputProvider == null) return;

        // 瞬时状态需要每帧记录状态等待fixedUpdate更新
        _jumpPressed |= _gameRuntimeContext.inputProvider.GetJumpInput();
        _interactPressed |= _gameRuntimeContext.inputProvider.GetInteractInput();
        var curFrameZoomInput = _gameRuntimeContext.inputProvider.GetZoomInput();
        if(Mathf.Abs(curFrameZoomInput)>0){
            _zoomInput = curFrameZoomInput;
        }
    }


    public void FixedUpdate()
    {
        // 每帧更新输入数据
        if (_gameRuntimeContext.inputProvider == null) return;

        // 收集所有输入
        _currentInput = new PlayerInputData
        {
            moveInput = _gameRuntimeContext.inputProvider.GetMoveInput(),
            lookInput = _gameRuntimeContext.inputProvider.GetLookInput(),
            zoomInput = _zoomInput,
            jumpPressed = _jumpPressed,
            sprintHeld = _gameRuntimeContext.inputProvider.GetSprintInput(),
            interactPressed = _interactPressed,
        };

        // 消耗输入
        _jumpPressed = false;
        _interactPressed = false;
        _zoomInput = 0f;
    }


    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public PlayerInputData GetInputData()
    {
        return _currentInput;
    }
}
