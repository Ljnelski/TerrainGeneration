using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public GameObject flag;
    // Y translates to Z world space
    public Mesh GenerateMesh(LandscapeData landscapeData, int heightMapCoordX, int heightMapCoordY)
    {
        Mesh mesh = new Mesh();

        if (!landscapeData.HasMeshVertexParameters)
        {
            Debug.LogError("ERROR MeshGenerator: Does not have VertexDataParameters, which must be set in order to generate a mesh");
            return mesh;
        }

        int meshWidth = landscapeData.ChunkSizeX;
        int meshLength = landscapeData.ChunkSizeY;

        int meshSimplifcationIncrement = Mathf.Max((landscapeData.MeshLOD) * 2, 1);
        int verticesPerLine = (meshWidth - 1) / meshSimplifcationIncrement + 1;

        // MeshData
        Vector3[] vertices = new Vector3[meshWidth * meshLength];
        Vector2[] uvs = new Vector2[meshWidth * meshLength];
        int[] triangles = new int[(meshWidth - 1) * (meshLength - 1) * 6];

        int currentVertexIndex = 0;
        int currentTriangleIndex = 0;

        float meshBottomLeftX = (meshWidth - 1) / -2f;
        float meshBottomLeftY = (meshLength - 1) / -2f;

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < meshLength; yy += meshSimplifcationIncrement)
        {
            for (int xx = 0; xx < meshWidth; xx += meshSimplifcationIncrement)
            {
                int heightMapSampleX = heightMapCoordX + xx;
                int heightMapSampleY = heightMapCoordY + yy;
                float heightMapValue = landscapeData.HeightMapValueToWorldSpace(landscapeData.HeightMap[heightMapSampleX, heightMapSampleY]);

                vertices[currentVertexIndex] = new Vector3(meshBottomLeftX + xx, heightMapValue, meshBottomLeftY + yy);

                float uvx = (float)xx / meshWidth;
                float uvy = (float)yy / meshLength;
                uvs[currentVertexIndex] = new Vector2(uvx, uvy);


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
                triangles[currentTriangleIndex] = currentVertexIndex;
                triangles[currentTriangleIndex + 1] = currentVertexIndex + verticesPerLine;
                triangles[currentTriangleIndex + 2] = currentVertexIndex + verticesPerLine + 1;
                ///*
                // * 0 1
                // * . 2
                // */
                triangles[currentTriangleIndex + 3] = currentVertexIndex;
                triangles[currentTriangleIndex + 4] = currentVertexIndex + verticesPerLine + 1;
                triangles[currentTriangleIndex + 5] = currentVertexIndex + 1;

                currentTriangleIndex += 6;
                currentVertexIndex++;
            }
        }

        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triCount = triangles.Length / 3;

        Debug.Log("Number of tris in mesh: " + triCount);
        // loop through all triangles
        for (int i = 0; i < triCount; i++)
        {
            int triangleIndex = i * 3;
            
            // Get the index for each vertex of the triangle
            int vertexIndexA = triangles[triangleIndex];
            int vertexIndexB = triangles[triangleIndex + 1];
            int vertexIndexC = triangles[triangleIndex + 2];

            // Get the point from each index 
            Vector3 pointA = vertices[vertexIndexA];
            Vector3 pointB = vertices[vertexIndexB];
            Vector3 pointC = vertices[vertexIndexC];

            //Debug.Log("triangle #: " + i);
            //Debug.Log("pointA: " + pointA);
            //Debug.Log("pointB: " + pointB);
            //Debug.Log("pointC: " + pointC);

            Vector3 AB = pointB - pointA;
            Vector3 AC = pointC - pointA;

            Vector3 normalABC = Vector3.Cross(AB, AC).normalized;
            vertexNormals[vertexIndexA] += normalABC;
            vertexNormals[vertexIndexB] += normalABC;
            vertexNormals[vertexIndexC] += normalABC;

            //Debug.Log("normalABC: " + normalABC);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = vertexNormals;

        return mesh;
    }
    public void CalculateNormals()
    {
        // !!Flat shading!!
        // Go through each tri

        // Create two direction vectors for triangle

        // Create the normal by calculating the cross product of the two vectors

        // store the normal

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
