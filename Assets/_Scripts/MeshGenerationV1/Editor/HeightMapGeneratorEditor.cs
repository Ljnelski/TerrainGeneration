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
        _heightMapGenerator = (HeightMapGenerator)target;

        _editorVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_EDITOR_VISUAL_TREE_ASSET_NAME);
        //_generatedTextureVisualElements = new VisualElement[_heightMapGenerator.GeneratedTextures.Count];

        //for (int i = 0; i < _generatedTextureVisualElements.Length; i++)
        //{
        //    _generatedTextureVisualElements[i] = Editor.CreateEditor(_heightMapGenerator.GeneratedTextures[i]).CreateInspectorGUI();

        //    SerializedObject so = new SerializedObject(_heightMapGenerator.GeneratedTextures[i]);

        //    _generatedTextureVisualElements[i].Bind(so);
        //}        
    }

    private void BuildProceduralTextureEditors()
    {
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
        BuildProceduralTextureEditors();

        VisualElement root = new VisualElement();

        _editorVisualTreeAsset.CloneTree(root);

        // Hookup add buttons
        var addNoiseButton = root.Q("AddNoiseButton");
        addNoiseButton.RegisterCallback<ClickEvent>((ClickEvent) => {
            _heightMapGenerator.AddProcedurealTexture(ProceduralTextureType.PT_Noise);
        });

        var addRadialGradientButton = root.Q("AddRadialGradientButton");
        addRadialGradientButton.RegisterCallback<ClickEvent>((ClickEvent) => {
            _heightMapGenerator.AddProcedurealTexture(ProceduralTextureType.PT_RadialGradient);
        });

        for (int i = 0; i < _proceduralTextureVisualElements.Length; i++)
        {
            var PT_EditorVisualElement = _proceduralTextureVisualElements[i];
            var PT_Script = _heightMapGenerator.ProceduralTextures[i];

            PT_Script.Assign(_heightMapGenerator, i);

            if (PT_EditorVisualElement != null)
            {
                var contentFoldOut = PT_EditorVisualElement.Q<Foldout>("Foldout");
                contentFoldOut.text = _heightMapGenerator.ProceduralTextures[i].InspectorName;
                contentFoldOut.Q<Label>().AddToClassList("header-label");

                var removeButton = PT_EditorVisualElement.Q<Button>("RemoveButton");
                removeButton.RegisterCallback<ClickEvent>((clickEvent) => {
                    PT_Script.RemoveFromHeightMapGeneration();
                });

                var moveUpButton = PT_EditorVisualElement.Q<Button>("MoveUpButton");
                moveUpButton.RegisterCallback<ClickEvent>((clickEvent) => {
                    PT_Script.MoveUpInOrder();
                });

                var moveDownButton = PT_EditorVisualElement.Q<Button>("MoveDownButton");
                moveDownButton.RegisterCallback<ClickEvent>((ClickEvent) => {
                    PT_Script.MoveDownInOrder();
                });

                if (i == 0)
                {
                    moveUpButton.visible = false;
                }
                if (i == _proceduralTextureVisualElements.Length - 1)
                {
                    moveDownButton.visible = false;
                }

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
