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

    public Vector3 HeightMapPositionToWorldSpace(int xx, int yy, Vector3 Offset)
    {
        return HeightMapPositionToWorldSpace(xx,yy) + Offset;
    }
    public Vector3 HeightMapPositionToWorldSpace(int xx, int yy)
    {
        float vertexXPosition = xx * _meshVertexSpacing - _worldcenterOffsetX;
        float vertexZPosition = _worldCenterOffsetY - yy * _meshVertexSpacing;
        float vertexYPosition = Mathf.Max(_heightMap[xx, yy] * _meshCeiling, _meshFloor);

        //Debug.Log("RainDropLocation: " + vertexYPosition);

        return new Vector3(vertexXPosition, vertexYPosition, vertexZPosition);
    }
}
