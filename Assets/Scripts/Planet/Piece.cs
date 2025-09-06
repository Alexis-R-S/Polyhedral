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
    public float perlinSeed = 5000;

    [SerializeField] int groundHeight = 1;

    // Initialize this piece
    public void Init()
    {
        biome = new Biome();
        perlinSeed = Random.Range(10000f, 10000000f);

        mf = GetComponent<MeshFilter>();
        FileLoader.LoadMeshInfo(placeholder.belongs_planet.Type, pieceType, out MeshInfo meshInfo);
        meshDeformer = new PieceMeshDeformer(meshInfo);

        DeformMesh();

        // Debug code to find vertex index
        if (gameObject.CompareTag("Test ray caster"))
        {
            Mesh m = FileLoader.LoadBaseMesh(placeholder.belongs_planet.Type, pieceType);
            Vector3[] verts = m.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Debug.DrawRay(transform.TransformPoint(transform.localPosition + verts[i]), transform.rotation * Vector3.up, Color.red, 10000);
                if (Physics.Raycast(transform.TransformPoint(transform.localPosition + verts[i]), transform.rotation * Vector3.up, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Index detector"))
                    {
                        Debug.Log("Index: " + i);
                    }
                }
            }
        }
    }

    public void DeformMesh()
    {
        mf.mesh = FileLoader.LoadBaseMesh(placeholder.belongs_planet.Type, pieceType);
        PerlinOpt perlinOpt;
        FileLoader.LoadPerlinOpt(placeholder.belongs_planet.Type, pieceType, out perlinOpt);
        meshDeformer.Deform(mf.mesh, groundHeight, placeholder.belongs_planet.perlinSeed + perlinSeed, perlinOpt);
    }
}
