using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UIElements;

[CustomEditor(typeof(ClassWithList))]
public class ClassWithListEditor : Editor
{
    VisualTreeAsset _editorVisualTreeAsset;
    VisualTreeAsset _listItemVisualTreeAsset;

    ClassWithList _classWithList;

    VisualElement[] _childVisualTrees;

    private void OnEnable()
    {
        _classWithList = (ClassWithList)target;
        _editorVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UITests/UXML_DrawingSeralizedClass/UXML/ClassWithListEditor.UXML");
        _listItemVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UITests/UXML_DrawingSeralizedClass/UXML/ListItemDrawer.UXML");

        _classWithList.LoadMockData();
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();       
        _editorVisualTreeAsset.CloneTree(root);

        var container = root.Q("container");
        var button = root.Q<Button>("button");
        button.RegisterCallback<MouseUpEvent>((evt) => WriteData());


        foreach (var listItem in _classWithList.list)
        {
            if (listItem != null)
            {
                VisualElement element = _listItemVisualTreeAsset.Instantiate();

                SerializedObject serializedListItem = new SerializedObject(listItem);
                element.Bind(serializedListItem);

                container.Add(element);
            }
        }

        return root;        
    }

    private void WriteData()
    {
        foreach (var item in _classWithList.list)
        {
            item.Logic();
            Debug.Log("\n\n");
        }
    }
}
