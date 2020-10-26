using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    private World world;
    private Text text;

    private float frameRate;
    private float timer;

    private int halfWorldSizeInChunks;
    private int halfWorldSizeInVoxels;
    private void Start()
    {
        text = GetComponent<Text>();
        world = GameObject.Find("World").GetComponent<World>();

        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
    }

    private void Update()
    {
        string debugText = "SellBro Minecraft Clone";
        debugText += "\n";
        debugText += frameRate + " FPS";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + "/" +
                     Mathf.FloorToInt(world.player.transform.position.y) + "/" +
                     (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + "/" + (world.playerChunkCoord.z - halfWorldSizeInChunks);

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int) (1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
