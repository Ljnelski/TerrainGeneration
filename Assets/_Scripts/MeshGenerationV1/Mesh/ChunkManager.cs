using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject _chunkObject;
    [SerializeField] private float _chunkUpdateFrequency = 0.1f;

    [Header("Mesh LOD")]
    [SerializeField] private Transform _viewerObject;
    [SerializeField] private float _LODThreshold;

    private PlaneMeshGenerator _meshGenerator;

    private GameObject[,] _chunkObjs;
    private Chunk[,] _chunks;
    private Queue<ChunkData> _chunksToDraw;

    private Stack<GameObject> _chunkObjectPool;

    public void Init(int width, int height)
    {
        Debug.Log("Init Called");

        _chunks = new Chunk[width, height];       
        _chunksToDraw = new Queue<ChunkData>();

        if (_chunkObjs == null)
        {
            GetChunkObjectsFromScene();
        }

        _meshGenerator = GetComponent<PlaneMeshGenerator>();
    }    

    public void BuildChunks()
    {
        if (_chunkObjs == null)
        {
            _chunkObjs = new GameObject[_chunks.GetLength(0), _chunks.GetLength(1)];
        }

        if (Application.IsPlaying(gameObject))
        {
            StartCoroutine(ChunkUpdateLoop());
        }
        else
        { 

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

    public void AddChunk(ChunkData chunkData)
    {
        _chunksToDraw.Enqueue(chunkData);
    }

    // TODO Make Update Existing Chunk
    private void LoadChunkFromChunkData(ChunkData chunkData)
    {
        Debug.Log("Drawing Chunk Mesh: " + chunkData.ChunkIndexX + ", " + chunkData.ChunkIndexY);
        Debug.Log("Chunk LOD" + chunkData.lod);

        Mesh newMesh = _meshGenerator.GenerateFromHeightMap(chunkData.HeightMap, chunkData.lod);

        Chunk chunk = _chunks[chunkData.ChunkIndexX, chunkData.ChunkIndexY];
        if (chunk == null)
        {
            chunk = new Chunk(chunkData.WorldPosition, newMesh);
        }
        else
        {
            chunk.WorldPosition = chunkData.WorldPosition;
        }

        GameObject chunkObj = _chunkObjs[chunkData.ChunkIndexX, chunkData.ChunkIndexY];
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
        chunkObj.name = "Chunk (" + chunkData.ChunkIndexX + ", " + chunkData.ChunkIndexY + ")";

        _chunkObjs[chunkData.ChunkIndexX, chunkData.ChunkIndexY] = chunkObj;
    }

    private void UpdateChunkObject(int x, int y)
    {
        Chunk chunk = _chunks[x, y];

        
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

    public void GetChunkObjectsFromScene()
    {
        MeshFilter[] chunksInScene = gameObject.transform.GetComponentsInChildren<MeshFilter>();

        _chunkObjectPool = new Stack<GameObject>();

        foreach (MeshFilter meshFilter in chunksInScene)
        {
            PoolChunk(meshFilter.gameObject);
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
}
