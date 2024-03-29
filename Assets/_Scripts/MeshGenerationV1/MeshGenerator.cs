using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public GameObject flag;
    // Y translates to Z world space
    public Mesh GenerateMesh(LandscapeData landscapeData, int heightMapCoordX, int heightMapCoordY)
    {
        if (!landscapeData.HasMeshVertexParameters)
        {
            Debug.LogError("ERROR MeshGenerator: Does not have VertexDataParameters, which must be set in order to generate a mesh");
            return new Mesh();
        }

        int meshSize = landscapeData.ChunkSizeX;
        int borderedSize = meshSize + 2;

        float meshBottomLeftX = (meshSize - 1) * landscapeData.VertexSpacing / -2f;
        float meshBottomLeftZ = (meshSize - 1) * landscapeData.VertexSpacing / -2f;

        int meshSimplifcationIncrement = Mathf.Max((landscapeData.MeshLOD) * 2, 1);
        int verticesPerLine = meshSize / meshSimplifcationIncrement + 1;

        float vertexSpacing = landscapeData.VertexSpacing;

        // MeshData
        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        
        // map out all the vertex Indexs.
        for (int yy = 0; yy < borderedSize; yy++)
        {
            string line = "[";
            for (int xx = 0; xx < borderedSize; xx++)
            {
                
                // if the vertex is a border vertex then give it a negative index, otherwise give it a positive index
                bool isBorderVertex = xx == 0 || xx == borderedSize - 1 || yy == 0 || yy == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[xx, yy] = borderVertexIndex;
                    line += " " + borderVertexIndex + ",";

                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[xx, yy] = meshVertexIndex;
                    line += " " + meshVertexIndex + ",";
                    meshVertexIndex++;
                }

            }
        }

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < borderedSize; yy += meshSimplifcationIncrement)
        {
            string line = "[";
            for (int xx = 0; xx < borderedSize; xx += meshSimplifcationIncrement)
            {
                int vertexIndex = vertexIndicesMap[xx, yy];
                if(vertexIndex >= 0)
                    line += " " + vertexIndex + ",";

                int heightMapSampleX = heightMapCoordX + xx;
                int heightMapSampleY = heightMapCoordY + yy;
                float heightMapValue = landscapeData.HeightMapValueToWorldSpace(landscapeData.HeightMap[heightMapSampleX, heightMapSampleY]);

                // finds a percent [0-1] value of where inside the actual mesh the vertex is 0 => left/bottom, 1 => right/top (also acts as UV value)
                float percentX = (float)(xx - meshSimplifcationIncrement) / meshSize;
                float percentY = (float)(yy - meshSimplifcationIncrement) / meshSize;
                Vector2 percent = new Vector2(percentX, percentY);

                Vector3 vertexPosition = new Vector3(
                    meshBottomLeftX + percentX * meshSize * vertexSpacing,
                    heightMapValue,
                    meshBottomLeftZ + percentY * meshSize * vertexSpacing);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                // > Guard Clause to prevent Tris being created beyond the edges
                if (!(xx < borderedSize - 1) || !(yy < borderedSize - 1)) continue;
                

                // > The current vertex index if not caught by guard would be the top left of a square of vertexs
                // and therefore two triangles can be created safely based on its position 

                //BC
                //AD
                int indexA = vertexIndicesMap[xx, yy];
                int indexB = vertexIndicesMap[xx, yy + meshSimplifcationIncrement];
                int indexC = vertexIndicesMap[xx + meshSimplifcationIncrement, yy + meshSimplifcationIncrement];
                int indexD = vertexIndicesMap[xx + meshSimplifcationIncrement, yy];

                // ABC
                meshData.AddTriangle(indexA, indexB, indexC);

                // ACD
                meshData.AddTriangle(indexA, indexC, indexD);
            }
        }

        meshData.CalculateNormals();

        return meshData.CreateMesh();
    }
}

public struct MeshData
{
    private Vector3[] meshVertices;
    private Vector3[] vertexNormals;
    private int[] meshTriangles;

    private Vector3[] borderVertices;
    private int[] borderTriangles;

    private Vector2[] uvs;

    private int meshTriIndex;
    private int borderTriIndex;

    public MeshData(int meshSize)
    {
        meshVertices = new Vector3[meshSize * meshSize];
        meshTriangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
        uvs = new Vector2[meshSize * meshSize];

        borderVertices = new Vector3[4 * meshSize + 4];
        borderTriangles = new int[8 * meshSize * 3];

        vertexNormals = new Vector3[meshVertices.Length];

        meshTriIndex = 0;
        borderTriIndex = 0;
    }

    public void AddVertex(Vector3 vertexPos, Vector3 vertexUV, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            //Debug.LogError("Adding a vertex");
            meshVertices[vertexIndex] = vertexPos;
            uvs[vertexIndex] = vertexUV;
        }
    }

    public void AddTriangle(int vertIndexA, int vertIndexB, int vertIndexC)
    {
        //Debug.Log("VetexIndexs: " + vertIndexA + "," + vertIndexB + ", " + +vertIndexC);
        if (vertIndexA < 0 || vertIndexB < 0 || vertIndexC < 0)
        {
            //Debug.Log("Adding a borderTri");
            borderTriangles[borderTriIndex] = vertIndexA;
            borderTriangles[borderTriIndex + 1] = vertIndexB;
            borderTriangles[borderTriIndex + 2] = vertIndexC;
            borderTriIndex += 3;
        }
        else
        {
            //Debug.Log("Adding a meshTri");
            meshTriangles[meshTriIndex] = vertIndexA;
            meshTriangles[meshTriIndex + 1] = vertIndexB;
            meshTriangles[meshTriIndex + 2] = vertIndexC;
            meshTriIndex += 3;
        }

    }

    public void CalculateNormals()
    {
        // Loop through all mesh Triangles
        int meshTriCount = meshTriangles.Length / 3;
        for (int i = 0; i < meshTriCount; i++)
        {
            int triangleIndex = i * 3;

            // Get the index for each vertex of the triangle
            int vertexIndexA = meshTriangles[triangleIndex];
            int vertexIndexB = meshTriangles[triangleIndex + 1];
            int vertexIndexC = meshTriangles[triangleIndex + 2];

            Vector3 normalABC = CalculateVertexNormal(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += normalABC;
            vertexNormals[vertexIndexB] += normalABC;
            vertexNormals[vertexIndexC] += normalABC;
        }

        // loop through all border triangles
        int borderTriCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriCount; i++)
        {
            int triangleIndex = i * 3;

            // Get the index for each vertex of the triangle
            int vertexIndexA = borderTriangles[triangleIndex];
            int vertexIndexB = borderTriangles[triangleIndex + 1];
            int vertexIndexC = borderTriangles[triangleIndex + 2];

            Vector3 normalABC = CalculateVertexNormal(vertexIndexA, vertexIndexB, vertexIndexC);

            // if the vertex is negative, its a border vertex which doesnt need a normal
            if(vertexIndexA > 0) vertexNormals[vertexIndexA] += normalABC;
            if (vertexIndexB > 0) vertexNormals[vertexIndexB] += normalABC;
            if (vertexIndexC > 0) vertexNormals[vertexIndexC] += normalABC;
        }
    }

    private Vector3 CalculateVertexNormal(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        Vector3 vertA;
        Vector3 vertB;
        Vector3 vertC;

        //Debug.Log("CalculateVertexNormal: VertexIndexs: ");
        //Debug.Log("vertexIndexA: " + vertexIndexA);
        //Debug.Log("vertexIndexB: " + vertexIndexB);
        //Debug.Log("vertexIndexC : " + vertexIndexC);
        // Get the position from each vertex
        vertA = (vertexIndexA >= 0) ? meshVertices[vertexIndexA] : borderVertices[-vertexIndexA - 1];
        vertB = (vertexIndexB >= 0) ? meshVertices[vertexIndexB] : borderVertices[-vertexIndexB - 1];
        vertC = (vertexIndexC >= 0) ? meshVertices[vertexIndexC] : borderVertices[-vertexIndexC - 1];

        Vector3 AB = vertB - vertA;
        Vector3 AC = vertC - vertA;

        return Vector3.Cross(AB, AC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.uv = uvs;
        mesh.normals = vertexNormals;

        return mesh;
    }

}
