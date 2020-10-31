using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree(Vector3 position, Queue<VoxelMod> queue, int minTrinkHeight, int maxTrunkHeight)
    {
        int height = (int) (maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.y), 666, 3f));
        if (height < minTrinkHeight)
            height = minTrinkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + i,position.z),6));
        }
        
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height + 1,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x + 1,position.y + height,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x + 1,position.y + height + 1,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x+1,position.y + height,position.z+1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x+1,position.y + height + 1,position.z+1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height + 1,position.z),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height + 1,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height + 1,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height,position.z+1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x,position.y + height + 1,position.z+1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x+1,position.y + height,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x+1,position.y + height + 1,position.z-1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height,position.z+1),11));
        queue.Enqueue(new VoxelMod(new Vector3(position.x-1,position.y + height + 1,position.z+1),11));

        for (int x = -2; x < 3; x++)
        {
            for (int y = -3; y < 0; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    if(x == 0 && z == 0) continue;
                    queue.Enqueue(new VoxelMod(new Vector3(position.x+x,position.y + height + y,position.z+z),11));
                }
            }
        }
    }
}
