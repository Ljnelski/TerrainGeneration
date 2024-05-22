
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[CustomEditor(typeof(LandscapeGenerator))]
public class LandscapeGeneratorEditor : Editor
{
    #region SerializedProperties
    private SerializedProperty _vertexCountX;
    private SerializedProperty _vertexCountY;
    private SerializedProperty _vertexSpacing;
    private SerializedProperty _maxHeight;
    private SerializedProperty _minHeight;

    private SerializedProperty _autoUpdate;
    #endregion

    bool meshSizeGroup, debugGroup = true;

    public void OnEnable()
    {
        _vertexCountX = serializedObject.FindProperty("_vertexCountX");
        _vertexCountY = serializedObject.FindProperty("_vertexCountY");
        _vertexSpacing = serializedObject.FindProperty("_vertexSpacing");
        _maxHeight = serializedObject.FindProperty("_maxHeight");
        _minHeight = serializedObject.FindProperty("_minHeight");
        _autoUpdate = serializedObject.FindProperty("_autoUpdate");
    }

    public override void OnInspectorGUI()
    {


        EditorGUI.BeginChangeCheck();

        serializedObject.Update();

        LandscapeGenerator meshGenerator = (LandscapeGenerator)target;
        if (meshGenerator == null) return;

        if(DrawDefaultInspector())
        {
            
        }   

        if(GUILayout.Button("RunErosion"))
        {
            //meshGenerator.StartErosion();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();


        if (GUILayout.Button("Generate Mesh") || EditorGUI.EndChangeCheck() && _autoUpdate.boolValue)
        {
            meshGenerator.GenerateTerrain();
        }

        serializedObject.ApplyModifiedProperties();        
    }
}
