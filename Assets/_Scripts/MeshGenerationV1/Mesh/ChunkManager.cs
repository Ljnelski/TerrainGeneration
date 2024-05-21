using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject _chunkObject;
    [SerializeField] private float _chunkUpdateFrequency = 0.1f;

    private GameObject[,] _chunkObjs;
    private PlaneMeshGenerator _meshGenerator;

    private Chunk[,] _chunks;
    private Queue<ChunkData> _chunksToDraw;

    private Stack<GameObject> _chunkObjectPool;

    // Gets a Reference to all existing chunk gameObjects in scene     

    public void Init(int width, int height)
    {
        _chunks = new Chunk[width, height];
        _chunksToDraw = new Queue<ChunkData>();

        if (_chunkObjs == null)
        {
            GetChunkObjectsFromScene();
        }

        _meshGenerator = GetComponent<PlaneMeshGenerator>();
    }

    public void GetChunkObjectsFromScene()
    {
        MeshFilter[] chunksInScene = gameObject.transform.GetComponentsInChildren<MeshFilter>();

        _chunkObjectPool = new Stack<GameObject>();

        foreach (MeshFilter meshFilter in chunksInScene)
        {
            PoolChunk(meshFilter.gameObject);
        }
    }

    public void GetChunkDataFromScene()
    {
        _chunks = new Chunk[_chunkObjs.GetLength(0), _chunkObjs.GetLength(1)];

        for (int yy = 0; yy < _chunkObjs.GetLength(1); yy++)
        {
            for (int xx = 0; xx < _chunkObjs.GetLength(0); xx++)
            {
                //Debug.Log("_chunkObjs in ChunkDataCreation is null: " + (_chunkData == null));

                Debug.Log(_chunkObjs[xx, yy].name);

                Chunk data = new Chunk(
                _chunkObjs[xx, yy].GetComponent<Transform>().position,
                _chunkObjs[xx, yy].GetComponent<MeshFilter>().sharedMesh
                );

                _chunks[xx, yy] = data;
            }
        }
    }

    public void AddChunk(ChunkData chunkData)
    {
        _chunksToDraw.Enqueue(chunkData);
    }

    public void BuildChunks()
    {
        if (Application.IsPlaying(gameObject))
        {
            StartCoroutine(ChunkUpdateLoop());
        }
        else
        {
            if (_chunkObjs == null)
            {
                _chunkObjs = new GameObject[_chunks.GetLength(0), _chunks.GetLength(1)];
            }

            if (_chunks.Length != _chunkObjs.Length)
            {
                ResizeChunkObjs();
            }

            // Create Every Chunk Immediatly
            foreach (ChunkData chunkData in _chunksToDraw)
            {
                LoadChunkFromChunkData(chunkData);
            }
        }
    }

    private void LoadChunkFromChunkData(ChunkData chunkData)
    {
        Mesh mesh = _meshGenerator.GenerateFromHeightMap(chunkData.HeightMap);
        Chunk chunk = _chunks[chunkData.ChunkIndexX, chunkData.ChunkIndexY];

        if (chunk == null)
        {
            chunk = new Chunk(chunkData.WorldPosition, mesh);
        }
        else
        {
            chunk.WorldPosition = chunkData.WorldPosition;
        }

        _chunks[chunkData.ChunkIndexX, chunkData.ChunkIndexY] = chunk;

        UpdateChunkObject(chunkData.ChunkIndexX, chunkData.ChunkIndexY);
    }

    private void UpdateChunkObject(int x, int y)
    {
        Chunk chunk = _chunks[x, y];
        GameObject chunkObj = _chunkObjs[x, y];

        // If there is no ChunkGameObject, grab a new one
        if (chunkObj == null)
        {
            chunkObj = GetNewChunkObject();
        }

        // Free Old Mesh from memory
        if (_chunkObject.GetComponent<MeshFilter>())
        {
            Mesh mesh = _chunkObject.GetComponent<MeshFilter>().sharedMesh;
            Resources.UnloadAsset(mesh);
        }

        chunkObj.transform.position = chunk.WorldPosition;
        chunkObj.GetComponent<MeshFilter>().sharedMesh = chunk.Mesh;
        chunkObj.name = "Chunk (" + x + ", " + y + ")";

        _chunkObjs[x, y] = chunkObj;
    }

    private IEnumerator ChunkUpdateLoop()
    {
        while (true)
        {
            PollChunksForUpdates();

            if (_chunks.Length != _chunkObjs.Length)
            {
                ResizeChunkObjs();
            }

            yield return new WaitForSeconds(_chunkUpdateFrequency);
        }
    }

    private void PollChunksForUpdates()
    {
        if (_chunksToDraw.Count > 0)
        {
            LoadChunkFromChunkData(_chunksToDraw.Dequeue());
        }
    }

    private GameObject GetNewChunkObject()
    {
        GameObject chunkObj = null;


        if (_chunkObjectPool.Count > 0)
        {
            while (chunkObj == null)
            {
                chunkObj = _chunkObjectPool.Pop();
            }

            chunkObj?.SetActive(true);
        }

        if (chunkObj == null)
        {
            chunkObj = InstantiateChunkObject();
        }

        return chunkObj;
    }

    private GameObject InstantiateChunkObject()
    {
        GameObject chunkGameObject = Instantiate(_chunkObject, Vector3.zero, Quaternion.identity, gameObject.transform);
        return chunkGameObject;
    }

    private void PoolChunk(GameObject chunkObj)
    {
        chunkObj.SetActive(false);
        chunkObj.name = "Pooled Chunk";
        _chunkObjectPool.Push(chunkObj);
    }

    private void ResizeChunkObjs()
    {
        // Create new Array with new dimesions
        GameObject[,] _chunksUpdatedSize = new GameObject[_chunks.GetLength(0), _chunks.GetLength(1)];

        // Load Array with data while deleting excess data
        for (int yy = 0; yy < _chunkObjs.GetLength(1); yy++)
        {
            for (int xx = 0; xx < _chunkObjs.GetLength(0); xx++)
            {
                // If chunk is outside the array then delete it
                if (xx > _chunks.GetLength(0) - 1 || yy > _chunks.GetLength(0) - 1)
                {
                    GameObject chunkObj = _chunkObjs[xx, yy];

                    // Free Old Mesh from memory
                    if (_chunkObject.GetComponent<MeshFilter>())
                    {
                        Mesh mesh = _chunkObject.GetComponent<MeshFilter>().sharedMesh;
                        Resources.UnloadAsset(mesh);
                    }

                    DestroyChunk(chunkObj);
                }
                else // load chunk into new temp array
                {
                    _chunksUpdatedSize[xx, yy] = _chunkObjs[xx, yy];
                }

            }
        }

        _chunkObjs = _chunksUpdatedSize;
    }

    public void DestroyChunk(GameObject chunkObject)
    {
        // Mesh must be explicitly Destroyed to free it from memory
        Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;

        bool destroyChunk = _chunkObjectPool.Count >= 5;

        if (Application.isPlaying)
        {
            Destroy(mesh);
            if (destroyChunk)
            {
                Destroy(chunkObject);
            }
            else
            {
                PoolChunk(chunkObject);
            }

        }
        else
        {
            DestroyImmediate(mesh);
            DestroyImmediate(chunkObject);
        }
    }


    // Loads all chunkObjects in scene with the most recent meshes
    /*public void UpdateChunkGameObjects()
    {
        if (_chunkObjs == null)
        {
            _chunkObjs = new GameObject[_chunks.GetLength(0), _chunks.GetLength(1)];
        }

        if (_chunks.Length != _chunkObjs.Length)
        {
            ResizeChunkObjs();
        }

        // Update the chunks with 
        int worldWidth = _chunks.GetLength(0);
        for (int yy = 0; yy < _chunks.GetLength(1); yy++)
        {
            for (int xx = 0; xx < worldWidth; xx++)
            {
                Chunk chunk = _chunks[xx, yy];

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

                GameObject chunkObj = _chunkObjs[xx, yy];
                chunkObj.transform.position = chunk.WorldPosition;

                chunkObj.GetComponent<MeshFilter>().sharedMesh = chunk.Mesh;
            }
        }

        //Resources.UnloadUnusedAssets();
    }*/
}
