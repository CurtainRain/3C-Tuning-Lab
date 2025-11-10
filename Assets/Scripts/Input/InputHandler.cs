using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("输入提供者")]
    [SerializeField] private MonoBehaviour inputProvider;

    private IInputProvider _inputProvider;
    private InputData _currentInput;

    // 事件：当输入更新时触发
    public System.Action<InputData> OnInputUpdated;


    private void Awake()
    {
        if( inputProvider == null || !(inputProvider is IInputProvider provider)){
            Debug.LogError("InputHandler: 未找到输入提供者！请添加 KeyboardMouseInputProvider 或其他实现了 IInputProvider 的组件。");
            return;
        }

        _inputProvider = provider;
    }


    private void Update()
    {
        // 每帧更新输入数据
        if (_inputProvider == null) return;

        // 收集所有输入
        _currentInput = new InputData
        {
            moveInput = _inputProvider.GetMoveInput(),
            lookInput = _inputProvider.GetLookInput(),
            zoomInput = _inputProvider.GetZoomInput(),
            jumpPressed = _inputProvider.GetJumpInput(),
            sprintHeld = _inputProvider.GetSprintInput(),
            interactPressed = _inputProvider.GetInteractInput()
        };

        // 通知输入更新
        OnInputUpdated?.Invoke(_currentInput);
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public InputData GetInputData()
    {
        return _currentInput;
    }
}
