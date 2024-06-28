using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterPiece : Piece
{
    // protected override Vector2[] projection_vectors;
    [SerializeField] public InnerWingPiece[] adjacents {get; private set;}    // Size 5
    public Sticker sticker;

    private Vector3[] shape_vertices;

    public void Build(InnerWingPiece[] adjacents, Face belongs_face) {
        if (!isBuilt) {
            isBuilt = true;
            this.adjacents = adjacents;
            
            sticker = Sticker.Create(belongs_face, this);
        }
    }

    public override IEnumerable<Piece> listAdjacents() {
        for (int i=0; i<5; i++) {
            yield return adjacents[i];
        }
    }

    public override Sticker GetSticker(Face face) {
        return face.id == sticker.belongs_face.id ? sticker : null;
    }

    public override Vector3[] GetShapeVertices(Face face) {
        if (face.id != sticker.belongs_face.id) {
            return null;
        }
        return shape_vertices;
    }

    public override void SetVertices(Face face, int side) {
        shape_vertices = new Vector3[5];
        for (int i=0; i<5; i++) {
            shape_vertices[i] = (2*face.transform.position + face.vertices[i]) /3;
        }

        vertices = new Vector3[5];
        for(int i=0; i<5; i++) {
            vertices[i] = (2*face.transform.localPosition + face.vertices[i]) /3;
        }
        transform.localPosition = (vertices[0] +vertices[1] +vertices[2] +vertices[3] +vertices[4]) /5;

        MeshFilter mesh_f = gameObject.GetComponent<MeshFilter>();
        Mesh m = CreateMesh();
        mesh_f.mesh = m;
    }

    private Mesh CreateMesh() {
        Mesh m = new Mesh();

        Vector3[] newVertices = new [] {
            Vector3.zero,
            vertices[0]-transform.localPosition,
            vertices[1]-transform.localPosition,
            vertices[2]-transform.localPosition,
            vertices[3]-transform.localPosition,
            vertices[4]-transform.localPosition,
        };
        int[] newTriangles = new [] {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 1
        };
        m.vertices = newVertices;
        m.triangles = newTriangles;

        return m;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}