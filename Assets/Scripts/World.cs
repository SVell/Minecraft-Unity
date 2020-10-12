using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blockTypes;
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

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
