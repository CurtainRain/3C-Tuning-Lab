using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 通用文件存储服务，负责文本/二进制/JSON 的基础读写
/// keyPath 由外部统一管理，可传入不高于基础地址的相对或绝对路径
/// </summary>
public class StorageService
{
    private readonly string _basePath;

    public StorageService(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
           throw new ArgumentException("basePath 不能为空", nameof(basePath));
        }

        Debug.Log("StorageService: basePath = " + basePath);
        _basePath = basePath;
    }

    /// <summary>
    /// 当前使用的基础路径（仅当 keyPath 传相对路径时生效）
    /// </summary>
    public string BasePath => _basePath;

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public bool Exists(string keyPath)
    {
        var fullPath = ResolvePath(keyPath);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// 读取文本内容
    /// </summary>
    public string ReadText(string keyPath)
    {
        var fullPath = ResolvePath(keyPath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return File.ReadAllText(fullPath);
    }

    /// <summary>
    /// 读取二进制内容
    /// <param name="keyPath">文件路径 为空报错</param>
    /// <returns>二进制内容 读取不到文件时返回null</returns>
    /// </summary>
    public byte[] ReadBytes(string keyPath)
    {
        var fullPath = ResolvePath(keyPath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return File.ReadAllBytes(fullPath);
    }

    /// <summary>
    /// 写入文本内容
    /// <param name="keyPath">文件路径 为空报错</param>
    /// <param name="content">文本内容 为空则以空字符串写入</param>
    /// <param name="overwrite">是否覆盖文件 默认覆盖</param>
    /// </summary>
    public void WriteText(string keyPath, string content, bool overwrite = true)
    {
        var fullPath = PrepareForWrite(keyPath, overwrite);
        File.WriteAllText(fullPath, content ?? string.Empty);
    }

    /// <summary>
    /// 写入二进制内容 传入null以空字节数组写入
    /// <param name="keyPath">文件路径 为空报错</param>
    /// <param name="data">二进制内容 为空则以空字节数组写入</param>
    /// <param name="overwrite">是否覆盖文件 默认覆盖</param>
    /// </summary>
    public void WriteBytes(string keyPath, byte[] data, bool overwrite = true)
    {
        var fullPath = PrepareForWrite(keyPath, overwrite);
        File.WriteAllBytes(fullPath, data ?? new byte[0]);
    }

    /// <summary>
    /// 保存对象到 JSON 传入空data会报错
    /// <param name="keyPath">文件路径 为空报错</param>
    /// <param name="data">对象数据 为空报错</param>
    /// <param name="prettyPrint">是否美化JSON 默认不美化</param>
    /// <param name="overwrite">是否覆盖文件 默认覆盖</param>
    /// </summary>
    public void SaveJson<T>(string keyPath, T data, bool prettyPrint = false, bool overwrite = true)
    {
        if (data == null)
        {
            throw new ArgumentException("data 不能为空", nameof(data));
        }

        var json = JsonUtility.ToJson(data, prettyPrint);
        WriteText(keyPath, json, overwrite);
    }

    /// <summary>
    /// 从 JSON 文件加载对象
    /// </summary>
    public T LoadJson<T>(string keyPath)
    {
        var json = ReadText(keyPath);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public void Delete(string keyPath)
    {
        var fullPath = ResolvePath(keyPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string ResolvePath(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
        {
            throw new ArgumentException("keyPath 不能为空");
        }

        // 处理全局路径
        if (Path.IsPathRooted(keyPath)){
            if (!keyPath.StartsWith(_basePath)){
                throw new ArgumentException($"keyPath 不能为绝对路径且不能超出基础路径：{_basePath}，当前路径：{keyPath}");
            }else{
                return keyPath;
            }
        }

        // 处理相对路径
        var normalized = keyPath.Replace('\\', Path.DirectorySeparatorChar)
                                .Replace('/', Path.DirectorySeparatorChar)
                                .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(_basePath, normalized);
    }

    private string PrepareForWrite(string keyPath, bool overwrite)
    {
        var fullPath = ResolvePath(keyPath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!overwrite && File.Exists(fullPath))
        {
            throw new IOException($"文件已存在且未启用 overwrite：{fullPath}");
        }

        return fullPath;
    }
}
