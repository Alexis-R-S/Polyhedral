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

    [SerializeField] int groundHeight = 1;

    [Header("Perlin noise")]
    [SerializeField] float perlinSeed = 5000;
    [SerializeField] float perlinScale = 4;
    [SerializeField] int perlinDetail = 10;

    [Header("Shape attenuation")]
    [SerializeField] int attenuationHeight = 2;
    [SerializeField] float attenuationMax = 1.5f;
    [SerializeField] float attenuationPower = 4;
    [SerializeField] float attenuationWidth = 3;

    // Initialize this piece
    public void Init()
    {
        biome = new Biome();

        mf = GetComponent<MeshFilter>();
        FileLoader.LoadMeshInfo(placeholder.belongs_planet.Type, pieceType, out MeshInfo meshInfo);
        meshDeformer = new PieceMeshDeformer(meshInfo);

        DeformMesh();

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
