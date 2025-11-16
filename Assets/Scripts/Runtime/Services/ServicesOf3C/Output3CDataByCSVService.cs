using UnityEngine;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// 输出 3C 数据服务
/// 负责输出 3C 数据到文件
/// </summary>
public class Output3CDataByCSVService
{

    private DatasOf3C _datasOf3C;
    private GameRuntimeContext _gameRuntimeContext;
    private string _presetName;

    public static string RecordFolderPath = "Records";
    public static string RecordFileNameTemplate = "Record_";
    public static string RecordFileExtension = ".csv";

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
    public void StartOutput3CData(string PresetName){
        if(_datasOf3C != null){
            Debug.LogError("Output3CDataByCSVService: 已经开始输出 3C 数据，不能重复开始！");
            return;
        }
        if(string.IsNullOrEmpty(PresetName))
        {
            throw new ArgumentException("预设名称不能为空！", nameof(PresetName));
        }

        _presetName = PresetName;
        _datasOf3C = new DatasOf3C();
    }

    //<summary>
    // 添加数据到输出 3C 数据
    // 参数：dataOf3C - 数据(不能为空)
    //</summary>
    public void AddDataToOutput3CData(DataOf3C dataOf3C){
        if (dataOf3C == null){
            throw new ArgumentException("数据不能为空！", nameof(dataOf3C));
        }

        _datasOf3C.dataOf3CList.Add(dataOf3C);
    }

    //<summary>
    // 停止输出 3C 数据并落盘保存
    //</summary>
    public void StopOutput3CData(){
        if(_datasOf3C == null){
            Debug.LogError("Output3CDataByCSVService: 还没有开始输出 3C 数据，无需停止！");
            return;
        }

        if(_gameRuntimeContext == null || _gameRuntimeContext.storageService == null){
            Debug.LogError("Output3CDataByCSVService: StorageService 无效，无法保存 CSV！");
            return;
        }

        var path = Path.Combine(RecordFolderPath, $"{RecordFileNameTemplate}{_presetName}{RecordFileExtension}");
        var csvContent = BuildCSV(_datasOf3C);
        _gameRuntimeContext.storageService.WriteText(path, csvContent);
        _datasOf3C = null;
        _presetName = null;
    }

    private static readonly CultureInfo _csvCulture = CultureInfo.InvariantCulture;
    private static readonly string _formatString = "F6";
    private static readonly (string header, Func<DataOf3C, string> getter)[] _csvColumns = new (string, Func<DataOf3C, string>)[]{
        ("Frame", d => d.frame.ToString()),
        ("Time", d => d.time.ToString(_formatString, _csvCulture)),

        ("CameraPosX", d => d.camera.position.x.ToString(_formatString, _csvCulture)),
        ("CameraPosY", d => d.camera.position.y.ToString(_formatString, _csvCulture)),
        ("CameraPosZ", d => d.camera.position.z.ToString(_formatString, _csvCulture)),

        ("CameraRotX", d => d.camera.rotation.x.ToString(_formatString, _csvCulture)),
        ("CameraRotY", d => d.camera.rotation.y.ToString(_formatString, _csvCulture)),
        ("CameraRotZ", d => d.camera.rotation.z.ToString(_formatString, _csvCulture)),
        ("CameraRotW", d => d.camera.rotation.w.ToString(_formatString, _csvCulture)),

        ("CameraZoom", d => d.camera.zoom.ToString(_formatString, _csvCulture)),

        ("PlayerPosX", d => d.player.position.x.ToString(_formatString, _csvCulture)),
        ("PlayerPosY", d => d.player.position.y.ToString(_formatString, _csvCulture)),
        ("PlayerPosZ", d => d.player.position.z.ToString(_formatString, _csvCulture)),

        ("PlayerRotX", d => d.player.rotation.x.ToString(_formatString, _csvCulture)),
        ("PlayerRotY", d => d.player.rotation.y.ToString(_formatString, _csvCulture)),
        ("PlayerRotZ", d => d.player.rotation.z.ToString(_formatString, _csvCulture)),
        ("PlayerRotW", d => d.player.rotation.w.ToString(_formatString, _csvCulture)),

        ("PlayerVelX", d => d.player.velocity.x.ToString(_formatString, _csvCulture)),
        ("PlayerVelY", d => d.player.velocity.y.ToString(_formatString, _csvCulture)),
        ("PlayerVelZ", d => d.player.velocity.z.ToString(_formatString, _csvCulture))
    };

    private string BuildCSV(DatasOf3C datas){
        if(datas == null || datas.dataOf3CList == null){
            throw new ArgumentException("数据不能为空！", nameof(datas));
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", _csvColumns.Select(c => c.header)));

        foreach(var data in datas.dataOf3CList){
            if(data == null){
                continue;
            }

            var columns = _csvColumns.Select(column => column.getter(data));
            sb.AppendLine(string.Join(",", columns));
        }

        return sb.ToString();
    }
}
