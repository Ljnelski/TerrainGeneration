using UnityEngine;

public struct ChunkData
{
    public Mesh mesh;
    public Vector3 pos;
    
    public ChunkData(Vector3 chunkPos, Mesh chunkMesh) : this()
    {
        pos = chunkPos;
        mesh = chunkMesh;
    }
}
