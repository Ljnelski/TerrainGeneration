using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParentBehaviour))]
public class ParentBehaviourEditor : Editor
{
    private ParentBehaviour parentBehaviour;
    private Editor[] childEditors;

    private void OnEnable()
    {
        parentBehaviour = (ParentBehaviour)target;
        childEditors = new Editor[parentBehaviour.childBehaviours.Count];
        for (int i = 0; i < childEditors.Length; i++)
        {
            childEditors[i] = Editor.CreateEditor(parentBehaviour.childBehaviours[i]);
        }
    }

    public override void OnInspectorGUI()
    {
        parentBehaviour.getChildren();

        //base.OnInspectorGUI();

        foreach (var childEditor in childEditors)
        {
            if (childEditor != null)
            {
                childEditor.OnInspectorGUI();
            }
        }

        childEditors = new Editor[parentBehaviour.childBehaviours.Count];
        for(int i = 0;i < childEditors.Length; i++)
        {
            var x = parentBehaviour.childBehaviours[i];

            childEditors[i] = CreateEditor(parentBehaviour.childBehaviours[i]);
        }
    }
}
