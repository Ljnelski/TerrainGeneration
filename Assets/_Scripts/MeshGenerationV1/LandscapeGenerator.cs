using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

public class LandscapeGenerator : MonoBehaviour
{
    // These are not really in use right now but I don't want to delete because I am not 100% sure
    [SerializeField] private TextureGenerator _textureGenerator;
    [SerializeField] private TextureDisplay _textureDisplay;

    // Generates the Mesh
    [SerializeField] private PlaneMeshGenerator _meshGenerator;

    // Heightmap 
    [SerializeField] private HeightMapGenerator _heightMapGenerator;
    [SerializeField] private Erosion _erosion;

    // Misc
    [SerializeField] private ChunkManager _chunkManager;

    // > Unity uses Y axis for height, so Y Axis is translated to the Z access when 
    // Generating points

    [Header("Viewer")]
    [SerializeField] Transform _viewerObject;

    [Header("LandscapeOptions")]
    [SerializeField] private int _worldSize; // number of chunks to render

    [Header("Mesh Options")]
    const int _meshSizeX = 240; // # of squares in mesh along X
    const int _meshSizeY = 240; // # of squares in mesh along Y
    [Range(0, 6)]

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

    private List<Vector3> _debugPositions;   
    private float _currentErosionIterations;

    private float[,] _heightMap;

    private Vector3 _previousViewerPosition;
    private int _chunkCoordX;
    private int _chunkCoordY;

    // --- PROPERTIES ----

    private float ChunkSizeX => MeshVertexCountX * 1f;
    private float ChunkSizeY => MeshVertexCountY * 1f;

    private int MeshVertexCountX => _meshSizeX + 1;
    private int MeshVertexCountY => _meshSizeY + 1;

    private int BorderedMeshVertexCountX => MeshVertexCountX + BORDER_VERTEX_COUNT;
    private int BorderedMeshVertexCountY=> MeshVertexCountY + BORDER_VERTEX_COUNT;

    private int HeightMapSizeX => MeshVertexCountX * _worldSize + BORDER_VERTEX_COUNT;
    private int HeightMapSizeY => MeshVertexCountY * _worldSize + BORDER_VERTEX_COUNT;

    private float WorldStartX => _worldSize * ChunkSizeX / -2f;
    private float WorldStartZ => _worldSize * ChunkSizeY / -2f;

    // --- CONSTANTS ---
    private const int BORDER_VERTEX_COUNT = 2;

    // Start is called before the first frame update
    void Awake()
    {
        _previousViewerPosition = _viewerObject.position;

        _meshGenerator = GetComponent<PlaneMeshGenerator>();
        _heightMapGenerator = GetComponent<HeightMapGenerator>();
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        _debugPositions = new List<Vector3>();
        _debugPositions.Clear();

#if UNITY_EDITOR
        GetScriptReferences();
#endif        

        _heightMap = _heightMapGenerator.GenerateHeightMap(HeightMapSizeX, HeightMapSizeY);
        _chunkManager.Init(_worldSize, _worldSize);

        ScaffoldChunks();

        _chunkManager.BuildChunks();


        if (_generateTexture)
        {
            UpdateTexture(_heightMap);
        }

        if (_saveTexture)
        {
            _textureGenerator.SaveTexture(_heightMap);
        }
    }

    public void ScaffoldChunks()
    {
        for (int chunkIndexY = 0; chunkIndexY < _worldSize; chunkIndexY++)
        {
            for (int chunkIndexX = 0; chunkIndexX < _worldSize; chunkIndexX++)
            {
                DrawChunk(chunkIndexX, chunkIndexY);            
            }
        }
    }

    // Calculate the World Space where the chunk should Lie
    private Vector3 CalculateChunkWorldPosition(int chunkIndexX, int chunkIndexY)
    {
        float chunkSize = _meshSizeY * _meshGenerator.VertexSpacing;
               
        float chunkPosX = WorldStartX + chunkIndexX * chunkSize + chunkSize / 2;
        float chunkPosZ = WorldStartZ + chunkIndexY * chunkSize + chunkSize / 2;

        return new Vector3(chunkPosX, 0f, chunkPosZ);
    }

    // Cuts out a chunk of the height map that has been generated
    private float[,] SampleHeightMapSection(float[,] heightMap, int chunkIndexX, int chunkIndexY)
    {
        int sampleIndexStartX = chunkIndexX * _meshSizeX;
        int sampleIndexStartY = chunkIndexY * _meshSizeX;

        float[,] heightMapSection = new float[BorderedMeshVertexCountX, BorderedMeshVertexCountY];

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

        return heightMapSection;
    }

    public void DrawChunk(int chunkIndexX, int chunkIndexY)
    {
        Vector3 chunkPosition = CalculateChunkWorldPosition(chunkIndexX, chunkIndexY);
        float[,] heightMapSection = SampleHeightMapSection(_heightMap, chunkIndexX, chunkIndexY);
        int LOD = (chunkIndexX == _chunkCoordX && chunkIndexY == _chunkCoordY) ? 0 : 3;

        //Debug.Log(_chunkCoordX + ", " + _chunkCoordY);
        //Debug.Log("Chunk IndexL " + chunkIndexX + "," + chunkIndexY + ", LOD: " + LOD);

        ChunkData chunkData = new ChunkData(chunkIndexX, chunkIndexY, heightMapSection, chunkPosition, LOD);
        _chunkManager.AddChunk(chunkData);
    }

    private void UpdateLODMap()
    {
        // Convert World space to chunk Coordinate

        int newChunkCoordX = (int)((_viewerObject.position.x - WorldStartX) / ChunkSizeX);
        int newChunkCoordY = (int)((_viewerObject.position.z - WorldStartZ) / ChunkSizeY);

        //Debug.Log("ViewerChunkCoord: " + newChunkCoordX + ", " + newChunkCoordY);

        // Detect Chunk Coordinate Change
        if(newChunkCoordX != _chunkCoordX || newChunkCoordY != _chunkCoordY)
        {
            int prevCoordX = _chunkCoordX;
            int prevCoordY = _chunkCoordY;


            _chunkCoordX = newChunkCoordX;
            _chunkCoordY = newChunkCoordY;

            DrawChunk(prevCoordX, prevCoordY);
            DrawChunk(_chunkCoordX, _chunkCoordY);
        }

    }

    private void Update()
    {
        // LOD CODE

        // 1 Check for Updated Position
        if(_previousViewerPosition != _viewerObject.position)
        {
            UpdateLODMap();
        }

        _previousViewerPosition = _viewerObject.position;
        // 2 if moved convert World Space to Chunk Coordinate space

        // 3 
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

        if (_heightMapGenerator == null)
        {
            _heightMapGenerator = gameObject.GetComponent<HeightMapGenerator>();
        }
    } 



    public void UpdateTexture(float[,] _heightMap)
    {
        //Texture2D newTexture = _textureGenerator.ValueFromHeightMap(_heightMap);
        Texture2D newTexture = _textureGenerator.ColourFromHeightMap(_heightMap);
        newTexture.Apply();
        _textureDisplay.DrawTexture(newTexture);
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
