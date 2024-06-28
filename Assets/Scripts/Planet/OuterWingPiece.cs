using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterWingPiece : Piece
{
    // protected override Vector2[] projection_vectors;
    [SerializeField] public InnerWingPiece adjacent_right_wing {get; private set;}
    [SerializeField] public InnerWingPiece adjacent_left_wing {get; private set;}
    [SerializeField] public OuterEdgePiece adjacent_right_edge {get; private set;}
    [SerializeField] public OuterEdgePiece adjacent_left_edge {get; private set;}
    public Sticker sticker;

    private Vector3[] shape_vertices;

    public void Build(InnerWingPiece right_wing, InnerWingPiece left_wing, OuterEdgePiece right_edge, OuterEdgePiece left_edge, Face belongs_face) {
        if (!isBuilt) {
            isBuilt = true;
            adjacent_right_wing = right_wing;
            adjacent_left_wing = left_wing;
            adjacent_right_edge = right_edge;
            adjacent_left_edge = left_edge;
            sticker = Sticker.Create(belongs_face, this);
        }
    }

    public override IEnumerable<Piece> listAdjacents() {
        yield return adjacent_right_wing;
        yield return adjacent_left_wing;
        yield return adjacent_right_edge;
        yield return adjacent_left_edge;
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
        shape_vertices = new Vector3[4];
        
        // Fetch needed information
        Vector3 base_left = (face.transform.localPosition + 2*face.vertices[Face.toBounds(side-1)]) /3;
        Vector3 base_right = (face.transform.localPosition + 2*face.vertices[Face.toBounds(side+1)]) /3;
        float innerwing_width = 2*Planet.edgeSize/3 -1 -(Mathf.Tan(54*Mathf.Deg2Rad) / Mathf.Tan(72*Mathf.Deg2Rad));
        float outerwing_width = Planet.edgeSize/3 - innerwing_width/2;
        
        // Calculate sticker extreme vertices
        shape_vertices[0] = (2*face.transform.localPosition + face.vertices[side]) /3;
        shape_vertices[2] = (face.transform.localPosition + 2*face.vertices[side]) /3;
        shape_vertices[1] = shape_vertices[2] + (base_left-shape_vertices[2])*outerwing_width/(2*outerwing_width+innerwing_width);
        shape_vertices[3] = shape_vertices[2] + (base_right-shape_vertices[2])*outerwing_width/(2*outerwing_width+innerwing_width);
        transform.localPosition = (shape_vertices[0] +shape_vertices[1] +shape_vertices[2] +shape_vertices[3]) /4;

        // Prepare projection
        sticker.SetProjections(
            new [] {
                (face.vertices[side] - face.vertices[Face.toBounds(side+1)]).normalized,
                (face.vertices[Face.toBounds(side-1)] - face.vertices[side]).normalized
            },
            new [] {
                face.adjacent_faces[side],
                face.adjacent_faces[Face.toBounds(side-1)]
            },
            new [] {
                true,
                true
            }
        );

        // Create mesh
        vertices = shape_vertices;

        MeshFilter mesh_f = gameObject.GetComponent<MeshFilter>();
        Mesh m = CreateMesh();
        mesh_f.mesh = m;
    }

    private Mesh CreateMesh() {
        Mesh m = new Mesh();

        Vector3[] newVertices = new [] {
            vertices[0]-transform.localPosition,
            vertices[1]-transform.localPosition,
            vertices[2]-transform.localPosition,
            vertices[3]-transform.localPosition
        };
        int[] newTriangles = new [] {
            0, 1, 2,
            2, 3, 0
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
