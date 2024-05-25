using System;
using UnityEngine;

public class LandscapeData
{
    public void SetWorldParameters(int worldSize, int chunkSizeX, int chunkSizeY, int meshLOD, float vertexSpacing, float meshCeiling, float meshFloor)
    {
        _worldSize = worldSize;
        _chunkSizeX = chunkSizeX;
        _chunkSizeY = chunkSizeY;
        _meshLOD = meshLOD;
        _meshVertexSpacing = vertexSpacing;
        _meshCeiling = meshCeiling;
        _meshFloor = meshFloor;

        // Calculate the offset that is used to place the vertices in the world to make the mesh is centred in the centre of the world
        _bottomLeftX = worldSize * (chunkSizeX - 1) / -2f * vertexSpacing;
        _bottomLeftZ = worldSize * (chunkSizeY - 1) / -2f * vertexSpacing;

        //Debug.Log("_BottomLeftX: " + _bottomLeftX);
        //Debug.Log("_BottomLeftZ: " + _bottomLeftZ);

        _chunks = new Chunk[worldSize, worldSize];

        _hasMeshVertexParameters = true;
    }

    public void SetHeightMapData(float[,] heightMap)
    {
        _heightMap = heightMap;
        _hasHeightMapData = true;
    }

    private int _worldSize;
    private int _chunkSizeX;
    private int _chunkSizeY;
    private Chunk[,] _chunks;
    private float _meshVertexSpacing;
    private float _bottomLeftX; // world X pos of top left vertex, heightmap index [0,0] is this vertex
    private float _bottomLeftZ; // world Z pos of top left vertex
    private float _meshCeiling;
    private float _meshFloor;
    private int _meshLOD;
    private float[,] _heightMap;

    private bool _hasMeshVertexParameters;
    private bool _hasHeightMapData;
    private bool _hasMeshData;

    public int WorldSize { get => _worldSize; }
    public int ChunkSizeX { get => _chunkSizeX; }
    public int ChunkSizeY { get => _chunkSizeY; }
    public int MeshLOD { get => _meshLOD; set => _chunkSizeY = value; }
    public float VertexSpacing { get => _meshVertexSpacing; set => _meshVertexSpacing = value; }
    public float BottomLeftX { get => _bottomLeftX; set => _bottomLeftX = value; }
    public float BottomLeftZ { get => _bottomLeftZ; set => _bottomLeftZ = value; }
    public float MeshCeiling { get => _meshCeiling; set => _meshCeiling = value; }
    public float MeshFloor { get => _meshFloor; set => _meshFloor = value; }

    public Chunk[,] ChunkData { get => _chunks; }
    public float[,] HeightMap { get => _heightMap; set => _heightMap = value; }

    public bool HasMeshVertexParameters { get => _hasMeshVertexParameters; }
    public bool HasHeightMapData { get => _hasHeightMapData; }
    public bool HasMeshData { get => _hasMeshData; }

    // Inputs a heightMap Position and returns a position in world space
    public Vector3 HeightMapPositionToWorldSpace(float xx, float yy)
    {
        int coordX = (int)xx;
        int coordY = (int)yy;

        float vertexXPosition = xx * VertexSpacing;
        float vertexZPosition = yy * VertexSpacing;

        try
        {
            float vertexYPosition = HeightMapValueToWorldSpace(_heightMap[coordX, coordY]);
            return new Vector3(vertexXPosition, vertexYPosition, vertexZPosition);
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.LogError("LandscapeData ERROR: Failed to translate a heightMap position to world space with Error: " + e);
            Debug.Log("xx: " + xx + "yy: " + yy);
        }

        return Vector3.zero;
    }

    // Inputs a heightMap Value and returns the height value in world space
    public float HeightMapValueToWorldSpace(float value)
    {
        return Mathf.Max(value * _meshCeiling, _meshFloor);
    }

    public void SetChuck(int x, int y, Chunk chunk)
    {
        _chunks[x, y] = chunk;
    }
}
