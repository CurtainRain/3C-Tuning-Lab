using UnityEngine;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

[System.Serializable]
public class PlayerInputDataCollection{
    public List<PlayerInputData> inputDatas = new List<PlayerInputData>(0);

    public DataOf3C_Camera initialCameraData;
    public DataOf3C_Player initialPlayerData;
}

/// <summary>
/// 输出 3C 数据服务
/// 负责输出 3C 数据到文件
/// </summary>
public class IOPlayerInputDataRecordService
{
    public static string RecordFilePath = "Records/Record.txt";

    private GameRuntimeContext _gameRuntimeContext;
    private List<PlayerInputData> _recordByInputDatas;
    private DataOf3C_Camera _initialCameraData;
    private DataOf3C_Player _initialPlayerData;


    public void Init(GameRuntimeContext runtimeContext){
       _gameRuntimeContext = runtimeContext;
    }

    public void Destroy(){
        _gameRuntimeContext = null;
    }

    //<summary>
    // 开始输出 3C 数据
    // 参数：PresetName - 预设名称(不能为空)
    //</summary>
    public void StartRecord(DataOf3C_Camera cameraData, DataOf3C_Player playerData){
        if(_recordByInputDatas != null){
            Debug.LogError("Output3CDataByCSVService: 已经开始输出 3C 数据，不能重复开始！");
            return;
        }

        Debug.Log("IOPlayerInputDataRecordService: 开始录制输入数据");
        _recordByInputDatas = new List<PlayerInputData>();
        _initialCameraData = cameraData;
        _initialPlayerData = playerData;
    }

    //<summary>
    // 添加数据到输出 3C 数据
    // 参数：dataOf3C - 数据(不能为空)
    //</summary>
    public void AddDataToRecord(PlayerInputData inputData){
        if(_recordByInputDatas == null){
            Debug.LogError("IOPlayerInputDataService: 还没有开始输出 3C 数据，不能添加数据！");
            return;
        }

        _recordByInputDatas.Add(inputData);
    }

    //<summary>
    // 停止输出 3C 数据并落盘保存
    //</summary>
    public void StopRecord(){
        if(_recordByInputDatas == null){
            Debug.LogError("IOPlayerInputDataService: 还没有开始输出 3C 数据，不能添加数据！");
            return;
        }

        if(_gameRuntimeContext == null || _gameRuntimeContext.storageService == null){
            throw new Exception("IOPlayerInputDataService: StorageService 无效，无法保存 CSV！");
        }

        SaveInputDataToFile(new PlayerInputDataCollection{
            inputDatas = _recordByInputDatas,
            initialCameraData = _initialCameraData,
            initialPlayerData = _initialPlayerData,
        });

        _recordByInputDatas = null;
        _initialCameraData = null;
        _initialPlayerData = null;
        Debug.Log("IOPlayerInputDataRecordService: 停止录制输入数据");
    }

    public PlayerInputDataCollection GetInputDataCollection(){
        var PlayerInputDataCollection = ReadInputDataFromFile();
        if(PlayerInputDataCollection == null){
            Debug.Log("RecordPlaybackInputHandler: 读取输入数据失败");
        }else{
            Debug.Log("RecordPlaybackInputHandler: 读取输入数据成功, 数据量: " + PlayerInputDataCollection.inputDatas.Count);
        }

        return PlayerInputDataCollection;
    }

    private void SaveInputDataToFile(PlayerInputDataCollection inputDataCollection){
        _gameRuntimeContext.storageService.SaveJson(RecordFilePath, inputDataCollection, true, true);
        Debug.Log("PlayerInputHandler: 数据保存成功, 数据量: " + inputDataCollection.inputDatas.Count);
    }

    private PlayerInputDataCollection ReadInputDataFromFile(){
        var inputDataCollection = _gameRuntimeContext.storageService.LoadJson<PlayerInputDataCollection>(RecordFilePath);
        return inputDataCollection;
    }
}
