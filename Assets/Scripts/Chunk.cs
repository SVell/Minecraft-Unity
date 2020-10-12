using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    
    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth,VoxelData.ChunkHeight,VoxelData.ChunkWidth];

    private World world;
    
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        
        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    if(y < 1) voxelMap[x, y, z] = 0; // Bedrock
                    else if(y == VoxelData.ChunkHeight - 1) voxelMap[x, y, z] = 2; // Grass
                    else voxelMap[x, y, z] = 1; // Stone
                }
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 ||
            z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    AddVoxelDataToChunck(new Vector3(x,y,z));
                }
            }
        }
    }

    void AddVoxelDataToChunck(Vector3 pos)
    {
        for (int p = 0; p < 6; ++p)
        {
            // If attached - skip
            if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
            {
                byte blockID = voxelMap[(int) pos.x, (int) pos.y, (int) pos.z];
                
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p,0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p,1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p,2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p,3]]);
                
                AddTexture(world.blockTypes[blockID].GetTextureId(p));
                
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex +2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    void CreateMesh()
    { 
        // Add components to render
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Recalculate normal map
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;
        
        uvs.Add(new Vector2(x,y));
        uvs.Add(new Vector2(x,y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize,y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize,y + VoxelData.NormalizedBlockTextureSize));
    }
}
