using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public GameObject flag;
    // Y translates to Z world space
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

        int meshSimplifcationIncrement = Mathf.Max((landscapeData.MeshLOD) * 2, 1);
        int verticesPerLine = (meshWidth - 1) / meshSimplifcationIncrement + 1;

        // MeshData
        Vector3[] _vertices = new Vector3[meshWidth * meshLength];
        Vector2[] _uvs = new Vector2[meshWidth * meshLength];
        int[] _triangles = new int[(meshWidth - 1) * (meshLength - 1) * 6];

        int currentVertexIndex = 0;
        int currentTriangleIndex = 0;

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < meshLength; yy += meshSimplifcationIncrement)
        {
            for (int xx = 0; xx < meshWidth; xx += meshSimplifcationIncrement)
            {
                //Debug.Log("Index:" + currentVertexIndex);

                _vertices[currentVertexIndex] = landscapeData.HeightMapPositionToWorldSpace(xx, yy);
                // yy and xx are offset -1 because of for loop
                float uvx = (float)xx / meshWidth;
                float uvy = (float)yy / meshLength;

                _uvs[currentVertexIndex] = new Vector2(uvx, uvy);


                // > Guard Clause to prevent Tris being created beyond the edges
                //Debug.Log("CurrentVertexIndex: " + currentVertexIndex);
                if (!(xx < meshWidth - 1) || !(yy < meshLength - 1))
                {
                    currentVertexIndex++;
                    continue;
                }

                //Debug.Log("Createing triangle");


                // > The current vertex index if not caught by guard would be the top left of a square of vertexs
                // and therefore two triangles can be created safely based on its position

                // > Tris are three integers, where the integers represent the index of the vertice located in the 
                // vertices array

                /*
                 * 0 .
                 * 2 1
                 */
                _triangles[currentTriangleIndex] = currentVertexIndex;
                _triangles[currentTriangleIndex + 1] = currentVertexIndex + verticesPerLine;
                _triangles[currentTriangleIndex + 2] = currentVertexIndex + verticesPerLine + 1;
                ///*
                // * 0 1
                // * . 2
                // */
                _triangles[currentTriangleIndex + 3] = currentVertexIndex;
                _triangles[currentTriangleIndex + 4] = currentVertexIndex + verticesPerLine + 1;
                _triangles[currentTriangleIndex + 5] = currentVertexIndex + 1;

                currentTriangleIndex += 6;
                currentVertexIndex++;
            }
        }

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        mesh.uv = _uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
    public Mesh GenerateMesh(LandscapeData landscapeData, int chunkStartX, int chunkStartY)
    {
        Mesh mesh = new Mesh();

        if (!landscapeData.HasMeshVertexParameters)
        {
            Debug.LogError("ERROR MeshGenerator: Does not have VertexDataParameters, which must be set in order to generate a mesh");
            return mesh;
        }

        int meshWidth = landscapeData.HeightMap.GetLength(0);
        int meshLength = landscapeData.HeightMap.GetLength(1);

        int meshSimplifcationIncrement = Mathf.Max((landscapeData.MeshLOD) * 2, 1);
        int verticesPerLine = (meshWidth - 1) / meshSimplifcationIncrement + 1;

        // MeshData
        Vector3[] _vertices = new Vector3[meshWidth * meshLength];
        Vector2[] _uvs = new Vector2[meshWidth * meshLength];
        int[] _triangles = new int[(meshWidth - 1) * (meshLength - 1) * 6];

        int currentVertexIndex = 0;
        int currentTriangleIndex = 0;

        // > generate vertices by looping through height and width
        for (int yy = chunkStartY; yy < meshLength; yy += meshSimplifcationIncrement)
        {
            for (int xx = chunkStartX; xx < meshWidth; xx += meshSimplifcationIncrement)
            {
                //Debug.Log("Index:" + currentVertexIndex);

                _vertices[currentVertexIndex] = landscapeData.HeightMapPositionToWorldSpace(xx, yy);
                // yy and xx are offset -1 because of for loop
                float uvx = (float)xx / meshWidth;
                float uvy = (float)yy / meshLength;

                _uvs[currentVertexIndex] = new Vector2(uvx, uvy);


                // > Guard Clause to prevent Tris being created beyond the edges
                //Debug.Log("CurrentVertexIndex: " + currentVertexIndex);
                if (!(xx < meshWidth - 1) || !(yy < meshLength - 1))
                {
                    currentVertexIndex++;
                    continue;
                }

                //Debug.Log("Createing triangle");


                // > The current vertex index if not caught by guard would be the top left of a square of vertexs
                // and therefore two triangles can be created safely based on its position

                // > Tris are three integers, where the integers represent the index of the vertice located in the 
                // vertices array

                /*
                 * 0 .
                 * 2 1
                 */
                _triangles[currentTriangleIndex] = currentVertexIndex;
                _triangles[currentTriangleIndex + 1] = currentVertexIndex + verticesPerLine;
                _triangles[currentTriangleIndex + 2] = currentVertexIndex + verticesPerLine + 1;
                ///*
                // * 0 1
                // * . 2
                // */
                _triangles[currentTriangleIndex + 3] = currentVertexIndex;
                _triangles[currentTriangleIndex + 4] = currentVertexIndex + verticesPerLine + 1;
                _triangles[currentTriangleIndex + 5] = currentVertexIndex + 1;

                currentTriangleIndex += 6;
                currentVertexIndex++;
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

public struct MeshData
{
    public Vector3 position;
    public int width;
    public int height;
    public int meshLOD;

    public int chunkStartX;
    public int chunkStartY;
}
