using System.Collections;
using Runtime.Const.Enums;
using UnityEngine;

/// <summary>
/// 移动输出器
/// 负责输出移动数据到文件
/// </summary>
public class MovementOutputer : MonoBehaviour
{
    [Header("角色设置")]
    [SerializeField] private CharacterController3C _characterController3C;
    [SerializeField] private CameraController3C _cameraController3C;

    private Coroutine _recordLoop;

    private bool _isOutputting = false;


    private void OnEnable()
    {
        // 查找 InputHandler
        if (_characterController3C == null || _cameraController3C == null)
        {
            Debug.LogError("MovementRecorder: 未找到 CharacterController3C 或 CameraController3C，将无法移动！");
            return;
        }

        _recordLoop = StartCoroutine(RecordLoop());
    }

    private void OnDisable()
    {
        if(_recordLoop != null){
            StopCoroutine(_recordLoop);
            _recordLoop = null;
        }

        if(_isOutputting){
            GameRuntimeContext.Instance.output3CDataByCSVService.StopOutput3CData();
            _isOutputting = false;
        }
    }

    private IEnumerator RecordLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();  // 等所有 LateUpdate 结束
            if(GameRuntimeContext.Instance.gameRunningModeSwitcher.currentRunningMode == GameRunningMode.RecordPlaybackMode){
                if(!_isOutputting){
                    _isOutputting = true;
                    var presetName = _characterController3C.getPresetName() + "_" + _cameraController3C.getPresetName();
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
    }

    private void CapturePlayerAndCamera()
    {
        var cameraData = _cameraController3C.GetData();
        var playerData = _characterController3C.GetData();
        var dataOf3C = new DataOf3C{
            frame = Time.frameCount,
            time = Time.time,
            camera = cameraData,
            player = playerData
        };
        GameRuntimeContext.Instance.output3CDataByCSVService.AddDataToOutput3CData(dataOf3C);
    }

}
