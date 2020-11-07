using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attributes")]
public class BiomAttributes : ScriptableObject
{
    [Header("Biome Options")] 
    public string biomeName;
    public int offset;
    public float scale;
    
    public int terrainHeight;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;
    
    [Header("Major Flora")] 
    public float majorFloraZoneScale = 1.3f;
    public int majorFloraIndex;
    [Range(0,1)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0,1)]
    public float treePlacementThreshold = 0.8f;
    public bool placeMajorFlora = true;

    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    public Lode[] loads;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;

}
