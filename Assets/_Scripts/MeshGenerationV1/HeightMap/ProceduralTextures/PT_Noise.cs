using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PT_Noise : BaseProceduralTexture
{
    [SerializeField] private float _offsetX;
    [SerializeField] private float _offsetY;

    [Min(0.001f)]
    [SerializeField] private float _scale = 10f;
    [Min(1)]
    [SerializeField] private int _octaves = 4;

    [Min(1)]
    [SerializeField] private float _lacunarity = 2f;
    [Range(0, 1)]
    [SerializeField] private float _persistance = 0.5f;

    [Range(0, 10000)]
    [SerializeField] private int _seed;

    private Vector2[] octaveOffsets;

    public override string InspectorName => "Noise";

    public override float[,] Generate(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        octaveOffsets = new Vector2[_octaves];

        // Get Random Coordinates and to add more randomness to octaves to avoid Symmetry
        System.Random rand = new System.Random(_seed);
        for (int octaves = 0; octaves < _octaves; octaves++)
        {
            octaveOffsets[octaves] = new Vector3(rand.Next(-10000, 10000) + _offsetX, rand.Next(-10000, 10000) + _offsetY);
        }

        Vector3 offset = new Vector3(rand.Next(-10000, 10000) + _offsetX, rand.Next(-10000, 10000) + _offsetY);

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                float heightValue = 0f;

                float frequency = 1f;
                float amplitude = 1f;

                for (int octave = 0; octave < _octaves; octave++)
                {
                    float sampleX = (xx - halfWidth) / _scale * frequency + offset.x;
                    float sampleY = (yy - halfHeight) / _scale * frequency + offset.y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY); // * 2 - 1f;
                    heightValue += perlinValue * amplitude;

                    frequency *= _lacunarity;
                    amplitude *= _persistance;
                }

                heightMap[xx, yy] += heightValue * _drawStrength;
            }
        }

        return heightMap;
    }

    public override float[,] Generate(int width, int height)
    {
        float[,] heightMap = new float[width, height];

        return Generate(heightMap);
    }
}
