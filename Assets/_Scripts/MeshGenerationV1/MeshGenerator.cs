using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    // Y translates to Z world space
    public Mesh GenerateMesh(float[,] heightMapData, float vertexSpacing, float maxHeight, float minHeight)
    {      
        Mesh mesh = new Mesh();

        int meshWidth = heightMapData.GetLength(0);
        int meshLength = heightMapData.GetLength(1);

        Vector3[] _vertices = new Vector3[meshWidth * meshLength];
        int[] _triangles = new int[(meshWidth - 1) * (meshLength - 1) * 6];
        Vector2[] _uvs = new Vector2[meshWidth * meshLength];

        float centerXOffset = (meshWidth - 1) * vertexSpacing / 2;
        float centerYOffset = (meshLength - 1) * vertexSpacing / 2;

        int currentVertexIndex;
        int currentTriangleIndex = 0;

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < meshLength; yy++)
        {
            for (int xx = 0; xx < meshWidth; xx++)
            {
                currentVertexIndex = yy * meshWidth + xx;

                // the Y value in the height map is converted into the Z position in world space,
                float vertexXPosition = xx * vertexSpacing - centerXOffset;
                float vertexZPosition = centerYOffset - yy * vertexSpacing;
                float vertexYPosition = Mathf.Max(heightMapData[xx, yy] * maxHeight, minHeight);

                _vertices[currentVertexIndex] = new Vector3(vertexXPosition, vertexYPosition, vertexZPosition);

                // yy and xx are offset -1 because of for loop
                float uvx = (float)xx / (meshWidth - 1);
                float uvy = (float)yy / (meshLength - 1);

                _uvs[currentVertexIndex] = new Vector2(uvx, uvy);


                // > Guard Clauses to prevent Tris being created beyond the edges
                if (!(xx < meshWidth - 1)) continue;
                if (!(yy < meshLength - 1)) continue;


                // > The current vertex index if not caught by guard would be the top left of a square of vertexs
                // and therefore two triangles can be created safely based on its position

                // > Tris are three integers, where the integers represent the index of the vertice located in the 
                // vertices array

                /*
                 * 0 .
                 * 2 1
                 */
                _triangles[currentTriangleIndex] = currentVertexIndex;
                _triangles[currentTriangleIndex + 1] = currentVertexIndex + meshWidth + 1;
                _triangles[currentTriangleIndex + 2] = currentVertexIndex + meshWidth;
                ///*
                // * 0 1
                // * . 2
                // */
                _triangles[currentTriangleIndex + 3] = currentVertexIndex;
                _triangles[currentTriangleIndex + 4] = currentVertexIndex + 1;
                _triangles[currentTriangleIndex + 5] = currentVertexIndex + meshWidth + 1;

                currentTriangleIndex += 6;

            }
        }

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        mesh.uv = _uvs;

        mesh.RecalculateNormals();
        return mesh;
    }
    public Mesh GenerateMesh(LandscapeData landscapeData)
    {  
        Mesh mesh = new Mesh();

        if (!landscapeData.HasMeshVertexParameters)
        {
            Debug.LogError("ERROR MeshGenerator: Does not have VertexDataParameters, which must be set in order to generate a mesh");
            return mesh;
        }

        int meshWidth = landscapeData.HeightMap.GetLength(0);
        int meshLength = landscapeData.HeightMap.GetLength(1);

        Vector3[] _vertices = new Vector3[meshWidth * meshLength];
        int[] _triangles = new int[(meshWidth - 1) * (meshLength - 1) * 6];
        Vector2[] _uvs = new Vector2[meshWidth * meshLength];

        float centerXOffset = (meshWidth - 1) * landscapeData.VertexSpacing / 2;
        float centerYOffset = (meshLength - 1) * landscapeData.VertexSpacing / 2;

        int currentVertexIndex;
        int currentTriangleIndex = 0;

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < meshLength; yy++)
        {
            for (int xx = 0; xx < meshWidth; xx++)
            {
                currentVertexIndex = yy * meshWidth + xx;

                _vertices[currentVertexIndex] = landscapeData.HeightMapPositionToWorldSpace(xx, yy);

                // yy and xx are offset -1 because of for loop
                float uvx = (float)xx / (meshWidth - 1);
                float uvy = (float)yy / (meshLength - 1);

                _uvs[currentVertexIndex] = new Vector2(uvx, uvy);


                // > Guard Clauses to prevent Tris being created beyond the edges
                if (!(xx < meshWidth - 1)) continue;
                if (!(yy < meshLength - 1)) continue;


                // > The current vertex index if not caught by guard would be the top left of a square of vertexs
                // and therefore two triangles can be created safely based on its position

                // > Tris are three integers, where the integers represent the index of the vertice located in the 
                // vertices array

                /*
                 * 0 .
                 * 2 1
                 */
                _triangles[currentTriangleIndex] = currentVertexIndex;
                _triangles[currentTriangleIndex + 1] = currentVertexIndex + meshWidth + 1;
                _triangles[currentTriangleIndex + 2] = currentVertexIndex + meshWidth;
                ///*
                // * 0 1
                // * . 2
                // */
                _triangles[currentTriangleIndex + 3] = currentVertexIndex;
                _triangles[currentTriangleIndex + 4] = currentVertexIndex + 1;
                _triangles[currentTriangleIndex + 5] = currentVertexIndex + meshWidth + 1;

                currentTriangleIndex += 6;

            }
        }

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        mesh.uv = _uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}
