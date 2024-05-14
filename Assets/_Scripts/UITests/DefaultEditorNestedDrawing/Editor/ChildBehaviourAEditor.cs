using UnityEditor;

[CustomEditor(typeof(ChildBehaviourA))]
public class ChildBehaviourAEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
