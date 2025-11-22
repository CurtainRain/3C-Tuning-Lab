using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatasOf3C{
    public List<DataOf3C> dataOf3CList = new List<DataOf3C>();
}

[System.Serializable]
public class DataOf3C{// c-mark:重命名为Snapshot
    public int frame;
    public float time;
    public DataOf3C_Camera camera;
    public DataOf3C_Player player;
    public PlayerInputData input;// c-mark:新增字段 需要在csv输出里支持
}

[System.Serializable]
public class DataOf3C_Camera{
    // c-mark:现在承担了csv输出和回放原始数据两项功能 考虑分开
    public Vector3 position;
    public Quaternion rotation;
    public float zoom;//c-mark:zoom现在记录逻辑值 需要区分下逻辑和渲染值
}

[System.Serializable]
public class DataOf3C_Player{
    // c-mark:现在承担了csv输出和回放原始数据两项功能 考虑分开
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}
