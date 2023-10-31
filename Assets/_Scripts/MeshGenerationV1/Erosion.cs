using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    [SerializeField] private int _seed;
    [SerializeField] private int _erosionPasses;
    [SerializeField] private int _dropletsPerPass;

    private List<Vector3> _droplets;
    private int _remaingPasses;

    private System.Random rand; 

    // Start is called before the first frame update
    void Start()
    {
        _droplets = new List<Vector3>();
        rand = new System.Random(_seed);
    }

    // Update is called once per frame
    void Update()
    {
        if (_remaingPasses > 0)
        {
            ErosionPass();

            _remaingPasses--;
        }
    }

    public void StartErosion(float[,] heightMap, float meshWidth, float meshLength, float meshHeight)
    {
        _remaingPasses = _erosionPasses;
    }

    private void ErosionPass()
    {
        for (int i = 0; i < _dropletsPerPass; i++)
        {
            _droplets.Add(new Vector3(rand.Next(-100, 100), 1f, rand.Next(-100, 100)));
        }
    }

    private void OnDrawGizmos()
    {
        if(_droplets != null)
        {
            for (int i = 0; i < _droplets.Count; i++)
            {
                Gizmos.DrawSphere(_droplets[i], 0.5f);
            }
        }        
    }
}
