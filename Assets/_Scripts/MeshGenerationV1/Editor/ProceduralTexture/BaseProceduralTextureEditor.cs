using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(BaseProceduralTexture))]
public class BaseProceduralTextureEditor : Editor
{
    protected VisualTreeAsset _baseVisualTreeAsset;
    [SerializeField] protected VisualTreeAsset _editorVisalTreeAsset;

    protected const string UXML_FOLDER_PATH = "Assets/_Scripts/MeshGenerationV1/UXML/";
    protected const string UXML_ERROR_ASSET_PATH = "ErrorVisualTree.uxml";
    protected const string UXML_VISUAL_BASE_TREE_ASSET_NAME = "GeneratedTextureEditorVisualTree.uxml";    

    private void LoadBaseVisualTree()
    {
        _baseVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_VISUAL_BASE_TREE_ASSET_NAME);
    }

    private void LoadVisualTree()
    {
        if (_editorVisalTreeAsset == null)
        {
            _editorVisalTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_ERROR_ASSET_PATH);
        }
    }


    public override VisualElement CreateInspectorGUI()
    {
        if (_baseVisualTreeAsset == null)
        {
            LoadBaseVisualTree();
        }

        if (_editorVisalTreeAsset == null)
        {
            LoadVisualTree();
        }

        VisualElement root = new VisualElement();

        var editorVisualTree = _baseVisualTreeAsset.CloneTree();


        // Attach the Content Element to the Fold out
        VisualElement foldout = editorVisualTree.Q<Foldout>("Foldout");
        VisualElement content = editorVisualTree.Q("Content");

        foldout.contentContainer.Add(content);

        root.Add(foldout);

        VisualElement childClassElement = root.Q("derivedClass");

        if (childClassElement == null)
        {
            Debug.LogError("GeneratedTextureEditorError: failed to find TextureVisual Tree subsitution");
        }

        if (_editorVisalTreeAsset != null && childClassElement != null)
        {
            _editorVisalTreeAsset.CloneTree(childClassElement);
        }

        return root;
    }
}
