using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Erosion))]
public class ErosionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Erosion erosion = (Erosion)target;
        if (erosion == null) return;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Clear Drops"))
        {
            erosion.ClearRainDrops();
        }
    }
}
