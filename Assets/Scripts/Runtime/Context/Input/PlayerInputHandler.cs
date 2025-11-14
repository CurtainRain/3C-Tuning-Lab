using System.Collections.Generic;
using Runtime.Const.Enums;
using UnityEngine;

/// <summary>
/// 输入处理器，负责收集所有输入并转换为 InputData
/// 作为输入层和逻辑层之间的桥梁
/// </summary>
public class PlayerInputHandler
{
    private PlayerInputData _currentInput;
    private List<PlayerInputData> _recordByInputDatas = new List<PlayerInputData>();
    private GameRuntimeContext _gameRuntimeContext;
    private bool _isRecording = false;

    // 事件：当输入更新时触发
    public System.Action<PlayerInputData> OnInputUpdated;


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
        OnInputUpdated = null;
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

        UpdateRecord();

        if (_gameRuntimeContext.gameRunningModeSwitcher.currentRunningMode != GameRunningMode.RecordPlaybackMode){
            // 如果当前模式非回放模式，则通知输入更新
            OnInputUpdated?.Invoke(_currentInput);
        }
    }

    private void UpdateRecord(){
        if(_gameRuntimeContext.gameRunningModeSwitcher.currentRunningMode != GameRunningMode.RecordMode){
            if(_isRecording){
                this.SaveInputDataToFile(_recordByInputDatas);
                _recordByInputDatas.Clear();
            }

            _isRecording = false;
            return;
        }


        _recordByInputDatas.Add(_currentInput);
        _isRecording = true;
    }

    private void SaveInputDataToFile(List<PlayerInputData> inputDatas){
        _gameRuntimeContext.storageService.SaveJson(RecordPlaybackInputHandler.RecordFilePath, new PlayerInputDataCollection{ inputDatas = inputDatas }, true, true);
        Debug.Log("PlayerInputHandler: 数据保存成功, 数据量: " + inputDatas.Count);
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public PlayerInputData GetInputData()
    {
        return _currentInput;
    }
}
