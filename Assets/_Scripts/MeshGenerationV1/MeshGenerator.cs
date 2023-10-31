using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public Mesh GenerateMesh(float[,] heightMapData, float vertexSpacing, float maxHeight, float minHeight)
    {      
        Mesh mesh = new Mesh();

        int meshWidth = heightMapData.GetLength(0);
        int meshHeight = heightMapData.GetLength(1);

        Vector3[] _vertices = new Vector3[meshWidth * meshHeight];
        int[] _triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        Vector2[] _uvs = new Vector2[meshWidth * meshHeight];

        float centerXOffset = (meshWidth - 1) * vertexSpacing / 2;
        float centerYOffset = (meshHeight - 1) * vertexSpacing / 2;

        int currentVertexIndex;
        int currentTriangleIndex = 0;

        // > generate vertices by looping through height and width
        for (int yy = 0; yy < meshHeight; yy++)
        {
            for (int xx = 0; xx < meshWidth; xx++)
            {
                currentVertexIndex = yy * meshWidth + xx;

                float vertexXPosition = xx * vertexSpacing - centerXOffset;
                float vertexZPosition = centerYOffset - yy * vertexSpacing;
                float vertexYPosition = Mathf.Max(((heightMapData[xx, yy] + 1f) / 2f) * maxHeight, minHeight);

                //Debug.Log("Height Value for Vertex: " + vertexYPosition);

                _vertices[currentVertexIndex] = new Vector3(vertexXPosition, vertexYPosition, vertexZPosition);

                // yy and xx are offset -1 because of for loop
                float uvx = (float)xx / (meshWidth - 1);
                float uvy = (float)yy / (meshHeight - 1);

                _uvs[currentVertexIndex] = new Vector2(uvx, uvy);


                // > Guard Clauses to prevent Tris being created beyond the edges
                if (!(xx < meshWidth - 1)) continue;
                if (!(yy < meshHeight - 1)) continue;


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
}
