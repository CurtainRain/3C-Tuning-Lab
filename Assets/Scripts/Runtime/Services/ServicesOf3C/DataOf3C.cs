using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatasOf3C{
    public List<DataOf3C> dataOf3CList = new List<DataOf3C>();
}

[System.Serializable]
public class DataOf3C{
    public int frame;
    public float time;
    public DataOf3C_Camera camera;
    public DataOf3C_Player player;
}

[System.Serializable]
public class DataOf3C_Camera{
    public Vector3 position;
    public Quaternion rotation;
    public float zoom;
}

[System.Serializable]
public class DataOf3C_Player{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}
