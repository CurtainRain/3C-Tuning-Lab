using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterController3CParams))]
class CharacterController3CParamGUIEditor:Editor
{
    SerializedProperty walkSpeed = null;
    SerializedProperty sprintSpeed = null;
    SerializedProperty jumpHeight = null;
    SerializedProperty gravityFactor = null;
    SerializedProperty positionSmoothFactor = null;
    SerializedProperty rotationSmoothFactor = null;

    private void OnEnable()
    {
        walkSpeed = serializedObject.FindProperty("walkSpeed");
        sprintSpeed = serializedObject.FindProperty("sprintSpeed");
        jumpHeight = serializedObject.FindProperty("jumpHeight");
        gravityFactor = serializedObject.FindProperty("gravityFactor");
        positionSmoothFactor = serializedObject.FindProperty("positionSmoothFactor");
        rotationSmoothFactor = serializedObject.FindProperty("rotationSmoothFactor");
    }

    private void OnDisable()
    {
        walkSpeed = null;
        sprintSpeed = null;
        jumpHeight = null;
        gravityFactor = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(walkSpeed, new GUIContent("行走速度"));
        EditorGUILayout.PropertyField(sprintSpeed, new GUIContent("冲刺速度"));
        EditorGUILayout.PropertyField(jumpHeight, new GUIContent("跳跃高度"));
        EditorGUILayout.PropertyField(gravityFactor, new GUIContent("重力系数"));
        EditorGUILayout.PropertyField(positionSmoothFactor, new GUIContent("位置平滑因子"));
        EditorGUILayout.PropertyField(rotationSmoothFactor, new GUIContent("旋转平滑因子"));

        serializedObject.ApplyModifiedProperties();
    }
}
