using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerPiece : Piece
{
    // protected static Vector2[] projection_vectors = new[] {
    //     new Vector2()
    // };
    [SerializeField] private OuterEdgePiece adjacent_rear;
    [SerializeField] private OuterEdgePiece adjacent_right;
    [SerializeField] private OuterEdgePiece adjacent_left;
    [SerializeField] private int face_pov_id;
    public Sticker sticker_front;
    public Sticker sticker_right;
    public Sticker sticker_left;

    private Vector3[] shape_vertices_front;
    private Vector3[] shape_vertices_right;
    private Vector3[] shape_vertices_left;


    public void Build(OuterEdgePiece rear, OuterEdgePiece right, OuterEdgePiece left, Face face_pov, Face face_right, Face face_left) {
        if (!isBuilt) {
            isBuilt = true;
            adjacent_rear = rear;
            adjacent_right = right;
            adjacent_left = left;
            face_pov_id = face_pov.id;

            sticker_front = Sticker.Create(face_pov, this);
            sticker_right = Sticker.Create(face_right, this);
            sticker_left = Sticker.Create(face_left, this);
        }
    }

    public override IEnumerable<Piece> listAdjacents() {
        yield return adjacent_rear;
        yield return adjacent_right;
        yield return adjacent_left;
    }


    public OuterEdgePiece GetAdjacentOppositeTo(Face f) {
        return GetAdjacentOppositeTo(f.id);
    }


    public OuterEdgePiece GetAdjacentOppositeTo(int face_id) {
        if (face_id == sticker_front.belongs_face.id) {
            return adjacent_rear;
        }
        if (face_id == sticker_right.belongs_face.id) {
            return adjacent_left;
        }
        if (face_id == sticker_left.belongs_face.id) {
            return adjacent_right;
        }
        Debug.LogError("Given face ID is unknown to this piece.");
        return null;
    }

    public override Sticker GetSticker(Face face) {
        if (face.id == sticker_front.belongs_face.id) {
            return sticker_front;
        }
        if (face.id == sticker_left.belongs_face.id) {
            return sticker_left;
        }
        if (face.id == sticker_right.belongs_face.id) {
            return sticker_right;
        }
        return null;
    }

    public override Vector3[] GetShapeVertices(Face face) {
        if (face.id == sticker_front.belongs_face.id) {
            return shape_vertices_front;
        }
        if (face.id == sticker_right.belongs_face.id) {
            return shape_vertices_right;
        }
        if (face.id == sticker_left.belongs_face.id) {
            return shape_vertices_left;
        }
        return null;
    }

    public override void SetVertices(Face face, int side) {
        // Prevent from defining vertices multiple times
        if (shape_vertices_front != null) {
            return;
        }

        // Fetch needed information
        Face face_right = face.adjacent_faces[side];
        int side_right = face_right.getIndexOf(face);
        Face face_left = face.adjacent_faces[Face.toBounds(side-1)];
        int side_left = face_left.getIndexOf(face);
        float inneredge_width = Planet.edgeSize -2*(1 +Mathf.Tan(54*Mathf.Deg2Rad)/Mathf.Tan(72*Mathf.Deg2Rad));
        float outeredge_width = (Planet.edgeSize - inneredge_width)/2;
        float corner_width_coef = (outeredge_width/2)/(2*outeredge_width+inneredge_width);

        // Calculate front sticker extreme vertices
        shape_vertices_front = new Vector3[] {
            face.vertices[side],
            face.vertices[side] + (face.vertices[Face.toBounds(side-1)]-face.vertices[side])*corner_width_coef,
            (face.transform.localPosition + 2*face.vertices[side]) /3,
            face.vertices[side] + (face.vertices[Face.toBounds(side+1)]-face.vertices[side])*corner_width_coef
        };

        // Calculate right sticker extreme vertices
        shape_vertices_right = new Vector3[] {
            shape_vertices_front[0],
            shape_vertices_front[3],
            (face_right.transform.localPosition + 2*face_right.vertices[Face.toBounds(side_right+1)]) /3,
            face.vertices[side] + (face_right.vertices[Face.toBounds(side_right+2)]-face.vertices[side])*corner_width_coef
        };

        // Calculate left sticker extreme vertices
        shape_vertices_left = new Vector3[] {
            shape_vertices_front[0],
            shape_vertices_right[3],
            (face_left.transform.localPosition + 2*face.vertices[side]) /3,
            shape_vertices_front[1]
        };

        // Prepare projection
        sticker_front.SetProjections(
            new [] {
                (face.vertices[side] - face.vertices[Face.toBounds(side+1)]).normalized,
                (face.vertices[Face.toBounds(side-1)] - face.vertices[side]).normalized
            },
            new [] {
                face_right,
                face_left
            },
            new [] {
                false,
                false
            }
        );
        sticker_right.SetProjections(
            new [] {
                (face.vertices[Face.toBounds(side+1)] - face.vertices[side]).normalized,
                (face.vertices[side] - face_right.vertices[Face.toBounds(side_right+2)]).normalized
            },
            new [] {
                face,
                face_left
            },
            new [] {
                false,
                false
            }
        );
        sticker_left.SetProjections(
            new [] {
                (face.vertices[side] - face.vertices[Face.toBounds(side-1)]).normalized,
                (face_left.vertices[Face.toBounds(side_left-1)] - face.vertices[side]).normalized
            },
            new [] {
                face,
                face_right
            },
            new [] {
                false,
                false
            }
        );

        // Create mesh
        vertices = new Vector3[] {
            shape_vertices_front[0],
            shape_vertices_front[2],
            shape_vertices_front[3],
            shape_vertices_right[2],
            shape_vertices_right[3],
            shape_vertices_left[2],
            shape_vertices_front[1]
        };

        transform.localPosition = (vertices[0] +vertices[1] +vertices[2] +vertices[3] +vertices[4] +vertices[5] +vertices[6]) /7;

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
            vertices[3]-transform.localPosition,
            vertices[4]-transform.localPosition,
            vertices[5]-transform.localPosition,
            vertices[6]-transform.localPosition
        };
        int[] newTriangles = new [] {
            6, 0, 1,
            0, 2, 1,
            2, 0, 3,
            0, 4, 3,
            4, 0, 5,
            0, 6, 5
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
