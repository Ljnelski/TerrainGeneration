using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct LandscapeData
{
    public void SetMeshVertexParameters(int vertexCountX, int vertexCountY, float vertexSpacing, float meshCeiling, float meshFloor)
    {
        _meshXVertexCount = vertexCountX;
        _meshYVertexCount = vertexCountY;
        _meshVertexSpacing = vertexSpacing;
        _meshCeiling = meshCeiling;
        _meshFloor = meshFloor;

        // Calculate the offset that is used to place the vertices in the world to make the mesh is centred in the centre of the world
        _worldcenterOffsetX = (_meshXVertexCount - 1) * _meshVertexSpacing / 2;
        _worldCenterOffsetY = (_meshYVertexCount - 1) * _meshVertexSpacing / 2;

        _hasMeshVertexParameters = true;
    }

    public void SetHeightMapData(float[,] heightMap)
    {
        _heightMap = heightMap;
        _hasHeightMapData = true;
    }

    public void SetMesh(Mesh mesh)
    {
        _mesh = mesh;
        _hasMeshData = true;
    }

    private int _meshXVertexCount;
    private int _meshYVertexCount;
    private float _meshVertexSpacing;
    private float _worldcenterOffsetX;
    private float _worldCenterOffsetY;
    private float _meshCeiling;
    private float _meshFloor;

    private Mesh _mesh;
    private float[,] _heightMap;

    private bool _hasMeshVertexParameters;
    private bool _hasHeightMapData;
    private bool _hasMeshData;

    public int MeshXVertexCount { get => _meshXVertexCount; set => _meshXVertexCount = value; }
    public int MeshYVertexCount { get => _meshYVertexCount; set => _meshYVertexCount = value; }
    public float VertexSpacing { get => _meshVertexSpacing; set => _meshVertexSpacing = value; }
    public float WorldcenterOffsetX { get => _worldcenterOffsetX; set => _worldcenterOffsetX = value; }
    public float WorldCenterOffsetY { get => _worldCenterOffsetY; set => _worldCenterOffsetY = value; }
    public float MeshCeiling { get => _meshCeiling; set => _meshCeiling = value; }
    public float MeshFloor { get => _meshFloor; set => _meshFloor = value; }

    public Mesh Mesh { get => _mesh; set => _mesh = value; }
    public float[,] HeightMap { get => _heightMap; set => _heightMap = value; }
    
    public bool HasMeshVertexParameters { get => _hasMeshVertexParameters; }
    public bool HasHeightMapData { get => _hasHeightMapData;  }
    public bool HasMeshData { get => _hasMeshData; }
   
    public Vector3 HeightMapPositionToWorldSpace(float xx, float yy)
    {
        int coordX = (int)xx;
        int coordY = (int)yy;

        float vertexXPosition = xx * _meshVertexSpacing - _worldcenterOffsetX;
        float vertexZPosition = _worldCenterOffsetY - yy * _meshVertexSpacing;

        try
        {
            float vertexYPosition = HeightMapValueToWorldSpace(_heightMap[coordX, coordY]);
            return new Vector3(vertexXPosition, vertexYPosition, vertexZPosition);
        }
        catch (IndexOutOfRangeException e)
        {
            //Debug.LogError("LandscapeData ERROR: Failed to translate a heightMap position to world space with Error: " + e);
            //Debug.Log("xx: " + xx + "yy: " + yy);
        }
        
        return Vector3.zero;
    }   

    // Inputs a heightMap Value and returns the height value in World Space
    public float HeightMapValueToWorldSpace(float value)
    {
        return Mathf.Max(value * _meshCeiling, _meshFloor);
    }
}
