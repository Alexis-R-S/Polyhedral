using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileLoader
{
    // Load a base mesh from Resources/Mesh/planetType/pieceType
    public static Mesh LoadBaseMesh(string planetType, string pieceType)
    {
        return Resources.Load<Mesh>("Mesh/" + planetType + "/" + pieceType);
    }

    // Load a MeshInfo from Resources/Mesh info/planetType/pieceType
    public static void LoadMeshInfo(string planetType, string pieceType, out MeshInfo meshInfo)
    {
        string path = "Mesh info/" + planetType + "/" + pieceType;
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("File not found at: " + path);
            meshInfo = null;
            return;
        }
        string meshInfoData = textAsset.text;
        meshInfo = JsonUtility.FromJson<MeshInfo>(meshInfoData);
    }

    // Load a PerlinOpt from Resources/Perlin options/planetType/pieceType
    public static void LoadPerlinOpt(string planetType, string pieceType, out PerlinOpt perlinOpt)
    {
        string path = "Perlin options/" + planetType + "/" + pieceType;
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("File not found at: " + path);
            perlinOpt = null;
            return;
        }
        string perlinOptData = textAsset.text;
        perlinOpt = JsonUtility.FromJson<PerlinOpt>(perlinOptData);
    }
    
    // TODO: cache loaded data to avoid loading multiple times the same file
}