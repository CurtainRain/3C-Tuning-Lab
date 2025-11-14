using System.Collections.Generic;
using Runtime.Const.Enums;
using UnityEngine;

[System.Serializable]
public class PlayerInputDataCollection{
    public List<PlayerInputData> inputDatas = new List<PlayerInputData>(0);
}



/// <summary>
/// 回放输入处理器，负责回播录制的输入数据
/// </summary>
public class RecordPlaybackInputHandler
{
    public static string RecordFilePath = "Record.txt";

    // 事件：当输入更新时触发
    public System.Action<PlayerInputData> OnInputUpdated;
    private GameRuntimeContext _gameRuntimeContext;
    private bool _isPlaybacking = false;
    private List<PlayerInputData> _recordByInputDatas = new List<PlayerInputData>();
    private PlayerInputData _currentInput;


    public void Init(GameRuntimeContext gameRuntimeContext)
    {
        _gameRuntimeContext = gameRuntimeContext;
        if(_gameRuntimeContext == null){
            Debug.LogError("RecordPlaybackInputHandler: 未找到 GameRuntimeContext，将无法接收模式切换！");
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
        if(_gameRuntimeContext.gameRunningModeSwitcher.currentRunningMode != GameRunningMode.RecordPlaybackMode){
            _isPlaybacking = false;
            return;
        }

        if(!_isPlaybacking){
            var recordByInputDatas = ReadInputDataFromFile();
            if(recordByInputDatas!=null){
                _recordByInputDatas = recordByInputDatas;
            }
        }

        if(_recordByInputDatas.Count == 0){
            _gameRuntimeContext.gameRunningModeSwitcher.SwitchToMode(GameRunningMode.PlayMode);
            _isPlaybacking = false;
            return;
        }

        _isPlaybacking = true;
        var item = _recordByInputDatas[0];
        _recordByInputDatas.RemoveAt(0);
        _currentInput = item;
        OnInputUpdated?.Invoke(_currentInput);
    }

      private List<PlayerInputData> ReadInputDataFromFile(){
        var inputDataCollection = _gameRuntimeContext.storageService.LoadJson<PlayerInputDataCollection>(RecordPlaybackInputHandler.RecordFilePath);
        if(inputDataCollection == null){
            Debug.LogError("RecordPlaybackInputHandler: 读取输入数据失败");
        }else{
            Debug.Log("RecordPlaybackInputHandler: 读取输入数据成功, 数据量: " + inputDataCollection.inputDatas.Count);
        }

        return inputDataCollection.inputDatas;
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public PlayerInputData GetInputData()
    {
        return _currentInput;
    }
}
