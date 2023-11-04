using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    [SerializeField] private GameObject _rainMarker;

    [SerializeField] private int _seed;
    [SerializeField] private int _maxAliveDroplets = 10;

    private List<WaterDrop> _droplets;
    private int _remaingPasses;

    private System.Random rand;



    // Start is called before the first frame update
    void Start()
    {
        _droplets = new List<WaterDrop>();
        rand = new System.Random(_seed);
    }

    public void ErosionPass(LandscapeData landscapeData)
    {
        if (rand == null)
        {
            rand = new System.Random();
        }

        if (_droplets == null)
        {
            _droplets = new List<WaterDrop>();
        }

        float halfVertexSpacing = landscapeData.VertexSpacing / 2;

        // Spawn Droplets
        for (int i = 0; i < _maxAliveDroplets - _droplets.Count; i++)
        {
            Debug.Log("Spawning Droplets");

            int xVertexIndex = rand.Next(0, landscapeData.MeshXVertexCount);
            int yVertexIndex = rand.Next(0, landscapeData.MeshYVertexCount);

            Vector2Int dropletHeightMapPos = new Vector2Int(xVertexIndex, yVertexIndex);
            // Droplet Conversion to world space for visual Aid
            // Height map Y position is converted to Z position world space

            WaterDrop waterDrop = new WaterDrop(dropletHeightMapPos, Vector3.zero, new Vector2Int(0, 0), 0);
            Vector3 dropWorldPos = CalculateDropWorldSpace(landscapeData, waterDrop);
            waterDrop.WPos = dropWorldPos;

            waterDrop.trail.Add(waterDrop.WPos);

            _droplets.Add(waterDrop);
            Instantiate(_rainMarker, waterDrop.WPos, Quaternion.identity);
        }

        // Do Pass
        for (int i = 0; i < _droplets.Count; i++)
        {

            Debug.Log("Erosion Pass Happening for Droplet: " + i);

            WaterDrop waterDrop = _droplets[i];

            float pxy, pxy1, px1y, px1y1; // Four vertexs around where droplet is located pxy = current location.
            float gx, gy; // Gradient Vector
            Vector2 gxy;
            Vector2 uv;
            float v = 0, u = 0; // Direction of water flow (v is x component, u is y component)           

            pxy = landscapeData.HeightMap[waterDrop.Pos.x, waterDrop.Pos.y];
            px1y = landscapeData.HeightMap[waterDrop.Pos.x + 1, waterDrop.Pos.y];
            pxy1 = landscapeData.HeightMap[waterDrop.Pos.x, waterDrop.Pos.y + 1];
            px1y1 = landscapeData.HeightMap[waterDrop.Pos.x + 1, waterDrop.Pos.y + 1];

            v = waterDrop.Dir.x;
            u = waterDrop.Dir.y;

            // Gradient which changes the way the water flows
            gy = (px1y - pxy) * (1 - v) + (px1y1 - pxy1) * v;
            gx = (pxy1 - pxy) * (1 - u) + (px1y1 - px1y) * u;

            // Normalize Gradient
            Debug.Log("Gradient Vector: " + gx + ", " + gy);
            gxy = new Vector2(gx, gy).normalized;

            // Set Droplet Velocity
            waterDrop.Dir.Set(Mathf.RoundToInt(gxy.x), Mathf.RoundToInt(gxy.y));         

            // Move Droplet
            waterDrop.Pos += waterDrop.Dir;

            // Calcuate Droplet Position in worldspace
            waterDrop.WPos = CalculateDropWorldSpace(landscapeData, waterDrop);
            waterDrop.trail.Add(waterDrop.WPos);

            // Assign the updated drop back to the list;
            _droplets[i] = waterDrop;           
        }
    }

    public void ClearRainDrops()
    {
        _droplets.Clear();
    }

    private Vector3 CalculateDropWorldSpace(LandscapeData landscapeData, WaterDrop drop) 
    {
        int xPos = drop.Pos.x;
        int yPos = drop.Pos.y;

        Vector3 pos1 = landscapeData.HeightMapPositionToWorldSpace(xPos, yPos + 1);//landscapeData.HeightMap[xPos, yPos + 1];
        Vector3 pos2 = landscapeData.HeightMapPositionToWorldSpace(xPos + 1, yPos); //landscapeData.HeightMap[xPos + 1, yPos];

        //float centreHeight = Mathf.Lerp(height1, height2, 0.5f);
        Vector3 centrePos = Vector3.Lerp(pos1, pos2, 0.5f);

        //float rainDropHeight = landscapeData.HeightMapValueToWorldSpace(centreHeight);

        //Vector3 Offset = new Vector3(0.5f, 0f, 0.5f);
        //Vector3 WPos = landscapeData.HeightMapPositionToWorldSpace(xPos, yPos, Offset);
        //WPos.y = rainDropHeight;

        return centrePos;
    }

    private void OnDrawGizmos()
    {
        if (_droplets != null)
        {
            for (int i = 0; i < _droplets.Count; i++)
            {
                WaterDrop drop = _droplets[i];

                for (int k = 0; k < drop.trail.Count; k++)
                {
                    if (k == drop.trail.Count - 1)
                    {
                        Gizmos.color = Color.white;
                    }
                    else
                    {
                        float placeInPath = (float)k / drop.trail.Count;
                        Color wayPointColor = Color.Lerp(Color.black, Color.white, placeInPath);
                        Gizmos.color = wayPointColor;
                    }
                    Gizmos.DrawSphere(drop.trail[k], 0.5f);

                    // Draw Line between spheres
                    if (k > 0 && drop.trail.Count > 1)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(drop.trail[k - 1],
                            drop.trail[k]);
                    }
                }
            }
            
        }
    }
}

struct WaterDrop
{
    public WaterDrop(Vector2Int pos, Vector3 wPos, Vector2Int dir, float vel)
    {
        // position on the heightmap
        Pos = pos;
        WPos = wPos;
        Dir = dir;
        Vel = vel;
        Water = 0f;
        Sediment = 0f;

        trail = new List<Vector3>();
    }

    public Vector2Int Pos;
    public Vector2Int Dir;
    public Vector3 WPos;
    public float Vel;
    public float Water;
    public float Sediment;

    public List<Vector3> trail;
}
