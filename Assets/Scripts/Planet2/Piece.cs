using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Piece : MonoBehaviour
{
    public PiecePlaceholder placeholder;
    [SerializeField] string pieceType;
    Biome biome;
    PieceMeshDeformer meshDeformer;
    MeshFilter mf;

    void Awake() {
        mf = GetComponent<MeshFilter>();
    }

    // Initialize this piece
    public void Init() {
        mf.mesh = FileLoader.LoadBaseMesh(placeholder.belongs_planet.Type, pieceType);
        FileLoader.LoadMeshInfo(placeholder.belongs_planet.Type, pieceType, out MeshInfo meshInfo);
        meshDeformer = new PieceMeshDeformer(meshInfo);
        if (heights.Length + cornersHeights.Length > 0)    {    // Both are initialized
            DeformMesh();
        }

        if (gameObject.CompareTag("Test ray caster")) {
            Mesh m = FileLoader.LoadBaseMesh(placeholder.belongs_planet.Type, pieceType);
            Vector3[] verts = m.vertices;
            for (int i=0; i<verts.Length; i++) {
                Debug.DrawRay(transform.position+verts[i], Vector3.up, Color.red, 10000);
                if (Physics.Raycast(transform.position+verts[i], Vector3.up, out RaycastHit hit))
                    if (hit.collider.CompareTag("Index detector")) {
                        Debug.Log("Index: " + i);
                    }
            }
        }
    }

    [SerializeField] int[] heights = null;
    [SerializeField] int[] cornersHeights = null;
    void DeformMesh() {
        meshDeformer.Deform(mf.mesh, heights, cornersHeights);
    }
}
