using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnapShotCollection{
    public List<SnapShot> dataOf3CList = new List<SnapShot>();
}

[System.Serializable]
public class SnapShot{
    public int frame;
    public float time;
    public SnapShot_Camera camera;
    public SnapShot_Player player;
    public PlayerInputData input;
}

[System.Serializable]
public class SnapShot_Camera{
    // TODO 现在承担了csv输出和回放原始数据两项功能 考虑分开
    public Vector3 position;
    public Quaternion rotation;
    public float zoom;
}

[System.Serializable]
public class SnapShot_Player{
    // TODO 现在承担了csv输出和回放原始数据两项功能 考虑分开
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}
