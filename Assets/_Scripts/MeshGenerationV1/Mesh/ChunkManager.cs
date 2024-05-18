using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject _chunkObject;

    private GameObject[,] _chunkObjs;
    private ChunkData[,] _chunkData;

    // Should only be called when a new Heightmap is created
    public void LoadChunks(ChunkData[,] chunkData)
    {
        _chunkData = chunkData;

        if (_chunkObjs == null)
        {
            GetChunkObjectsFromScene();
        }

        UpdateChunkGameObjects();
    }

    // Gets a Reference to all existing chunk gameObjects in scene
    public void GetChunkObjectsFromScene()
    {
        MeshFilter[] chunksInScene = gameObject.transform.GetComponentsInChildren<MeshFilter>();
        int worldWidth = (int)Mathf.Sqrt(chunksInScene.Length);

        if (chunksInScene.Length <= 0)
        {
            Debug.Log("No Chunks In Scene");
            return;
        }

        _chunkObjs = new GameObject[worldWidth, worldWidth];


        //Debug.Log("Children: ");
        for (int i = 0; i < chunksInScene.Length; i++)
        {
            //Debug.Log(chunksInScene[i].name);
        }


        for (int yy = 0; yy < worldWidth; yy++)
        {
            for (int xx = 0; xx < worldWidth; xx++)
            {
                //Debug.Log("Adding Chunk");
                _chunkObjs[xx, yy] = chunksInScene[xx + yy * worldWidth].gameObject;
                //Debug.Log(_chunkObjs[xx, yy].name);
            }
        }
    }

    public void GetChunkDataFromScene()
    {
        _chunkData = new ChunkData[_chunkObjs.GetLength(0), _chunkObjs.GetLength(1)];

        for (int yy = 0; yy < _chunkObjs.GetLength(1); yy++)
        {
            for (int xx = 0; xx < _chunkObjs.GetLength(0); xx++)
            {
                //Debug.Log("_chunkObjs in ChunkDataCreation is null: " + (_chunkData == null));

                Debug.Log(_chunkObjs[xx, yy].name);

                ChunkData data = new ChunkData(
                _chunkObjs[xx, yy].GetComponent<Transform>().position,
                _chunkObjs[xx, yy].GetComponent<MeshFilter>().sharedMesh
                );

                _chunkData[xx, yy] = data;
            }
        }
    }

    public void UpdateChunkGameObjects()
    {
        if (_chunkObjs == null)
        {
            _chunkObjs = new GameObject[_chunkData.GetLength(0), _chunkData.GetLength(1)];
        }

        if (_chunkData.Length != _chunkObjs.Length)
        {
            Debug.Log("Resizing ChunkObjs");
            ResizeChunkObjs();
        }

        // Update the chunks with 
        int worldWidth = _chunkData.GetLength(0);
        for (int yy = 0; yy < _chunkData.GetLength(1); yy++)
        {
            for (int xx = 0; xx < worldWidth; xx++)
            {
                if (_chunkObjs[xx, yy] == null)
                {
                    InstantiateChunkObject(xx, yy);
                }

                // Free Old Mesh from memory
                if (_chunkObject.GetComponent<MeshFilter>())
                {
                    Mesh mesh = _chunkObject.GetComponent<MeshFilter>().sharedMesh;
                    Resources.UnloadAsset(mesh);
                }

                ChunkData chunk = _chunkData[xx, yy];
                GameObject chunkObj = _chunkObjs[xx, yy];
                chunkObj.transform.position = chunk.pos;
                chunkObj.GetComponent<MeshFilter>().sharedMesh = chunk.mesh;
            }
        }

        //Resources.UnloadUnusedAssets();
    }

    private void InstantiateChunkObject(int indexX, int yy)
    {
        GameObject chunkGameObject = Instantiate(_chunkObject, Vector3.zero, Quaternion.identity, gameObject.transform);
        chunkGameObject.name = "Chunk (" + indexX + ", " + yy + ")";
        _chunkObjs[indexX, yy] = chunkGameObject;
    }

    private void ResizeChunkObjs()
    {
        // Create new Array with new dimesions
        GameObject[,] _chunksUpdatedSize = new GameObject[_chunkData.GetLength(0), _chunkData.GetLength(1)];

        // Load Array with data while deleting excess data
        for (int yy = 0; yy < _chunkObjs.GetLength(1); yy++)
        {
            for (int xx = 0; xx < _chunkObjs.GetLength(0); xx++)
            {
                // If chunk is outside the array then delete it
                if (xx > _chunkData.GetLength(0) - 1 || yy > _chunkData.GetLength(0) - 1)
                {
                    // Free Old Mesh from memory
                    if (_chunkObject.GetComponent<MeshFilter>())
                    {
                        Mesh mesh = _chunkObject.GetComponent<MeshFilter>().sharedMesh;
                        Resources.UnloadAsset(mesh);
                    }

                    DestroyChunk(xx, yy);
                } 
                else // load chunk into new temp array
                {
                    _chunksUpdatedSize[xx, yy] = _chunkObjs[xx, yy];
                }

            }
        }

        _chunkObjs = _chunksUpdatedSize;
    }

    // Loads all chunkObjects in scene with the most recent meshes

    public void DestroyChunk(int indexX, int indexY)
    {
        GameObject chunkObject = _chunkObjs[indexX, indexY];

        // Mesh must be explicitly Destroyed to free it from memory
        Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;

        if (Application.isPlaying)
        {
            Destroy(mesh);
            Destroy(chunkObject);
        }
        else
        {
            DestroyImmediate(mesh);
            DestroyImmediate(chunkObject);
        }
    }
}
