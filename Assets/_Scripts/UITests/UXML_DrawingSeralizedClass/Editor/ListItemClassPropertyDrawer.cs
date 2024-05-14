using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ListItemClass))]
public class ListItemClassPropertyDrawer : Editor
{    
    public override VisualElement CreateInspectorGUI()
    {
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UITests/UXML_DrawingSeralizedClass/UXML/ListItemDrawer.UXML");

        VisualElement root = new VisualElement();

        visualTree.CloneTree(root);

        //return base.CreatePropertyGUI(property);
        return root;
    }

}
