using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public static class ProceduralTextureRegistry
{
    private const string UXML_FOLDER_PATH = "PATH";

    private static Dictionary<string, PT_Data> _dictRegistry;
    private static List<PT_Data> _listRegistry;

    private static Dictionary<string, PT_Data> DictRegistry
    {
        get
        {
            if (_dictRegistry == null)
            {
                RegisterDefaultProceduralTextures();
            }

            return _dictRegistry;
        }
    }

    private static List<PT_Data> ListRegistry
    {
        get
        {
            if (_listRegistry == null)
            {
                RegisterDefaultProceduralTextures();
            }

            return _listRegistry;
        }
    }

    private static void RegisterDefaultProceduralTextures()
    {
        _dictRegistry = new Dictionary<string, PT_Data>();
        _listRegistry = new List<PT_Data>();

        RegisterProceduralTexture<PT_Noise>();
        RegisterProceduralTexture<PT_RadialGradient>();
        RegisterProceduralTexture<PT_Falloff>();
    }

    public static VisualTreeAsset GetVisualTreeAsset<T>() where T : BaseProceduralTexture
    {
        return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_FOLDER_PATH + DictRegistry[typeof(T).Name].UXMLAssetName);
    }

    public static void RegisterProceduralTexture<T>() where T: BaseProceduralTexture
    {
        string proceduralTextureClassName = typeof(T).Name;

        PT_Data data = new PT_Data(_listRegistry.Count, proceduralTextureClassName);

        _listRegistry.Add(data);
        _dictRegistry.Add(proceduralTextureClassName, data);
    }

    public static void PrintRegistry()
    {
        foreach (PT_Data data in ListRegistry)
        {
            Debug.Log(data.ToString());
        }
    }
}



public struct PT_Data
{
    private int _id;
    private string _UXMLAssetName;

    public int Id => _id;
    public string UXMLAssetName => _UXMLAssetName;

    public PT_Data(int id, string UXMLAssetName)
    {
        _id = id;
        _UXMLAssetName = UXMLAssetName;
    }

    public override string ToString()
    {
        return "PT_Data: (" + Id + ", " + UXMLAssetName + ")";
    }
}
