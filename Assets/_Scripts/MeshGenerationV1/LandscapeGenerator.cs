using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private GameObject _chunk;

    [Header("LandscapeOptions")]
    [SerializeField] private int _worldSize; // number of chuncks to render

    [Header("Mesh Options")]
    const int _vertexCountX = 241; // Each chunckX is this big
    const int _vertexCountY = 241; // Each chunckY is this big
    [SerializeField] private float _vertexSpacing = 2f;
    [SerializeField] private float _maxHeight = 1f;
    [SerializeField] private float _minHeight = 0f;
    [Range(0,6)]
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
    

    private float _currentErosionIterations;
    //private bool isEroding = false;

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
        //heightMapData = _gradient.Generate(_vertexCountX, _vertexCountY);
        heightMapData = _noise.Generate(_vertexCountX * _worldSize, _vertexCountY * _worldSize);
        _landscapeData.SetHeightMapData(heightMapData);

        for (int chunkY = 0; chunkY < _worldSize; chunkY++)
        { 
            for (int chunkX = 0; chunkX < _worldSize; chunkX++)
            {

                float chunkPosX = _landscapeData.TopLeftX + (_vertexCountX * _vertexSpacing) * chunkX;
                float chunkPosZ = _landscapeData.TopLeftY + (_vertexCountY * _vertexSpacing) * chunkY;

                Vector3 chunkPos = new Vector3(chunkPosX, 0f, chunkPosZ);
                Mesh chunkMesh = null;//_meshGenerator.GenerateMesh(_landscapeData, chunkX, chunkY);

                Chunk chunk = new Chunk(chunkPos, chunkMesh);

                _landscapeData.SetChuck(chunkX, chunkY, chunk);
            }
        }

        LoadMesh();
        //UpdateMesh();
       
        if (_generateTexture)
        {
            UpdateTexture(heightMapData);
        }

        if(_saveTexture)
        {
           _textureGenerator.SaveTexture(heightMapData);
        }       
    }   

    public void LoadMesh()
    {
        foreach (var chunck in _landscapeData.Chuncks)
        {
            _debugPositions.Add(chunck.pos);
        }

        Debug.Log(_landscapeData.Chuncks.Length);
        Debug.Log(_landscapeData.Chuncks.GetLength(0));
        Debug.Log(_landscapeData.Chuncks.GetLength(1));
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
        if(_erosion == null)
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
        while(_currentErosionIterations > 0)
        {

            //Debug.Log("RUN EROSION");
            _landscapeData = _erosion.ErosionPass(_landscapeData);
            //UpdateMesh();
            _currentErosionIterations -= 1;          

            yield return new WaitForSeconds(_erosionTickTime);
        }
    }

    private void OnDrawGizmos()
    {
        if(_debugPositions != null)
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
