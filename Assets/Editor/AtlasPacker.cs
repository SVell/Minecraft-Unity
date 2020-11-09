using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SVell.Editor.AtlasPacker {
	public class AtlasPacker : EditorWindow
	{
		private int blockSize = 16; // Block size in pixels
		private int atlasSizeInBlocks = 16; // Atlas size in blocks
		private int atlasSize;
		
		Object[] rawTextures = new Object[256];
		List<Texture2D> sortedTextures = new List<Texture2D>();
		private Texture2D atlas;
		
		[MenuItem("SellBro Minecraft Clone/Atlas Packer")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(AtlasPacker),false,"Atlas Packer");
		}

		private void OnGUI()
		{
			atlasSize = blockSize * atlasSizeInBlocks;
			
			GUILayout.Label("Minecraft Clone texture Atlas Packer", EditorStyles.boldLabel);

			blockSize = EditorGUILayout.IntField("Block Size", blockSize);
			atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size (in blocks)", atlasSizeInBlocks);

			GUILayout.Label(atlas);
			
			if (GUILayout.Button("Load Textures"))
			{
				LoadTextures();
				PackAtlas();
				
				Debug.Log("Atlas Packer: Textures loaded");
			}

			if (GUILayout.Button("Clear Textures"))
			{
				atlas = new Texture2D(atlasSize,atlasSize);
				Debug.Log("Atlas Packer: Textures cleared");
			}

			if (GUILayout.Button("Save Atlas"))
			{
				byte[] bytes = atlas.EncodeToPNG();

				try
				{
					File.WriteAllBytes(Application.dataPath + "/Textures/Packed_Atlas.png",bytes);
				}
				catch(Exception e)
				{
					Debug.Log("Atlas Packer: Couldn't save atlas to file: " + e);
				}
			}
		}

		void LoadTextures()
		{
			sortedTextures.Clear();
			
			rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));
			
			int index = 0;
			foreach (Object tex in rawTextures)
			{
				Texture2D t = (Texture2D) tex;
				if (t.width == blockSize && t.height == blockSize)
				{
					sortedTextures.Add(t);
				}
				else
				{
					Debug.Log("Atlas Packer: " + tex.name + " incorrect size. Texture not loaded");
				}
					
				Debug.Log("Atlas Packer: " + sortedTextures.Count + " loaded");
				index++;
			}
		}

		void PackAtlas()
		{
			atlas = new Texture2D(atlasSize,atlasSize);
			Color[] pixels = new Color[atlasSize * atlasSize];

			for (int x = 0; x < atlasSize; ++x)
			{
				for (int y = 0; y < atlasSize; ++y)
				{
					// Get current block
					int currentBlockX = x / blockSize;
					int currentBlockY = y / blockSize;

					int index = currentBlockY * atlasSizeInBlocks + currentBlockX;
					
					// Get the pixel in the current block
					int currentPixelX = x - (currentBlockX * blockSize);
					int currentPixelY = y - (currentBlockY * blockSize);

					if (index < sortedTextures.Count)
					{
						pixels[(atlasSize - y - 1) * atlasSize + x] =
							sortedTextures[index].GetPixel(x, blockSize - y - 1);
					}
					else
					{
						pixels[(atlasSize - y - 1) * atlasSize + x] = new Color(0,0,0,0);
					}
				}
			}
			
			atlas.SetPixels(pixels);
			atlas.Apply();
		}
	}
}