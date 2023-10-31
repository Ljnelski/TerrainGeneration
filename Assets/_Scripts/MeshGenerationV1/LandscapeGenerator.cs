using System;
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

    [Range(0, 241)][SerializeField] private int _vertexCountX = 3;
    [Range(0, 241)][SerializeField] private int _vertexCountY = 3;
    [SerializeField] private float _vertexSpacing = 2f;
    [SerializeField] private float _maxHeight = 1f;
    [SerializeField] private float _minHeight = 0f;

    [SerializeField] private bool _autoUpdate;
    [SerializeField] private bool _generateTexture;
    [SerializeField] private bool _saveTexture;

    // Start is called before the first frame update
    void Awake()
    {
        _meshGenerator = GetComponent<MeshGenerator>();
        _noise = GetComponent<Noise>();
        _gradient = GetComponent<Gradient>();
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        if(_textureGenerator == null)
        {
            _textureGenerator = GetComponent<TextureGenerator>();
        }

        if(_textureDisplay == null)
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

        float[,] heightMapData = _gradient.Generate(_vertexCountX, _vertexCountY);
        heightMapData = _noise.Generate(heightMapData);


        GetComponent<MeshFilter>().mesh = _meshGenerator.GenerateMesh(heightMapData, _vertexSpacing, _maxHeight, _minHeight);

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
}

[System.Serializable]
public struct TerrainColors
{
    public float height;
    public Color colour;
}
