using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    // These are not really in use right now but I don't want to delete because I am not 100% sure
    [SerializeField] private TextureGenerator _textureGenerator;
    [SerializeField] private TextureDisplay _textureDisplay;

    // Generates the Mesh
    [SerializeField] private PlaneMeshGenerator _meshGenerator;

    // Heightmap Modifiers
    [SerializeField] private Noise _noise;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Erosion _erosion;

    // Misc
    [SerializeField] private ChunkManager _chunkManager;

    // > Unity uses Y axis for height, so Y Axis is translated to the Z access when 
    // Generating points

    [Header("LandscapeOptions")]
    [SerializeField] private int _worldSize; // number of chuncks to render

    [Header("Mesh Options")]
    const int _vertexCountX = 240; // Each chunckX is this big
    const int _vertexCountY = 240; // Each chunckY is this big
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
    //private LandscapeData _landscapeData;
    private List<Vector3> _debugPositions;

    private float _currentErosionIterations;

    // Start is called before the first frame update
    void Awake()
    {
        _meshGenerator = GetComponent<PlaneMeshGenerator>();
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

        heightMapData = _gradient.Generate(_vertexCountX * _worldSize, _vertexCountY * _worldSize);
        int heightMapSize = (_vertexCountX * _worldSize) + 2; // ADD two for edge vertices

        heightMapData = _noise.Generate(heightMapSize, heightMapSize);
        //_landscapeData.SetHeightMapData(heightMapData);

        GenerateChunks(heightMapData);

        if (_generateTexture)
        {
            UpdateTexture(heightMapData);
        }

        if (_saveTexture)
        {
            _textureGenerator.SaveTexture(heightMapData);
        }
    }
    private void GenerateChunks(float[,] heightMap)
    {
        ChunkData[,] chunkData = new ChunkData[_worldSize, _worldSize];

        for (int chunkIndexY = 0; chunkIndexY < _worldSize; chunkIndexY++)
        {
            for (int chunkIndexX = 0; chunkIndexX < _worldSize; chunkIndexX++)
            {
                // Calculate the location where the chunck will be place in world space (current Set up is centre origin)               
                float chunkSize = (_vertexCountX - 1) * _meshGenerator.VertexSpacing;

                float chunkBottomLeftX = _worldSize * chunkSize / -2f;
                float chunkBottomLeftZ = _worldSize * chunkSize / -2f;

                float chunkPosX = chunkBottomLeftX + chunkIndexX * chunkSize + chunkSize / 2;
                float chunkPosZ = chunkBottomLeftZ + chunkIndexY * chunkSize + chunkSize / 2;

                Vector3 chunkPos = new Vector3(chunkPosX, 0f, chunkPosZ);

                int sampleIndexStartX = chunkIndexX * (_vertexCountX - 1);
                int sampleIndexStartY = chunkIndexY * (_vertexCountY - 1);

                float[,] heightMapSection = new float[_vertexCountX + 2, _vertexCountY + 2];

                // Take Section of the heightmap and convert it to a chunk
                for (int yy = 0; yy < heightMapSection.GetLength(1); yy++)
                {
                    for (int xx = 0; xx < heightMapSection.GetLength(0); xx++)
                    {
                        int sampleIndexX = sampleIndexStartX + xx;
                        int sampleIndexY = sampleIndexStartY + yy;

                        heightMapSection[xx, yy] = heightMap[sampleIndexX, sampleIndexY];
                    }
                }

                Mesh chunkMesh = _meshGenerator.GenerateFromHeightMap(heightMapSection);
                chunkData[chunkIndexX, chunkIndexY] = new ChunkData(chunkPos, chunkMesh);
            }
        }

        _chunkManager.LoadChunks(chunkData);
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
        //StartCoroutine(RunErosion());
    }
    private void GetScriptReferences()
    {
        if (_chunkManager == null)
        {
            _chunkManager = GetComponent<ChunkManager>();
        }

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
            _meshGenerator = gameObject.GetComponent<PlaneMeshGenerator>();
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
        // TODO Fix Erosion with Removal of landscapeData
        while (_currentErosionIterations > 0)
        {
            //_landscapeData = _erosion.ErosionPass(_landscapeData);

            //GenerateChunks();
            //_chunkManager.LoadChunks();

            _currentErosionIterations -= 1;

            yield return new WaitForSeconds(_erosionTickTime);
        }
    }

    // TODO Fix the Erosion with the new system
    private void Update()
    {
        if (_currentErosionIterations > 0)
        {
            //_landscapeData = _erosion.ErosionPass(_landscapeData);

            //GenerateChunks();
            //_chunkManager.LoadChunks();

            _currentErosionIterations -= 1;
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
