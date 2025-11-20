using System.Collections.Generic;
using Runtime.Const.Enums;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 回放输入处理器，负责回播录制的输入数据
/// </summary>
public class RecordPlaybackInputHandler
{
    // 事件：当输入更新时触发
    public System.Action<PlayerInputDataCollection> OnPlaybackStart;
    private GameRuntimeContext _gameRuntimeContext;
    private bool _isPlaybacking = false;
    private PlayerInputDataCollection _recordByInputDatasCollection;
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
        OnPlaybackStart = null;
    }

    public void FixedUpdate(){
        // 为本帧准备回放输入数据
        if(_gameRuntimeContext.gameRunningModeSwitcher.currentRunningMode != GameRunningMode.RecordPlaybackMode){
            _isPlaybacking = false;
            return;
        }

        if(!_isPlaybacking){
            StartPlayback();
            _isPlaybacking = true;
        }else{
            UpdatePlayback();
        }

        if(_recordByInputDatasCollection == null || _recordByInputDatasCollection.inputDatas.Count == 0){
            EndPlayback();
        }
    }

    void StartPlayback(){
        var collection = _gameRuntimeContext.iOPlayerInputDataService.GetInputDataCollection();
        if(collection != null && collection.inputDatas != null){
            _recordByInputDatasCollection = collection;
            OnPlaybackStart?.Invoke(_recordByInputDatasCollection);
        }else{
            Debug.LogError("RecordPlaybackInputHandler: 未能读取录制数据, 无法回放");
        }
    }

    void UpdatePlayback(){
        var item = _recordByInputDatasCollection.inputDatas[0];
        _recordByInputDatasCollection.inputDatas.RemoveAt(0);
        _currentInput = item;
    }

    void EndPlayback(){
        _gameRuntimeContext.gameRunningModeSwitcher.SwitchToMode(GameRunningMode.PlayMode);
    }

    /// <summary>
    /// 获取当前输入数据（供其他组件主动读取）
    /// </summary>
    public PlayerInputData GetInputData()
    {
        return _currentInput;
    }
}
