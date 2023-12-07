using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    [SerializeField] private TextureGenerator _textureGenerator;
    [SerializeField] private TextureDisplay _textureDisplay;
    [SerializeField] private MeshGenerator _meshGenerator;
    [SerializeField] private Noise _noise;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Erosion _erosion;

    // > Unity uses Y axis for height, so Y Axis is translated to the Z access when 
    // Generating points

    [SerializeField] private GameObject _chunkObject;

    [Header("LandscapeOptions")]
    [SerializeField] private int _worldSize; // number of chuncks to render

    [Header("Mesh Options")]
    const int _vertexCountX = 241; // Each chunckX is this big
    const int _vertexCountY = 241; // Each chunckY is this big
    [SerializeField] private float _vertexSpacing = 1f;
    [SerializeField] private float _maxHeight = 1f;
    [SerializeField] private float _minHeight = 0f;
    [Range(0, 6)]
    [SerializeField] private int _meshLOD = 0;

    [Header("Erosion")]
    [SerializeField] private float _erosionTickTime = 0.5f;
    [Min(0)]
    [SerializeField] private float _erosionIterations = 10f;

    [Header("Options")]

    [SerializeField] private bool _autoUpdate;
    [SerializeField] private bool _generateTexture;
    [SerializeField] private bool _saveTexture;

    [Header("Debug")]
    [SerializeField] private GameObject _debugFlag;
    private LandscapeData _landscapeData;
    private List<Vector3> _debugPositions;

    private List<GameObject> _chunkObjs;
    private float _currentErosionIterations;

    // Start is called before the first frame update
    void Awake()
    {
        _meshGenerator = GetComponent<MeshGenerator>();
        _noise = GetComponent<Noise>();
        _gradient = GetComponent<Gradient>();
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        _debugPositions = new List<Vector3>();
        _debugPositions.Clear();

#if UNITY_EDITOR
        GetScriptReferences();
#endif

        float[,] heightMapData;

        _landscapeData = new LandscapeData();
        _landscapeData.SetWorldParameters(_worldSize, _vertexCountX, _vertexCountY, _meshLOD, _vertexSpacing, _maxHeight, _minHeight);
        //heightMapData = _gradient.Generate(_vertexCountX * _worldSize, _vertexCountY * _worldSize);
        int heightMapSize = (_vertexCountX * _worldSize) + 2;

        heightMapData = _noise.Generate(heightMapSize, heightMapSize);
        _landscapeData.SetHeightMapData(heightMapData);

        GenerateChunks();
        LoadChunks();

        if (_generateTexture)
        {
            UpdateTexture(heightMapData);
        }

        if (_saveTexture)
        {
            _textureGenerator.SaveTexture(heightMapData);
        }
    }

    private void GenerateChunks()
    {
        _landscapeData.SetWorldParameters(_worldSize, _vertexCountX, _vertexCountY, _meshLOD, _vertexSpacing, _maxHeight, _minHeight);

        for (int chunkY = 0; chunkY < _worldSize; chunkY++)
        {
            for (int chunkX = 0; chunkX < _worldSize; chunkX++)
            {
                // Calculate the location where the chunck will be place in world space (current Set up is centre origin)               
                float chunkSize = (_vertexCountX - 1) * _vertexSpacing;

                float chunkPosX = _landscapeData.BottomLeftX + chunkX * chunkSize + chunkSize / 2;
                float chunkPosZ = _landscapeData.BottomLeftZ + chunkY * chunkSize + chunkSize / 2;

                Vector3 chunkPos = new Vector3(chunkPosX, 0f, chunkPosZ);

                // The coordinates of the chunks origin on the height map
                int chunkCoordX = chunkX * (_landscapeData.ChunkSizeX - 1);
                int chunkCoordY = chunkY * (_landscapeData.ChunkSizeY - 1);

                Mesh chunkMesh = _meshGenerator.GenerateMesh(_landscapeData, chunkCoordX, chunkCoordY);

                ChunkData chunk = new ChunkData(chunkPos, chunkMesh);

                _landscapeData.SetChuck(chunkX, chunkY, chunk);
            }
        }
    }

    // Gets a Reference to all pre existing chunk gameObjects in scene    
    public void LoadChunks()
    {
        GetChunkObjectsFromScene();

        UpdateChunkObjects();
    }

    public void GetChunkObjectsFromScene()
    {
        _chunkObjs = new List<GameObject>();

        Transform[] chunkObjects = gameObject.transform.GetComponentsInChildren<Transform>();
        for (var i = 0; i < chunkObjects.Length; i++)
        {
            if (chunkObjects[i] == gameObject.transform) continue;

            _chunkObjs.Add(chunkObjects[i].gameObject);
        }
    }

    private void InstantiateChunkObject(int xx, int yy)
    {
        GameObject chunkGameObject = Instantiate(_chunkObject, Vector3.zero, Quaternion.identity, gameObject.transform);
        chunkGameObject.name = "Chunk (" + xx + ", " + yy + ")";
        _chunkObjs.Add(chunkGameObject);
    }

    // Loads all chunkObjects in scene with the most recent meshes
    private void UpdateChunkObjects()
    {
        // Clear any excess chunks
        if (_chunkObjs.Count > _landscapeData.ChunkData.Length)
        {
            int lastChunkInUseIndex = _landscapeData.ChunkData.Length - 1;
            for (int i = _chunkObjs.Count - 1; i > lastChunkInUseIndex; i--)
            {
                DestroyChunk(i);
            }
        }

        // Update the chunks with 
        int worldWidth = _landscapeData.ChunkData.GetLength(0);
        for (int yy = 0; yy < _landscapeData.ChunkData.GetLength(1); yy++)
        {
            for (int xx = 0; xx < worldWidth; xx++)
            {
                int chunkObjIndex = xx + yy * worldWidth;

                if (chunkObjIndex > _chunkObjs.Count - 1)
                {
                    InstantiateChunkObject(xx, yy);
                }

                ChunkData chunk = _landscapeData.ChunkData[xx, yy];
                GameObject chunkObj = _chunkObjs[chunkObjIndex];
                chunkObj.transform.position = chunk.pos;
                chunkObj.GetComponent<MeshFilter>().sharedMesh = chunk.mesh;
            }
        }

       
    }
    
    private void DestroyChunk(int chunkIndex)
    {
        GameObject chunkObject = _chunkObjs[chunkIndex];
        _chunkObjs.RemoveAt(chunkIndex);

        if (Application.isPlaying)
        {
            Destroy(chunkObject);
        }
        else
        {
            DestroyImmediate(chunkObject);
        }
    }

    public void UpdateTexture(float[,] heightMapData)
    {
        //Texture2D newTexture = _textureGenerator.ValueFromHeightMap(heightMapData);
        Texture2D newTexture = _textureGenerator.ColourFromHeightMap(heightMapData);
        newTexture.Apply();
        _textureDisplay.DrawTexture(newTexture);
    }
    public void StartErosion()
    {
        if (_erosion == null)
        {
            _erosion = GetComponent<Erosion>();

        }
        _currentErosionIterations = _erosionIterations;
        StartCoroutine(RunErosion());
    }
    private void GetScriptReferences()
    {
        if (_textureGenerator == null)
        {
            _textureGenerator = GetComponent<TextureGenerator>();
        }

        if (_textureDisplay == null)
        {
            _textureDisplay = GetComponent<TextureDisplay>();
        }

        if (_meshGenerator == null)
        {
            _meshGenerator = gameObject.GetComponent<MeshGenerator>();
        }

        if (_noise == null)
        {
            _noise = gameObject.GetComponent<Noise>();
        }

        if (_gradient == null)
        {
            _gradient = gameObject.GetComponent<Gradient>();
        }
    }
    private IEnumerator RunErosion()
    {
        while (_currentErosionIterations > 0)
        {
            _landscapeData = _erosion.ErosionPass(_landscapeData);

            GenerateChunks();
            LoadChunks();

            _currentErosionIterations -= 1;

            yield return new WaitForSeconds(_erosionTickTime);
        }
    }
    private void OnDrawGizmos()
    {
        if (_debugPositions != null)
        {
            foreach (Vector3 pos in _debugPositions)
            {
                Gizmos.DrawWireSphere(pos, 20f);
            }
        }
    }
}

[System.Serializable]
public struct TerrainColors
{
    public float height;
    public Color colour;
}
