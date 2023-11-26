using System;
using UnityEngine;

public class LandscapeData
{
    public void SetWorldParameters(int worldSize, int chunkSizeX, int chunkSizeY,  int meshLOD, float vertexSpacing, float meshCeiling, float meshFloor)
    {
        _worldSize = worldSize;
        _chunkSizeX = chunkSizeX;
        _chunkSizeY = chunkSizeY;
        _meshLOD = meshLOD;
        _meshVertexSpacing = vertexSpacing;
        _meshCeiling = meshCeiling;
        _meshFloor = meshFloor;

        // Calculate the offset that is used to place the vertices in the world to make the mesh is centred in the centre of the world
        _topLeftX = (_chunkSizeX - 1) / -2f;
        _topLeftZ = (_chunkSizeY - 1) / -2f;

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
    private float _topLeftX; // world X pos of top left vertex, heightmap index [0,0] is this vertex
    private float _topLeftZ; // world Z pos of top left vertex
    private float _meshCeiling;
    private float _meshFloor;
    private int _meshLOD;
    private float[,] _heightMap;

    private bool _hasMeshVertexParameters;
    private bool _hasHeightMapData;
    private bool _hasMeshData;

    public int WorldSize { get => _worldSize; }
    public int MeshXVertexCount { get => _chunkSizeX; }
    public int MeshYVertexCount { get => _chunkSizeY; }
    public int MeshLOD { get => _meshLOD; set => _chunkSizeY = value; }
    public float VertexSpacing { get => _meshVertexSpacing; set => _meshVertexSpacing = value; }
    public float TopLeftX { get => _topLeftX; set => _topLeftX = value; }
    public float TopLeftY { get => _topLeftZ; set => _topLeftZ = value; }
    public float MeshCeiling { get => _meshCeiling; set => _meshCeiling = value; }
    public float MeshFloor { get => _meshFloor; set => _meshFloor = value; }

    public Chunk[,] Chuncks { get => _chunks; }
    public float[,] HeightMap { get => _heightMap; set => _heightMap = value; }

    public bool HasMeshVertexParameters { get => _hasMeshVertexParameters; }
    public bool HasHeightMapData { get => _hasHeightMapData; }
    public bool HasMeshData { get => _hasMeshData; }

    // Inputs a heightMap Position and returns a position in world space
    public Vector3 HeightMapPositionToWorldSpace(float xx, float yy)
    {
        int coordX = (int)xx;
        int coordY = (int)yy;

        float vertexXPosition = _topLeftX + xx;
        float vertexZPosition = _topLeftZ + yy;

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
        _chunks[x,y] = chunk;
    }   
}

public struct Chunk
{
    public Mesh mesh;
    public Vector3 pos;

    public Chunk(Vector3 chunkPos, Mesh chunkMesh) : this()
    {
        pos = chunkPos;
        mesh = chunkMesh;
    }

}
