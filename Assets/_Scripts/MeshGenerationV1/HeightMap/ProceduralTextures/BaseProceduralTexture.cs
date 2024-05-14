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
