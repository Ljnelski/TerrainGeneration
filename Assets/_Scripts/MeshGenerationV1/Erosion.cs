using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    [SerializeField] private GameObject _rainMarker;

    [SerializeField] private int _seed;
    [SerializeField] private int _maxAliveDroplets = 10;

    private List<WaterDrop> _droplets;
    private List<Vector3> _dropTrail;
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

        if (_dropTrail == null)
        {
            _dropTrail = new List<Vector3>();
        }


        // Spawn Droplets
        for (int i = 0; i < _maxAliveDroplets - _droplets.Count; i++)
        {
            Debug.Log("Spawning Droplets");

            int xVertexIndex = rand.Next(0, landscapeData.MeshXVertexCount);
            int yVertexIndex = rand.Next(0, landscapeData.MeshYVertexCount);

            Vector2Int dropletHeightMapPos = new Vector2Int(xVertexIndex, yVertexIndex);
            // Droplet Conversion to world space for visual Aid
            // Height map Y position is converted to Z position world space
            Vector3 dropWorldSpaceOffset = new Vector3(landscapeData.VertexSpacing / 2, 0, landscapeData.VertexSpacing / 2);
            Vector3 dropletWorldPos = landscapeData.HeightMapPositionToWorldSpace(xVertexIndex, yVertexIndex, dropWorldSpaceOffset);

            WaterDrop waterDrop = new WaterDrop(dropletHeightMapPos, dropletWorldPos, new Vector2Int(0, 0), 0);

            _droplets.Add(waterDrop);

            _dropTrail.Add(dropletWorldPos);
            Instantiate(_rainMarker, waterDrop.WPos, Quaternion.identity);
        }

        // Do Pass
        for (int i = 0; i < _droplets.Count; i++)
        {
            //Debug.Log("Erosion Pass Happening for Droplet: " + i);

            WaterDrop drop = _droplets[i];

            float pxy, pxy1, px1y, px1y1; // Four vertexs around where droplet is located pxy = current location.
            float gx, gy; // Gradient Vector
            Vector2 gxy;
            Vector2 uv;
            float v = 0, u = 0; // Direction of water flow (v is x component, u is y component)           

            pxy = landscapeData.HeightMap[drop.Pos.x, drop.Pos.y];
            px1y = landscapeData.HeightMap[drop.Pos.x, drop.Pos.y];
            pxy1 = landscapeData.HeightMap[drop.Pos.x, drop.Pos.y + 1];
            px1y1 = landscapeData.HeightMap[drop.Pos.x + 1, drop.Pos.y + 1];

            v = drop.Dir.x;
            u = drop.Dir.y;

            // Gradient which changes the way the water flows
            gx = (px1y - pxy) * (1 - v) + (px1y1 - pxy1) * v;
            gy = (pxy1 - pxy) * (1 - u) + (px1y1 - px1y) * u;

            // Normalize Gradient
            //Debug.Log("Gradient Vector: " + gx + ", " + gy);
            gxy = new Vector2(gx, gy).normalized;

            // Set Droplet Velocity
            drop.Dir.Set(Mathf.RoundToInt(gxy.x), Mathf.RoundToInt(gxy.y));
            //Debug.Log("Drop Direction: " + drop.Dir);
            // Move Droplet

            Debug.Log("Drop Position: " + drop.Pos);

            drop.Pos += drop.Dir;

            //Debug.Log("New Drop Position: " + drop.Pos);

            _droplets[i] = drop;

            Debug.Log("\n\n\n\n\n");
            _dropTrail.Add(
                landscapeData.HeightMapPositionToWorldSpace(
                    drop.Pos.x,
                    drop.Pos.y,
                    new Vector3(landscapeData.VertexSpacing / 2, 0, landscapeData.VertexSpacing / 2)));
        }
    }

    public void ClearRainDrops()
    {
        _droplets.Clear();
        _dropTrail.Clear();
    }

    private void OnDrawGizmos()
    {
        if (_droplets != null)
        {
            for (int i = 0; i < _dropTrail.Count; i++)
            {
                if (i == _dropTrail.Count - 1)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(_dropTrail[i], 0.5f);
                
                if(i > 0 &&_dropTrail.Count > 1)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(_dropTrail[i - 1],
                        _dropTrail[i]);
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
    }

    public Vector2Int Pos;
    public Vector2Int Dir;
    public Vector3 WPos;
    public float Vel;
    public float Water;
    public float Sediment;
}
