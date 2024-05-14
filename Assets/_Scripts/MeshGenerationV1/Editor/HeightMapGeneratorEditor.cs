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

    private VisualTreeAsset _editorVisualTreeAsset;

    private const string UXML_FOLDER_PATH = "Assets/_Scripts/MeshGenerationV1/UXML/";
    private const string UXML_EDITOR_VISUAL_TREE_ASSET_NAME = "HeightMapGeneratorEditorVisualTree.uxml";

    // Bind to the length of _proceduralTexture list

    // Refresh the list when the value changes


    private void OnEnable()
    {
        _heightMapGenerator = (HeightMapGenerator)target;

        _editorVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_EDITOR_VISUAL_TREE_ASSET_NAME);      
    }    

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        _editorVisualTreeAsset.CloneTree(root);

        VisualElement proceduralTextureGroup = root.Q("Section2");
        var proceduralTexturesCount = serializedObject.FindProperty("_proceduralTextures");

        root.TrackPropertyValue(proceduralTexturesCount, (e) => {
            Debug.Log("Count Changed");
            DrawProceduralTextureEditors(proceduralTextureGroup);
        });

        // Hookup add buttons
        var addNoiseButton = root.Q("AddNoiseButton");
        addNoiseButton.RegisterCallback<ClickEvent>((ClickEvent) => {
            _heightMapGenerator.AddProcedurealTexture(ProceduralTextureType.PT_Noise);
        });

        var addRadialGradientButton = root.Q("AddRadialGradientButton");
        addRadialGradientButton.RegisterCallback<ClickEvent>((ClickEvent) => {
            _heightMapGenerator.AddProcedurealTexture(ProceduralTextureType.PT_RadialGradient);
        });

        DrawProceduralTextureEditors(proceduralTextureGroup);
        
        return root;
    }

    public void DrawProceduralTextureEditors(VisualElement root)
    {
        // Clear the exising ones
        root.Clear();

        int length = _heightMapGenerator.ProceduralTextures.Count;
        // Draw and hook up button logic to each Procedural Texture Editor
        for (int i = 0; i < length; i++)
        {
            var PT_EditorVisualElement = CreateAndBindProceduralTextureEditor(i);
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


                // Hide moveup button if first in list
                if (i == 0)
                {
                    moveUpButton.visible = false;
                }
                
                // Hide movedown button if last in list or only in list
                if (i == length - 1 || length == 1)
                {
                    moveDownButton.visible = false;
                }
                root.Add(PT_EditorVisualElement);
            }
        }
    }

    private VisualElement CreateAndBindProceduralTextureEditor(int i)
    {
        VisualElement proceduralTextureEditorElement = CreateEditor(_heightMapGenerator.ProceduralTextures[i]).CreateInspectorGUI();
        SerializedObject serializedProceduralTexture = new SerializedObject(_heightMapGenerator.ProceduralTextures[i]);
        proceduralTextureEditorElement.Bind(serializedProceduralTexture);

        return proceduralTextureEditorElement;
    }

    //private void BuildProceduralTextureEditors()
    //{
    //    _proceduralTextureVisualElements = new VisualElement[_heightMapGenerator.ProceduralTextures.Count];

    //    for (int i = 0; i < _heightMapGenerator.ProceduralTextures.Count; i++)
    //    {
    //        _proceduralTextureVisualElements[i] = CreateEditor(_heightMapGenerator.ProceduralTextures[i]).CreateInspectorGUI();

    //        SerializedObject serializedProceduralTexture = new SerializedObject(_heightMapGenerator.ProceduralTextures[i]);

    //        _proceduralTextureVisualElements[i].Bind(serializedProceduralTexture);
    //    }
    //}

}
