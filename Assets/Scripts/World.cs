using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Threading;
using Random = System.Random;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;
    
    [Header("World Generation")]
    public BiomAttributes biome;
    
    
    [Range(0,1)]
    public float globalLightLevel;
    public Color day;
    public Color night;
    
    
    public Transform player;
    public Vector3 spawnPosition;
    
    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    
    public ChunkCoord playerChunkCoord;
    private ChunkCoord playerLastChunkCoord;
    
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    public GameObject debugScreen;

    private bool _inUI = false;

    private bool isApplyingModifications = false;

    public GameObject creativeInvWindow;
    public GameObject cursorSlot;
    public DragAndDrop dragAndDrop;

    private Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();
    private void Start()
    {
        //string jsonExport = JsonUtility.ToJson(settings);
        
        //File.WriteAllText(Application.dataPath + "/setting.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/setting.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);
        
        UnityEngine.Random.InitState(settings.seed);
        
        Shader.SetGlobalFloat("minGlobalLightLevel",VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel",VoxelData.maxLightLevel);
        
        ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        ChunkUpdateThread.Start();
        
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        
        
    }

    private void Update()
    {
        Shader.SetGlobalFloat("GlobalLightLevel",globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }

        

        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }
        

        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
            {
                chunksToDraw.Dequeue().CreateMesh();
            }
        }
    }

    private void FixedUpdate()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord)) 
        { 
            CheckViewDistance();
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x,z] = new Chunk(newChunk,this);
                chunksToCreate.Add(newChunk);
            }
        }

        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    private void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!isApplyingModifications)
            {
                ApplyModifications();
            }
            
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        ChunkUpdateThread.Abort();
    }

    void ApplyModifications()
    {
        isApplyingModifications = true;
        
        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            
            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }

        isApplyingModifications = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x,z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }
    
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        
        activeChunks.Clear();

        // Loops through all chunks currently within view distance
        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; ++x)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; ++z)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    // Check if is active, if not, activate it
                    if (chunks[x, z] == null)
                    {
                        chunks[x,z] = new Chunk(new ChunkCoord(x,z),this);
                        chunksToCreate.Add(new ChunkCoord(x,z));
                    }
                    else if (!chunks[x,z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x,z));

                    // Check through previousActiveChunk to se if this chunk is there/ If it is, remove it from the list
                    for (int i = 0; i < previouslyActiveChunks.Count; ++i)
                    {
                        if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunk list are no longer in player's view distance? so loop through and disable them
        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;
    }
    
    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return null;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }

        return new VoxelState(GetVoxel(pos));
    }

    public bool inUI
    {
        get { return _inUI; }
        set
        {
            // TODO: Remember last slot and return item to it when inv closes 
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                creativeInvWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                dragAndDrop.OnSetActive();
                creativeInvWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }
    
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        
        // IMMUTABLE PASS
        // If outside of the world, return air
        if (!IsVoxelInWorld(pos))
            return 0;
        
        // If bottom of the chunk, return bedrock
        if (yPos == 0)
            return 1;
        
        // BASIC TERRAIN PASS

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0f, biome.terrainScale) + biome.solidGroundHeight);
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        // SECOND PASS

        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.loads)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        
        // TREE PASS
        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 2000, biome.treePlacementScale) >
                    biome.treePlacementThreshold)
                {
                    lock (modifications)
                    {
                        modifications.Enqueue(Structure.MakeTree(pos,biome.minTreeHeight,biome.maxTreeHeight));
                    }
                }
            }
        }
        

        return voxelValue;

    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 &&
            coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public Sprite icon;
    public bool renderNeighborFaces;
    public float transparency;

    [Header("Texture Values")] 
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    
    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            break;
            case 1:
                return frontFaceTexture;
                break;
            case 2:
                return topFaceTexture;
                break;
            case 3:
                return bottomFaceTexture;
                break;
            case 4:
                return leftFaceTexture;
                break;
            case 5:
                return rightFaceTexture;
                break;
            default:
                Debug.Log("Error in GetTextureId; Invalid face index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }
    
    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")] 
    public string version;
    
    [Header("Performance")]
    public int viewDistance;
    
    [Header("Controls")]
    [Range(1f,20)]
    public float mouseSensitivity = 5;

    [Header("World Gen")] 
    public int seed;
}
