using System;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    [SerializeField] private GameObject _rainMarker;

    [SerializeField] private int _maxDropletLifetime = 10;
    [SerializeField] private int _dropletIterations = 10;

    [SerializeField] private int _seed;

    [Header("Droplet Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _dropletInteria = 0.5f;
    [SerializeField] private float _dropletCapacity = 2f;
    [SerializeField] private float _dropletMinSlope = -0.05f;
    [SerializeField] private float _dropletDeposition = 0.25f;
    [SerializeField] private float _dropletErosion = 1f; // base amount of sediment to be taken per drop
    [SerializeField] private float _dropletGravity = 1f;
    [SerializeField] private float _dropletIntialWater = 1f;
    [SerializeField] private float _dropletEvaporation = 0.05f;
    [SerializeField] private int _dropletErodeRadius = 1;

    [Header("Debug")]
    [SerializeField] private bool _drawRainDrops = false;

    private List<List<Vector3>> _lastDropletTrails;
    private ErosionBrush _dropletBrush;

    private System.Random rand;

    // Start is called before the first frame update
    void Start()
    {
        rand = new System.Random(_seed);
        _lastDropletTrails = new List<List<Vector3>>();
    }

    // TODO -> Change Input output to Heightmap
    // Cell -> Refers the quad that the drop is located on, which is defined by four vertexs on the mesh
    public LandscapeData ErosionPass(LandscapeData landscapeData)
    {

        if(_dropletBrush.Cells == null)
        {
            _dropletBrush = CalculateBrush();
        }

        if (rand == null)
        {
            rand = new System.Random();
        }

        if (_lastDropletTrails == null)
        {
            _lastDropletTrails = new List<List<Vector3>>();
        }

        ClearRainDrops();

        // Droplet Values
        Vector2 pos;
        Vector2 dir;
        float speed;
        float water;
        float sediment;

        for (int i = 0; i < _dropletIterations; i++)
        {
            // Intalize the Droplet

            int heightMapWidth = landscapeData.HeightMap.GetLength(0) - 1;
            // Intial Position
            int posX = rand.Next(0, heightMapWidth);
            int posY = rand.Next(0, heightMapWidth);

            // Intail Direction
            int dirX = rand.Next(0, 1);
            int dirY = rand.Next(0, 1);

            pos = new Vector2(posX, posY);
            dir = new Vector2(dirX, dirY);
            speed = 1f;
            water = _dropletIntialWater;
            sediment = 0f;

            // Debug add list to stash droplets positions every iteration
            _lastDropletTrails.Add(new List<Vector3>());

            for (int k = 0; k < _maxDropletLifetime; k++)
            {
                //-----Gradient and Movement-----//       

                // If the water droplet has zero water, velocity or direction, kill it.
                if (speed < 0 || water <= 0) break;

                float depositX = pos.x;
                float depositY = pos.y;

                Vector2 gradient = CalculateGradient(landscapeData.HeightMap, pos);
                float height = CalculateHeight(landscapeData.HeightMap, pos);

                // Use the gradient to calculate the new direction
                Vector2 dirNew = dir * _dropletInteria - new Vector2(gradient.x, gradient.y) * (1 - _dropletInteria);
                dirNew.Normalize();

                // move the drop
                pos = pos + dirNew;

                // if the droplet is out of bounds kill it
                if (pos.x >= heightMapWidth - 1 || pos.x < 0 || pos.y >= heightMapWidth - 1 || pos.y < 0) break;

                float hNew = CalculateHeight(landscapeData.HeightMap, pos);
                float hDif = hNew - height;

                // ------ Erosion And Deposition ------ //                

                // If the height difference is positive the droplet when up a hill
                if (hDif > 0)
                {
                    // Drop just the sediment needed to fill the hole
                    if (sediment > hDif)
                    {
                        landscapeData.HeightMap = Deposit(landscapeData.HeightMap, depositX, depositY, hDif);

                        sediment -= hDif;
                    }
                    else if (sediment <= hDif)
                    {
                        landscapeData.HeightMap = Deposit(landscapeData.HeightMap, depositX, depositY, sediment);

                        sediment = 0;
                    }
                }
                else if (hDif < 0) // The droplet moves down the hill
                {
                    float capacity = Mathf.Max(-hDif, _dropletMinSlope) * speed * water * _dropletCapacity;

                    // Deposit sediment if the droplet has space for less sediment
                    if (sediment >= capacity)
                    {
                        float sedimentDeposit = (sediment - capacity) * _dropletDeposition;
                        landscapeData.HeightMap = Deposit(landscapeData.HeightMap, pos.x, pos.y, sedimentDeposit);

                        sediment -= sedimentDeposit;
                    }

                    // Erode from the previous position if the droplet has space for more sediment
                    if (sediment < capacity)
                    {
                        float sedimentErode = Mathf.Min((capacity - sediment) * _dropletErosion, -hDif);
                        landscapeData.HeightMap = ErodeHeightmap(_dropletBrush, landscapeData.HeightMap, depositX, depositY, sedimentErode);
                        sediment += sedimentErode;
                    }
                }

                // ----- Update Water & Speed ----- //

                speed = Mathf.Sqrt(speed * speed + hDif * _dropletGravity);
                water = water * (1 - _dropletEvaporation);
            }

        }

        return landscapeData;
    }

    Vector2 CalculateGradient(float[,] heightMap, Vector2 pos)
    {
        int coordX = (int)pos.x;
        int coordY = (int)pos.y;

        float cellPosX = pos.x - coordX;
        float cellPosY = pos.y - coordY;

        float hpxy = heightMap[coordX, coordY];
        float hpx1y = heightMap[coordX + 1, coordY];
        float hpxy1 = heightMap[coordX, coordY + 1];
        float hpx1y1 = heightMap[coordX + 1, coordY + 1];

        float gx = (hpx1y - hpxy) * (1 - cellPosY) + (hpx1y1 - hpxy1) * cellPosY;
        float gy = (hpxy1 - hpxy) * (1 - cellPosX) + (hpx1y1 - hpx1y) * cellPosX;

        return new Vector2(gx, gy);
    }

    float CalculateHeight(float[,] heightMap, Vector2 pos)
    {
        int coordX = (int)pos.x;
        int coordY = (int)pos.y;

        float cellPosX = pos.x - coordX;
        float cellPosY = pos.y - coordY;

        float hpxy = heightMap[coordX, coordY];
        float hpx1y = heightMap[coordX + 1, coordY];
        float hpxy1 = heightMap[coordX, coordY + 1];
        float hpx1y1 = heightMap[coordX + 1, coordY + 1];

        return (hpx1y1 * cellPosX + hpxy1 * (1 - cellPosX)) * cellPosY + (hpx1y * cellPosX + hpxy * (1 - cellPosX)) * (1 - cellPosY);
    }

    private float[,] Deposit(float[,] heightMap, float posX, float posY, float amount)
    {
        int coordX = (int)posX;
        int coordY = (int)posY;

        float cellPosX = posX - coordX;
        float cellPosY = posY - coordY;

        heightMap[coordX, coordY] += amount * (1 - cellPosX) * (1 - cellPosY);
        heightMap[coordX + 1, coordY] += amount * cellPosX * (1 - cellPosY);
        heightMap[coordX, coordY + 1] += amount * (1 - cellPosX) * cellPosY;
        heightMap[coordX + 1, coordY + 1] += amount * cellPosX * cellPosY;

        return heightMap;
    }

    private float[,] Deposit(float cellPosX, float cellPosY, float amount, float[,] heightMap)
    {
        // THESE ARE NOT CORRECT
        heightMap[(int)cellPosX, (int)cellPosY] += amount * (1 - cellPosX) * (1 - cellPosY);
        heightMap[(int)cellPosX + 1, (int)cellPosY] += amount * cellPosX * (1 - cellPosY);
        heightMap[(int)cellPosX, (int)cellPosY + 1] += amount * (1 - cellPosX) * cellPosY;
        heightMap[(int)cellPosX + 1, (int)cellPosY + 1] += amount * cellPosX * cellPosY;

        return heightMap;
    }

    // Remove value from the height map using a brush and return the total amount taken
    private float[,] ErodeHeightmap(ErosionBrush brush, float[,] heightMap, float posX, float posY, float amount)
    {
        float totalSedimentTaken = 0f;

        int coordX = (int)posX;
        int coordY = (int)posY;

        int heightMapWidth = heightMap.GetLength(0) - 1;
        int heightMapLength = heightMap.GetLength(1) - 1;

        int brushWidth = brush.Cells.GetLength(0) - 1;
        int brushLength = brush.Cells.GetLength(1) - 1;

        for (int yy = 0; yy < brushLength; yy++)
        {
            for (int xx = 0; xx < brushWidth; xx++)
            {
                BrushCell cell = brush.Cells[yy, xx];

                // If cell has no weight then skip it
                if (cell.weight <= 0) continue;

                // Calculate where on the height map the brush cell will be taken from.
                int heightMapCoordX = coordX + cell.coord.x;
                int heightMapCoordY = coordY + cell.coord.y;

                // Check if the point is inside the heightmap
                if (heightMapCoordX > heightMapWidth || heightMapCoordX < 0 || heightMapCoordY > heightMapLength || heightMapCoordY < 0)
                    continue;

                // Calculate how much sediment is taken
                float sedimentTaken = cell.weight * amount;
                heightMap[heightMapCoordX, heightMapCoordY] -= sedimentTaken;


                totalSedimentTaken += sedimentTaken;
            }
        }


        return heightMap;
    }

    // Precalculates a brush
    public ErosionBrush CalculateBrush()
    {
        int brushHeight;
        int brushWidth;

        BrushCell[,] brushCells;

        Vector2 brushCentre = new Vector2(0.5f, 0.5f);
        float totalWeight = 0;
        int wPosIndexOffset = _dropletErodeRadius - 1;


        if (_dropletErodeRadius == 1)
        {
            brushCells = new BrushCell[2, 2];

            brushCells[0, 0] = new BrushCell(new Vector2Int(0, 0), 0.25f);
            brushCells[1, 0] = new BrushCell(new Vector2Int(1, 0), 0.25f);
            brushCells[0, 1] = new BrushCell(new Vector2Int(0, 1), 0.25f);
            brushCells[1, 1] = new BrushCell(new Vector2Int(1, 1), 0.25f);

            totalWeight = 1f;
        }
        else
        {
            brushHeight = _dropletErodeRadius * 2;
            brushWidth = _dropletErodeRadius * 2;

            brushCells = new BrushCell[brushHeight, brushWidth];


            for (int brushIndexY = 0; brushIndexY < brushHeight; brushIndexY++)
            {
                for (int brushIndexX = 0; brushIndexX < brushWidth; brushIndexX++)
                {
                    Vector2Int brushCellCoords = new Vector2Int(brushIndexX - _dropletErodeRadius + 1, brushIndexY - _dropletErodeRadius + 1);

                    float maxDistance = _dropletErodeRadius - 0.5f;

                    float dx = brushCentre.x - brushCellCoords.x;
                    float dy = brushCentre.y - brushCellCoords.y;

                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float cellWeight = 0.0f;

                    if (distance < maxDistance)
                    {
                        cellWeight = 1 - distance / maxDistance;
                    }

                    totalWeight += cellWeight;
                    brushCells[brushIndexX, brushIndexY] = new BrushCell(brushCellCoords, cellWeight);
                }

            }
        }

        float totalWeightNormalized = 0;
        string line = "";

        // Normalize Weights
        for (int yy = 0; yy < brushCells.GetLength(0); yy++)
        {
            for (int xx = 0; xx < brushCells.GetLength(1); xx++)
            {
                brushCells[xx, yy].weight = brushCells[xx, yy].weight / totalWeight;
                totalWeightNormalized += brushCells[xx, yy].weight;
                line += "{ W:" + String.Format("{0:.00}", brushCells[xx, yy].weight) + ", coord: (" + brushCells[xx, yy].coord + ") } ";
            }
            line += "\n";
        }


        return new ErosionBrush(brushCells);
    }

    public void ClearRainDrops()
    {
        _lastDropletTrails.Clear();
    }

    // Takes the heights of the cell, and weighs them using the distance from the vector pos (0-1, 0-1) which is the position of the drop in the sell
    private Vector3 CalculateDropWorldSpace(LandscapeData landscapeData, float posX, float posY, int coordX, int coordY, float height)
    {
        Vector3 worldPos = landscapeData.HeightMapPositionToWorldSpace(coordX, coordY);
        worldPos.x += posX;
        worldPos.y = landscapeData.HeightMapValueToWorldSpace(height);
        worldPos.z += posY;


        return worldPos;
    }

    private void OnDrawGizmos()
    {
        if (_drawRainDrops)
        {
            if (_lastDropletTrails != null && _lastDropletTrails.Count > 0)
            {
                foreach (List<Vector3> dropletTrail in _lastDropletTrails)
                {
                    for (int k = 0; k < dropletTrail.Count; k++)
                    {
                        if (k == dropletTrail.Count - 1)
                        {
                            Gizmos.color = Color.white;
                        }
                        else
                        {
                            float placeInPath = (float)k / dropletTrail.Count;
                            Color wayPointColor = Color.Lerp(Color.black, Color.white, placeInPath);
                            Gizmos.color = wayPointColor;
                        }
                        Gizmos.DrawSphere(dropletTrail[k], 0.5f);

                        // Draw Line between spheres
                        if (k > 0 && dropletTrail.Count > 1)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(dropletTrail[k - 1], dropletTrail[k]);
                        }
                    }
                }
            }
        }
    }
}

struct WaterDrop
{
    public WaterDrop(int id, Vector2 pos, Vector2 dir, float vel)
    {
        // position on the heightmap
        Id = id;
        Pos = pos;
        Dir = dir;
        Vel = vel;
        Water = 0f;
        Sediment = 0f;
    }

    public int Id;
    public Vector2 Pos;
    public Vector2 Dir;
    public float Vel;
    public float Water;
    public float Sediment;
}

public struct ErosionBrush
{
    public ErosionBrush(BrushCell[,] brushCells)
    {
        Cells = brushCells;
    }

    public BrushCell[,] Cells;
}

public struct BrushCell
{
    public BrushCell(Vector2Int coord, float weight)
    {
        this.coord = coord;
        this.weight = weight;
    }

    public Vector2Int coord;
    public float weight;
}
