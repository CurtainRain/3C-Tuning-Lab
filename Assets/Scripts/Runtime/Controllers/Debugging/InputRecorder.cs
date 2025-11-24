using System;
using System.Collections;
using Runtime.Const.Enums;
using UnityEngine;

/// <summary>
/// 移动输出器
/// 负责输出移动数据到文件
/// </summary>
public class InputRecorder : MonoBehaviour
{
    [Header("3C组件")]
    [SerializeField] private CharacterController3C _characterController3C;
    [SerializeField] private CameraController3C _cameraController3C;
    private bool _isRecording = false;

    private void OnEnable()
    {
        // 查找 InputHandler
        if(GameRuntimeContext.Instance.playerInputHandler == null){
            throw new Exception("InputRecorder: 未找到 PlayerInputHandler，将无法录制输入数据！");
        }
    }

    private void OnDisable()
    {
        if(_isRecording){
            GameRuntimeContext.Instance.iOPlayerInputDataService.StopRecord();
            _isRecording = false;
        }
    }

    private void FixedUpdate()
    {
        if(_isRecording){
            CaptureInputData();
        }

        if(GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode == GameRunningMode.RecordMode){
            if(!_isRecording){
                _isRecording = true;
                GameRuntimeContext.Instance.iOPlayerInputDataService.StartRecord(_cameraController3C.GetData(), _characterController3C.GetData());
            }
        }else{
            if(_isRecording){
                _isRecording = false;
                GameRuntimeContext.Instance.iOPlayerInputDataService.StopRecord();
            }
        }
    }

    private void CaptureInputData()
    {
        var inputData = GameRuntimeContext.Instance.GetInputData();
        GameRuntimeContext.Instance.iOPlayerInputDataService.AddDataToRecord(inputData);
    }

}
