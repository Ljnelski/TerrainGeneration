using UnityEngine;

public class Chunk
{
    public Vector3 WorldPosition;
    public Mesh Mesh;

    public Chunk(Vector3 worldPosition, Mesh chunkMesh)
    {
        WorldPosition = worldPosition;
        Mesh = chunkMesh;
    }
}


public struct ChunkData
{
    public int ChunkIndexX;
    public int ChunkIndexY;

    public Vector3 WorldPosition;
    public float[,] HeightMap;

    public int lod;

    public ChunkData(int chunkIndexX, int chunkIndexY, float[,] heightMap, Vector3 worldPosition, int LOD)
    {
        ChunkIndexX = chunkIndexX;
        ChunkIndexY = chunkIndexY;

        WorldPosition = worldPosition;
        HeightMap = heightMap;

        lod = LOD;
    }    
}