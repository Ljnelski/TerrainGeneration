
using UnityEngine;

public class PT_Falloff : BaseProceduralTexture
{
    [SerializeField] private float _steepness = 1;
    [SerializeField] private float _tightness = 1;

    public override string InspectorName => "FallOff";
    public override ProceduralTextureType ProceduralTextureType => ProceduralTextureType.PT_FallOff;

    public override float[,] Generate(float[,] heightMap)
    {
        for (int yy = 0; yy < heightMap.GetLength(1); yy++)
        {
            for (int xx = 0; xx < heightMap.GetLength(0); xx++)
            {
                float percentX = (float)xx / heightMap.GetLength(0);
                float percentY = (float)yy / heightMap.GetLength(1);


                float distanceX = Mathf.Abs(percentX * 2 - 1);
                float distanceY = Mathf.Abs(percentY * 2 - 1);
                //float vX = 4 * Mathf.Pow(percentX - 0.5f, 2) + 1;
                //float vY = 4 * Mathf.Pow(percentY - 0.5f, 2) + 1;

                float vX = Mathf.Pow(distanceX, _steepness) / (Mathf.Pow(distanceX, _steepness) + Mathf.Pow(_tightness - _tightness * distanceX, _steepness));
                float vY = Mathf.Pow(distanceY, _steepness) / (Mathf.Pow(distanceY, _steepness) + Mathf.Pow(_tightness - _tightness * distanceY, _steepness));

                float value = Mathf.Max(vX, vY);

                DrawOnHeightMap(heightMap, xx, yy, value);
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
