--- Mesh ---

meshSize: Refers to the number of Quads along one Axis of the meshSize. Eg; A mesh with meshSize of 4 has 4 Quads. THIS DOES NOT INCLUDE THE BORDER QUADS
meshSize = x;

meshVertexCount/VertexCount: Refers to the number of Vertexs that the actual mesh will contain.
meshVertexCount/VertexCount = meshSize + 1;

borderedVertexCount: Refers to the number of vertexs including border vertices which are soley used to calculate normals to remove seams 
borderedVertexCount = meshVertexCount + 2;

meshSimplificationIncrement: calculated from the MeshLOD Level. It Is the number of times the mesh is simplified or the incrementation across the heightMap. eg; a mesh with 4 quads can be simplified to 2 Quads with a meshSimplificationIncrement of 1
meshSimplificationIncrement = max(LOD * 2, 1);

vertexIncrement: The number of height map cells/pixels/indexs that a vertex is placed. Eg a vertexIncrement of 2 would mean that a vertex would only be placed every 2 pixels on the heightmap

--- Chunks ---

A world is made up of several chunks. Each Chunk has its own Mesh

worldSize: the number of Chunks on each axis that will be generated 

--- Height Map ---

The height map is a two dimesional float array with values ranging from 0 - 1 or -1 to 1. Each Cell can be visualized as a pixel on a image. each one of these 'pixels'
is translated into a vertex when the height map is generated.

The size of the height map is calculated as
  _worldSize * MeshVertex + 2;

The +2 is the extra border vertexs added on as padding for calculating the normals for each vertex that is on the edge of the world 

--- Visualization ---

. = a mesh vertex
x = a border vertex
| / = both are edges that make up the edge triangles

1 Quad:
.---.
| / |
.---.

Example 1

meshSize = 4
vertexCount = 5
borderedSize = 7
meshSimplificationIncrement = 0
verticesPerLine = 7

x---x---x---x---x---x---x
| / | / | / | / | / | / |
x---.---.---.---.---.---x
| / | / | / | / | / | / |
x---.---.---.---.---.---x
| / | / | / | / | / | / |
x---.---.---.---.---.---x
| / | / | / | \ | / | / |
x---.---.---.---.---.---x
| / | / | / | / | / | / |
x---x---x---x---x---x---x

Example 2

meshSize = 4
vertexCount = 5
borderedSize = 7
meshSimplificationIncrement = 1
verticesPerLine = 5

x---x-------x-------x---x
| / |   /   |   /   | / |
x---.-------.-------.---x
|  /|    /  |    /  |  /|
|/  |  /    |  /    |/  |
x---.-------.-------.---x
|  /|    /  |    /  |  /|
|/  |  /    |  /    |/  |
x---.-------.-------.---x
| / |   /   |   /   | / |
x---x-------x-------x---x

