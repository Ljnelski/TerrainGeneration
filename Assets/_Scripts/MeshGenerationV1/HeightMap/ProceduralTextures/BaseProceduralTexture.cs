using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseProceduralTexture : ScriptableObject
{
    [SerializeField] protected DrawMode _drawMode = DrawMode.Mix;
    [Range(0f, 2f)][SerializeField] protected float _drawStrength;

    public int IndexInHeightMapGenerator
    {        
        get
        {
            return _index;
        }
    }

    private HeightMapGenerator _parent;
    private int _index;

    public abstract string InspectorName
    {
        get;
    }

    public abstract ProceduralTextureType ProceduralTextureType
    {
        get;
    }


    abstract public float[,] Generate(float[,] heightMap);
    abstract public float[,] Generate(int width, int height);

    public void Assign(HeightMapGenerator parent, int index)
    {
        _parent = parent;
        _index = index;
    }

    protected void DrawOnHeightMap(float[,] heightMap, int x, int y, float value)
    {
        switch (_drawMode)
        {
            case DrawMode.Mix:
                heightMap[x,y] = Mathf.Lerp(heightMap[x,y], value, _drawStrength);
                break;
            case DrawMode.Add:
                heightMap[x,y] = heightMap[x,y] + value * _drawStrength;
                break;
            case DrawMode.Subtract:
                heightMap[x,y] = heightMap[x,y] - value * _drawStrength;
                break;
            case DrawMode.Multiply:
                heightMap[x, y] = heightMap[x, y] * (value * _drawStrength);
                break;
            case DrawMode.Draw:
                heightMap[x, y] = value * _drawStrength;
                break;
            default:
                break;
        }
    }

    public void RemoveFromHeightMapGeneration()
    {
        _parent.RemoveProceduralTexture(_index);
    }

    public void MoveUpInOrder()
    {
        _parent.MoveProceduralTextureUp(_index);
    }

    public void MoveDownInOrder()
    {
        _parent.MoveProceduralTextureDown(_index);
    }
}
