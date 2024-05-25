using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct MeshSettingsData
{
    [SerializeField] public float MeshSizeX;
    [SerializeField] public float MeshSizeY;

    [SerializeField] public float Floor;
    [SerializeField] public float Ceiling;
}

[CreateAssetMenu(fileName ="MeshSettings", menuName ="ProceduralTerrainGenerator/MeshSettings")]
public class MeshSettings : ScriptableObject
{
    [SerializeField] private MeshSettingsData _meshSettingsData;
    [SerializeField] private int _lodIndex;

    [SerializeField] private List<int> _lods;

    public float MeshSizeX
    {
        get => _meshSettingsData.MeshSizeX;
        set => _meshSettingsData.MeshSizeX = value;
    }

    public float MeshSizeY
    {
        get => _meshSettingsData.MeshSizeY;
        set => _meshSettingsData.MeshSizeY = value;
    }

    public int LOD => _lods[_lodIndex];
    public List<int> SupportedLODs
    {
        get
        {
            if (_lods == null)
            {
                _lods = new List<int>();

                for (int i = 0; i < _meshSettingsData.MeshSizeX; i++)
                {
                    if (_meshSettingsData.MeshSizeX % i == 0)
                    {
                        _lods.Add(i);
                        Debug.Log(i);
                    }
                }
            }

            return _lods;
        }
    }

    private void OnValidate()
    {
        _lods = new List<int>();

        for (int i = 0; i < _meshSettingsData.MeshSizeX; i++)
        {
            if (_meshSettingsData.MeshSizeX % i == 0)
            {
                _lods.Add(i);
                Debug.Log(i);
            }
        }
    }
}

