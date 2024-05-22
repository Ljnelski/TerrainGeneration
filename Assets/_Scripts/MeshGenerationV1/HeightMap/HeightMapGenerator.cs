using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HeightMapGenerator : MonoBehaviour
{
    [SerializeField] private List<BaseProceduralTexture> _proceduralTextures;
    public List<BaseProceduralTexture> ProceduralTextures
    {
        get
        {
            if (_proceduralTextures == null)
            {
                _proceduralTextures = new List<BaseProceduralTexture>();
            }

            return _proceduralTextures;
        }
    }

    private void CreateProceduralTexture(ProceduralTextureType type)
    {
        switch (type)
        {
            case ProceduralTextureType.PT_Noise:
                _proceduralTextures.Add((BaseProceduralTexture)PT_Noise.CreateInstance<PT_Noise>());
                break;
            case ProceduralTextureType.PT_RadialGradient:
                _proceduralTextures.Add((PT_RadialGradient)PT_Noise.CreateInstance<PT_RadialGradient>());
                break;
            default:
                break;
        }
    }

    public void CreateTestProceduralTextures()
    {
        _proceduralTextures = new List<BaseProceduralTexture>();

        CreateProceduralTexture(ProceduralTextureType.PT_Noise);
        CreateProceduralTexture(ProceduralTextureType.PT_RadialGradient);
    }

    public float[,] GenerateHeightMap(int width, int height)
    {
        float[,] heightMap = new float[width, height];

        if (_proceduralTextures == null)
        {
            return heightMap;
        }

        if (_proceduralTextures.Count == 0)
        {
            Debug.LogWarning("HeightMapGenerator WARNING: No Generated Textures found on GameObject; Heightmap will be all 0");
            return heightMap;
        }

        foreach (var proceduralTexture in _proceduralTextures)
        {
            heightMap = proceduralTexture.Generate(heightMap);
        }

        return heightMap;
    }

    public void AddProcedurealTexture(ProceduralTextureType PT_Type)
    {
        switch (PT_Type)
        {
            case ProceduralTextureType.PT_Noise:
                _proceduralTextures.Add((PT_Noise)PT_Noise.CreateInstance<PT_Noise>());
                break;
            case ProceduralTextureType.PT_RadialGradient:
                _proceduralTextures.Add(PT_RadialGradient.CreateInstance<PT_RadialGradient>());
                break;
            default:
                break;
        }
    }

    public void RemoveProceduralTexture(int index)
    {
        _proceduralTextures.RemoveAt(index);
    }

    public void MoveProceduralTextureUp(int targetIndex)
    {
        SwapProceduralTexture(targetIndex, targetIndex - 1);
    }

    public void MoveProceduralTextureDown(int targetIndex)
    {
        SwapProceduralTexture(targetIndex, targetIndex + 1);
    }

    private void SwapProceduralTexture(int indexFrom, int indexTo)
    {
        BaseProceduralTexture PT_moved = _proceduralTextures[indexFrom];
        _proceduralTextures.RemoveAt(indexFrom);
        _proceduralTextures.Insert(indexTo, PT_moved);
    }
}
