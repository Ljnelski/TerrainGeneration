using System;
using System.Collections;
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
    [Header("Mesh Options")]
    [Range(0, 241)][SerializeField] private int _vertexCountX = 3;
    [Range(0, 241)][SerializeField] private int _vertexCountY = 3;
    [SerializeField] private float _vertexSpacing = 2f;
    [SerializeField] private float _maxHeight = 1f;
    [SerializeField] private float _minHeight = 0f;

    [Header("Erosion")]
    [SerializeField] private float _erosionTickTime = 0.5f;
    [Min(0)]
    [SerializeField] private float _erosionIterations = 10f;

    [Header("Options")]

    [SerializeField] private bool _autoUpdate;
    [SerializeField] private bool _generateTexture;
    [SerializeField] private bool _saveTexture;

    private LandscapeData _landscapeData;

    private float _currentErosionIterations;
    private bool isEroding = false;

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
#if UNITY_EDITOR
        GetScriptReferences();
#endif
        _landscapeData.SetMeshVertexParameters(_vertexCountX, _vertexCountY, _vertexSpacing, _maxHeight, _minHeight);


        float[,] heightMapData;

        heightMapData = _gradient.Generate(_vertexCountX, _vertexCountY);
        heightMapData = _noise.Generate(heightMapData);

        _landscapeData.SetHeightMapData(heightMapData);

        Mesh mesh;
        //mesh = _meshGenerator.GenerateMesh(heightMapData, _vertexSpacing, _maxHeight, _minHeight);

        mesh = _meshGenerator.GenerateMesh(_landscapeData);

        _landscapeData.SetMesh(mesh);





        //GetComponent<MeshFilter>().mesh = _meshGenerator.GenerateMesh(heightMapData, _vertexSpacing, _maxHeight, _minHeight);
        GetComponent<MeshFilter>().mesh = mesh;

        if (_generateTexture)
        {
            UpdateTexture(heightMapData);
        }

        if(_saveTexture)
        {
           _textureGenerator.SaveTexture(heightMapData);
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
            _erosion.ErosionPass(_landscapeData);
            _currentErosionIterations -= 1;

            yield return new WaitForSeconds(_erosionTickTime);
        }
    }
}

[System.Serializable]
public struct TerrainColors
{
    public float height;
    public Color colour;
}
