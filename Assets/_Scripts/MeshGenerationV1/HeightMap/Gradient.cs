using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Gradient : GeneratedTexture
{ 
    [SerializeField] private float _offsetX;
    [SerializeField] private float _offsetY;
    [SerializeField] private float _radius;
    [SerializeField] private AnimationCurve _gradientCurve;

    public override float[,] Generate(int width, int height)
    {
        float[,] gradient = new float[width, height];

        return Generate(gradient);        
    }

    // Applys a Circular Gradient to a height Map
    public override float[,] Generate(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        Vector2 gradientCentre = new Vector2(halfWidth + _offsetX, halfHeight + _offsetY);

        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                float distanceToCentre = Vector2.Distance(gradientCentre, new Vector2(xx, yy));
                float curveValue = _gradientCurve.Evaluate(1 - distanceToCentre / _radius);

                float value = Mathf.Min(Mathf.Max(curveValue, 0), 1) * 2 - 1f;

                heightMap[xx, yy] =+ value * _drawStrength;
            }
        }

        return heightMap;
    }
}
