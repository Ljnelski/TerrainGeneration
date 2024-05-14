using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GeneratedTexture : MonoBehaviour
{
    [SerializeField] protected DrawMode _drawMode = DrawMode.Mix;
    [Range(0f, 2f)] [SerializeField] protected float _drawStrength = 1f;

    public enum DrawMode
    {
        Mix,
        Add,
        Subtract,
    }

    abstract public float[,] Generate(float[,] heightMap);
    abstract public float[,] Generate(int width, int height);

}
