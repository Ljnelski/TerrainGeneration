using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureGenerator : MonoBehaviour
{
    public TerrainColors[] terrains;

    [SerializeField] private string _saveLocation = "./Assets/Textures/SavedTextures";
    [SerializeField] private string _fileName = "GeneratedTexture";
    [SerializeField] private Texture2D _texture;
    [SerializeField] private TextureType _type;

    private enum TextureType
    {
        ColourMap,
        HeightMap
    }

    public void SaveTexture(float[,] heightMapData)
    {
        byte[] bytes;

        switch (_type)
        {
            case TextureType.ColourMap:
                bytes = ColourFromHeightMap(heightMapData).EncodeToPNG();
                break;
            case TextureType.HeightMap:
                bytes = ValueFromHeightMap(heightMapData).EncodeToPNG();
                break;
            default:
                bytes = new byte[heightMapData.GetLength(0) * heightMapData.GetLength(1)];
                break;
        }
        if (!Directory.Exists(_saveLocation))
        {
            Directory.CreateDirectory(_saveLocation);
        }

        File.WriteAllBytes(_saveLocation + "/" + _fileName + ".png", bytes);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public Texture2D ValueFromHeightMap(float[,] heightMapData)
    {
        int width = heightMapData.GetLength(0);
        int height = heightMapData.GetLength(1);

        Texture2D heightMapTexture = new Texture2D(width, height);

        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                heightMapTexture.SetPixel(xx, yy, Color.Lerp(Color.black, Color.white, (heightMapData[xx, yy] + 1f) / 2));
            }
        }

        heightMapTexture.Apply();

        return heightMapTexture;
    }

    public Texture2D ColourFromHeightMap(float[,] heightMapData)
    {
        int width = heightMapData.GetLength(0);
        int height = heightMapData.GetLength(1);

        Texture2D colourMapTexture = new Texture2D(width, height);


        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                float value = (heightMapData[xx, yy] + 1f) / 2;
                //Debug.Log(value);

                for (int i = 0; i < terrains.Length; i++)
                {
                    if (value <= terrains[i].height)
                    {
                        colourMapTexture.SetPixel(xx, yy, terrains[i].colour);
                        break;
                    }
                }
            }
        }

        colourMapTexture.Apply();

        return colourMapTexture;
    }
}
