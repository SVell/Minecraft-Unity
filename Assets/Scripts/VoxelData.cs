using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 5;
    public static readonly int ChunkHeight = 7;
    public static readonly int WorldSizeInChunks = 100;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int ViewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlocks = 4;

    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float) TextureAtlasSizeInBlocks; }
    }
    
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        // Should be done in clockwise order for render
        new Vector3(0f, 0f, 0f), // 0
        new Vector3(1f, 0f, 0f), // 1
        new Vector3(1f, 1f, 0f), // 2
        new Vector3(0f, 1f, 0f), // 3
        new Vector3(0f, 0f, 1f), // 4
        new Vector3(1f, 0f, 1f), // 5
        new Vector3(1f, 1f, 1f), // 6
        new Vector3(0f, 1f, 1f), // 7
    };
    
    // Adjustment faces of a block
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0f, 0f, -1f),
        new Vector3(0f, 0f, 1f),
        new Vector3(0f, 1f, 0f),
        new Vector3(0f, -1f, 0f),
        new Vector3(-1f, 0f, 0f),
        new Vector3(1f, 0f, 0f)
    };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Bottom, Left, Right
        
        //0 1 2 2 1 3
        {0,3,1,2}, // Back Face of a block
        {5,6,4,7}, // Front Face
        {3,7,2,6}, // Top Face
        {1,5,0,4}, // Bottom Face
        {4,7,0,3}, // Left Face
        {1,2,5,6} // Right Face
    };
    
    // Texture draw order
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,0f),
        new Vector2(1f,1f), 
    };
}
