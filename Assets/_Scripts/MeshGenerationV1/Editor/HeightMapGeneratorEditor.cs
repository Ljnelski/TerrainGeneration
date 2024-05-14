using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(HeightMapGenerator))]
public class HeightMapGeneratorEditor : Editor
{
    private HeightMapGenerator _heightMapGenerator;

    private VisualElement[] _generatedTextureVisualElements;
    private VisualElement[] _proceduralTextureVisualElements;

    private VisualTreeAsset _editorVisualTreeAsset;

    private const string UXML_FOLDER_PATH = "Assets/_Scripts/MeshGenerationV1/UXML/";
    private const string UXML_EDITOR_VISUAL_TREE_ASSET_NAME = "HeightMapGeneratorEditorVisualTree.uxml";

    private void OnEnable()
    {
        _editorVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_EDITOR_VISUAL_TREE_ASSET_NAME);

        _heightMapGenerator = (HeightMapGenerator)target;
        _generatedTextureVisualElements = new VisualElement[_heightMapGenerator.GeneratedTextures.Count];

        for (int i = 0; i < _generatedTextureVisualElements.Length; i++)
        {
            _generatedTextureVisualElements[i] = Editor.CreateEditor(_heightMapGenerator.GeneratedTextures[i]).CreateInspectorGUI();

            SerializedObject so = new SerializedObject(_heightMapGenerator.GeneratedTextures[i]);

            _generatedTextureVisualElements[i].Bind(so);
        }

        BuildProceduralTextureEditors();
    }

    private void BuildProceduralTextureEditors()
    {
        _heightMapGenerator.CreateTestProceduralTextures();

        _heightMapGenerator = (HeightMapGenerator)target;
        _proceduralTextureVisualElements = new VisualElement[_heightMapGenerator.ProceduralTextures.Count];

        for (int i = 0; i < _heightMapGenerator.ProceduralTextures.Count; i++)
        {
            _proceduralTextureVisualElements[i] = CreateEditor(_heightMapGenerator.ProceduralTextures[i]).CreateInspectorGUI();

            SerializedObject serializedProceduralTexture = new SerializedObject(_heightMapGenerator.ProceduralTextures[i]);

            _proceduralTextureVisualElements[i].Bind(serializedProceduralTexture);
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        _editorVisualTreeAsset.CloneTree(root);

        // Hookup add buttons
        var addNoise = root.Q("AddNoiseButton");
        var addRadialGradient = root.Q("AddRadialGradientButton");
        
        for (int i = 0; i < _proceduralTextureVisualElements.Length; i++)
        {
            var PT_EditorVisualElement = _proceduralTextureVisualElements[i];
            if (PT_EditorVisualElement != null)
            {
                if (i == 0)
                {
                    PT_EditorVisualElement.Q("UpButton").visible = false;
                }
                if (i == _proceduralTextureVisualElements.Length - 1)
                {
                    PT_EditorVisualElement.Q("DownButton").visible = false;
                }

                var contentFoldOut = PT_EditorVisualElement.Q<Foldout>("Foldout");
                contentFoldOut.text = _heightMapGenerator.ProceduralTextures[i].InspectorName;
                contentFoldOut.Q<Label>().AddToClassList("header-label");

                var removeButton = PT_EditorVisualElement.Q<Button>("RemoveButton");
                removeButton.RegisterCallback<ClickEvent>((clickEvent) => {
                    RemoveProceduralTexture(i);
                });

                root.Add(PT_EditorVisualElement);
            }
        }

        return root;
    }

    private void RemoveProceduralTexture(int i)
    {
        _heightMapGenerator.RemoveProceduralTexture(i);
    }    
}
