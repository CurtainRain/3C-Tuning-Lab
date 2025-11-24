using System.Collections;
using Runtime.Const.Enums;
using UnityEngine;

/// <summary>
/// 移动输出器
/// 负责输出移动数据到文件
/// </summary>
public class MovementOutputer : MonoBehaviour
{
    [Header("3C组件")]
    [SerializeField] private CharacterController3C _characterController3C;
    [SerializeField] private CameraController3C _cameraController3C;

    private bool _isOutputting = false;


    private void OnEnable()
    {
        // 查找 InputHandler
        if (_characterController3C == null || _cameraController3C == null)
        {
            Debug.LogError("MovementRecorder: 未找到 CharacterController3C 或 CameraController3C，将无法移动！");
            return;
        }
    }

    private void OnDisable()
    {
        if(_isOutputting){
            GameRuntimeContext.Instance.output3CDataByCSVService.StopOutput3CData();
            _isOutputting = false;
        }
    }

    private void FixedUpdate()
    {
        if (_characterController3C == null || _cameraController3C == null){
            return;
        }

        if(GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode == GameRunningMode.RecordPlaybackMode){
            if(!_isOutputting){
                _isOutputting = true;
                var presetName = _characterController3C.GetPresetName() + "_" + _cameraController3C.GetPresetName();
                GameRuntimeContext.Instance.output3CDataByCSVService.StartOutput3CData(presetName);
            }
            CapturePlayerAndCamera();
        }else{
            if(_isOutputting){
                _isOutputting = false;
                GameRuntimeContext.Instance.output3CDataByCSVService.StopOutput3CData();
            }
        }
    }

    private void CapturePlayerAndCamera()
    {
        var cameraData = _cameraController3C.GetData();
        var playerData = _characterController3C.GetData();
        var inputData = GameRuntimeContext.Instance.GetInputData();
        var dataOf3C = new SnapShot{
            frame = Time.frameCount,
            time = Time.time,
            camera = cameraData,
            player = playerData,
            input = inputData
        };
        GameRuntimeContext.Instance.output3CDataByCSVService.AddDataToOutput3CData(dataOf3C);
    }

}
