using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseProceduralTexture : ScriptableObject
{
    [SerializeField] protected DrawMode _drawMode = DrawMode.Mix;
    [Range(0f, 2f)][SerializeField] protected float _drawStrength;

    public int Index
    {
        get
        {
            return _index;
        }
        set
        {
            _index = value;
        }
    }

    public int _index;

    public abstract string InspectorName
    {
        get;
    }

    abstract public float[,] Generate(float[,] heightMap);
    abstract public float[,] Generate(int width, int height);
}
