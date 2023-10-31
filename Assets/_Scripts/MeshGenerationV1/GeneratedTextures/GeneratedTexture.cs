using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GeneratedTexture : MonoBehaviour
{
    [Header("Mix Values")]
    [SerializeField] protected float _textureAmplitude = 1f;

    abstract public float[,] Generate(float[,] heightMap);
    abstract public float[,] Generate(int width, int height);

}
