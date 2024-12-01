using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileLoader
{
    public static Mesh LoadBaseMesh(string planetType, string pieceType) {
        return Resources.Load<Mesh>("Mesh/" + planetType + "/" + pieceType);
    }

    public static void LoadMeshInfo(string planetType, string pieceType, out MeshInfo meshInfo) {
        string path = "Mesh info/" + planetType + "/" + pieceType;
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null) {
            Debug.LogError("File not found at: " + path);
            meshInfo = null;
            return;
        }
        string meshInfoData = textAsset.text;
        meshInfo = JsonUtility.FromJson<MeshInfo>(meshInfoData); 
    }
}