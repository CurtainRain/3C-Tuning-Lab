using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController3CParams))]
class CameraController3CParamGUIEditor:Editor
{
    SerializedProperty rotationSmoothTime = null;
    SerializedProperty zoomSmoothFactor = null;
    SerializedProperty mouseSensitivity = null;
    SerializedProperty minVerticalAngle = null;
    SerializedProperty maxVerticalAngle = null;
    SerializedProperty zoomSensitivity = null;
    SerializedProperty minZoom = null;
    SerializedProperty maxZoom = null;
    SerializedProperty cameraRadius = null;
    SerializedProperty initialYaw = null;
    SerializedProperty initialPitch = null;
    SerializedProperty initialZoom = null;

    private void OnEnable()
    {
        rotationSmoothTime = serializedObject.FindProperty("rotationSmoothTime");
        zoomSmoothFactor = serializedObject.FindProperty("zoomSmoothFactor");
        mouseSensitivity = serializedObject.FindProperty("mouseSensitivity");
        minVerticalAngle = serializedObject.FindProperty("minVerticalAngle");
        maxVerticalAngle = serializedObject.FindProperty("maxVerticalAngle");
        zoomSensitivity = serializedObject.FindProperty("zoomSensitivity");
        minZoom = serializedObject.FindProperty("minZoom");
        maxZoom = serializedObject.FindProperty("maxZoom");
        cameraRadius = serializedObject.FindProperty("cameraRadius");
        initialYaw = serializedObject.FindProperty("initialYaw");
        initialPitch = serializedObject.FindProperty("initialPitch");
        initialZoom = serializedObject.FindProperty("initialZoom");
    }

    private void OnDisable()
    {
        rotationSmoothTime = null;
        zoomSmoothFactor = null;
        mouseSensitivity = null;
        minVerticalAngle = null;
        maxVerticalAngle = null;
        zoomSensitivity = null;
        minZoom = null;
        maxZoom = null;
        cameraRadius = null;
        initialYaw = null;
        initialPitch = null;
        initialZoom = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rotationSmoothTime, new GUIContent("旋转平滑时间"));
        EditorGUILayout.PropertyField(zoomSmoothFactor, new GUIContent("缩放平滑因子"));
        EditorGUILayout.PropertyField(mouseSensitivity, new GUIContent("鼠标灵敏度"));
        EditorGUILayout.PropertyField(minVerticalAngle, new GUIContent("垂直视角最小角度"));
        EditorGUILayout.PropertyField(maxVerticalAngle, new GUIContent("垂直视角最大角度"));
        EditorGUILayout.PropertyField(zoomSensitivity, new GUIContent("缩放灵敏度"));
        EditorGUILayout.PropertyField(minZoom, new GUIContent("最小缩放距离"));
        EditorGUILayout.PropertyField(maxZoom, new GUIContent("最大缩放距离"));
        EditorGUILayout.PropertyField(cameraRadius, new GUIContent("摄像机半径"));
        EditorGUILayout.PropertyField(initialYaw, new GUIContent("初始水平视角"));
        EditorGUILayout.PropertyField(initialPitch, new GUIContent("初始垂直视角"));
        EditorGUILayout.PropertyField(initialZoom, new GUIContent("初始缩放距离"));

        serializedObject.ApplyModifiedProperties();
    }
}
