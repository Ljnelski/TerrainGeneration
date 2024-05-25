using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(BaseProceduralTexture))]
public class BaseProceduralTextureEditor : Editor
{
    protected VisualTreeAsset _baseVisualTreeAsset;
    [SerializeField] protected VisualTreeAsset _editorVisalTreeAsset;

    protected const string UXML_FOLDER_PATH = "Assets/ProceduralGeneration/UXML/";
    protected const string UXML_ERROR_ASSET_PATH = "ErrorVisualTree.uxml";
    protected const string UXML_VISUAL_BASE_TREE_ASSET_NAME = "GeneratedTextureEditorVisualTree.uxml";

    private void LoadBaseVisualTree()
    {
        _baseVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + UXML_VISUAL_BASE_TREE_ASSET_NAME);
    }

    public override VisualElement CreateInspectorGUI()
    {
        if (_baseVisualTreeAsset == null)
        {
            LoadBaseVisualTree();
        }

        VisualElement root = new VisualElement();

        var editorVisualTree = _baseVisualTreeAsset.CloneTree();

        VisualElement content = editorVisualTree.Q("Content");
        VisualElement foldout = editorVisualTree.Q<Foldout>("Foldout");
        VisualElement buttonGroup = editorVisualTree.Q("ButtonGroup");

        foldout.contentContainer.Add(content);

        root.Add(foldout);
        root.Add(buttonGroup);

        // If There is no ProceduralTextureAsset then Load the Default Inspector instead
        if (_editorVisalTreeAsset == null)
        {
            content.Clear();

            InspectorElement.FillDefaultInspector(content, serializedObject, this);
        }
        else
        {
            VisualElement proceduralTextureVarsContainer = root.Q("ProceduralTextureVars");

            _editorVisalTreeAsset.CloneTree(proceduralTextureVarsContainer);
        }

        return root;
    }
}
