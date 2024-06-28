using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    [SerializeField] private CenterPiece center;
    [SerializeField] private InnerWingPiece[] inner_wings;          // Size 5
    [SerializeField] private OuterWingPiece[] outer_wings;          // Size 5
    [SerializeField] private InnerEdgePiece[] inner_edges;          // Size 5
    [SerializeField] private OuterEdgePiece[] outer_edges_right;    // Size 5
    [SerializeField] private OuterEdgePiece[] outer_edges_left;     // Size 5
    [SerializeField] private CornerPiece[] corners;                 // Size 5

    [SerializeField] public Face[] adjacent_faces;                 // Size 5
    [SerializeField] public Planet belongs_planet;


    public int id;

    public Vector3[] vertices;      // Size 5
    private Vector3[] intern_vertices;  // Intern pentagon of the face (center+wings). Size 5
    public Vector3 normal;
    Vector3 plane_origin;
    Plane face_plane;
    Vector3 plane_axis_X;
    Vector3 plane_axis_Y;

    float current_rotation = 0.0f;


    public static int toBounds(int n) {
        return (n%5 + 5)%5;
    }


    public static Face Create(Vector3 normal, Vector3[] verts, Planet belongs_planet) {
        if (verts.Length != 5) {
            Debug.LogError("Vertices length should be 5.");
            return null;
        }
        int id = Planet.GetUniqueFaceID;
        Face f = new GameObject("Face "+id, typeof(Face)).GetComponent<Face>();
        f.transform.SetParent(belongs_planet.transform);
        f.belongs_planet = belongs_planet;
        f.id = id;
        f.vertices = verts;
        f.intern_vertices = new Vector3[5];
        f.transform.localPosition = (verts[0] + verts[1] + verts[2] + verts[3] + verts[4]) /5;
        for (int i=0; i<5; i++) {
            f.intern_vertices[i] = (f.transform.localPosition + 2*verts[i]) /3;
        }

        for (int i=0; i<5; i++) {
            Debug.DrawLine(verts[i], verts[toBounds(i+1)], Color.blue, 180);
        }

        // Calculate face plane
        f.normal = normal;
        f.plane_origin = f.transform.position;
        f.face_plane = new Plane(normal, f.plane_origin);
        f.plane_axis_X = verts[0] - f.plane_origin;
        f.plane_axis_Y = verts[1] - f.plane_origin;
        return f;
    }


    // Sort of second constructor
    public void Build() {
        Face[] neighbors = adjacent_faces;  // Alias

        // Create / assign 
        center = Piece.Create<CenterPiece>("Center "+id, transform.parent);

        inner_wings = new InnerWingPiece[5];
        outer_wings = new OuterWingPiece[5];
        inner_edges = new InnerEdgePiece[5];
        outer_edges_right = new OuterEdgePiece[5];
        outer_edges_left = new OuterEdgePiece[5];
        corners = new CornerPiece[5];
        for (int i=0; i<5; i++) {
            Face neighbor = neighbors[i];

            inner_wings[i] = Piece.Create<InnerWingPiece>("InnerWing "+id+" "+neighbor.id, transform.parent);
            outer_wings[i] = Piece.Create<OuterWingPiece>("OuterWing "+id+" "+neighbor.id, transform.parent);

            if (neighbor.inner_edges == null) {
                inner_edges[i] = Piece.Create<InnerEdgePiece>("InnerEdge "+id+" "+neighbor.id, transform.parent);
            }
            else {
                inner_edges[i] = neighbor.inner_edges[neighbor.getIndexOf(this)];
            }

            if (neighbor.outer_edges_right == null) {
                outer_edges_left[i] = Piece.Create<OuterEdgePiece>("OuterEdge "+id+" "+neighbor.id, transform.parent);
            }
            else {
                outer_edges_left[i] = neighbor.outer_edges_right[neighbor.getIndexOf(this)];
            }

            if (neighbor.outer_edges_left == null) {
                outer_edges_right[i] = Piece.Create<OuterEdgePiece>("OuterEdge "+neighbor.id+" "+id, transform.parent);
            }
            else {
                outer_edges_right[i] = neighbor.outer_edges_left[neighbor.getIndexOf(this)];
            }

            if (neighbor.corners == null) {
                if (neighbors[toBounds(i-1)].corners == null) {
                    corners[i] = Piece.Create<CornerPiece>("Corner "+id+" "+neighbor.id+" "+neighbors[toBounds(i-1)].id, transform.parent);
                }
                else {
                    corners[i] = neighbors[toBounds(i-1)].corners[neighbors[toBounds(i-1)].getIndexOf(this)];
                }
            }
            else {
                corners[i] = neighbor.corners[toBounds(neighbor.getIndexOf(this)+1)];
            }
        }
    }
    
    // Sort of third constructor. Should be called after build, when all faces are built.
    public void BuildPieces() {
        center.Build(inner_wings, this);
        for (int i=0; i<5; i++) {
            inner_wings[i].Build(
                center,
                outer_wings[toBounds(i+1)],
                outer_wings[i],
                inner_edges[i],
                this
            );
            outer_wings[i].Build(
                inner_wings[i],
                inner_wings[toBounds(i-1)],
                outer_edges_left[i],
                outer_edges_right[toBounds(i-1)],
                this
            );
            inner_edges[i].Build(
                inner_wings[i],
                adjacent_faces[i].inner_wings[adjacent_faces[i].getIndexOf(this)],
                outer_edges_right[i],
                outer_edges_left[i],
                this,
                adjacent_faces[i]
            );
            outer_edges_right[i].Build(
                adjacent_faces[i].outer_wings[adjacent_faces[i].getIndexOf(this)],
                outer_wings[toBounds(i+1)],
                corners[toBounds(i+1)],
                inner_edges[i],
                adjacent_faces[i],
                this
            );
            outer_edges_left[i].Build(
                outer_wings[i],
                adjacent_faces[i].outer_wings[toBounds(adjacent_faces[i].getIndexOf(this)+1)],
                corners[i],
                inner_edges[i],
                this,
                adjacent_faces[i]
            );
            corners[i].Build(
                adjacent_faces[i].outer_edges_left[ toBounds(adjacent_faces[i].getIndexOf(this)+1) ],
                outer_edges_left[i],
                outer_edges_right[toBounds(i-1)],
                this,
                adjacent_faces[i],
                adjacent_faces[toBounds(i-1)]
            );
        }

        center.SetVertices(this, 0);
        for (int i=0; i<5; i++) {
            inner_wings[i].SetVertices(this, i);
            outer_wings[i].SetVertices(this, i);
            inner_edges[i].SetVertices(this, i);
            outer_edges_left[i].SetVertices(this, i);
            corners[i].SetVertices(this, i);
        }
    }

    public int getIndexOf(Face face) {
        for (int i=0; i<5; i++) {
            if (adjacent_faces[i].id == face.id) {
                return i;
            }
        }
        return -1;
    }

    private IEnumerable<Piece> enumSurfacePieces() {
        yield return center;
        for (int i=0; i<5; i++) {
            yield return inner_wings[i];
            yield return outer_wings[i];
            yield return inner_edges[i];
            yield return outer_edges_right[i];
            yield return outer_edges_left[i];
            yield return corners[i];
        }
    }

    private IEnumerable<Piece> enumDeepPieces() {
        for (int i=0; i<5; i++) {
            yield return inner_edges[i].GetAdjacentOppositeTo(id);
            yield return outer_edges_left[i].GetAdjacentOppositeTo(id);
            yield return outer_edges_right[i].GetAdjacentOppositeTo(id);
            yield return corners[i].GetAdjacentOppositeTo(id);
        }
    }

    private bool _is_glowing = false;
    public bool IsGlowing {
        get {
            return _is_glowing;
        }
        set {
            // TODO
            _is_glowing = value;
        }
    }

    // Set current rotation of this face
    // ConfirmRotation should be called on this before using SetRotation on another face
    public void SetRotation(bool deep_rotate, float angle) {
        float rot_angle = angle - current_rotation;
        foreach (Piece p in enumSurfacePieces()) {
            p.RotateAround(this, rot_angle);
        }
        
        if (deep_rotate) {
            foreach (Piece p in enumDeepPieces()) {
                p.RotateAround(this, rot_angle);
            }
        }

        current_rotation = angle;
    }

    public void ConfirmRotation(bool deep_rotate, float minRotToConfirm) {
        // Clockwise rotation
        if (current_rotation > minRotToConfirm) {
            SetRotation(deep_rotate, 72);
            current_rotation = 0;
            Debug.Log("OK +");
            return;
        }
        // Counter-clockwise rotation
        if (current_rotation < -minRotToConfirm) {
            SetRotation(deep_rotate, -72);
            current_rotation = 0;
            Debug.Log("OK -");

            return;
        }

        // Back to original rotation
        SetRotation(deep_rotate, 0);
        Debug.Log("Pas OK");
    }

    // Convert the 3-dimentional point to a 2-dimentional point on the face plane
    private Vector2 ToLocalPlane(Vector3 point) {
        return new Vector2(
            Vector3.Dot(point - plane_origin, plane_axis_X),
            Vector3.Dot(point - plane_origin, plane_axis_Y)
        );
    }

    // The returned bool represents the side of point on the line formed by line_point_A and line_point_B
    private bool GetPointSide(Vector2 point, Vector2 line_point_A, Vector2 line_point_B) {
        return point.x*(line_point_B.y - line_point_A.y) - point.y*(line_point_B.x - line_point_A.x) + line_point_A.y*line_point_B.x - line_point_A.x*line_point_B.y > 0;
    }

    // Returns true only if point is within the shape formed by the verts.
    // Verts shape must be concave
    private bool IsInsideShape(Vector3 point, Vector3[] verts) {
        if (verts.Length < 3) {
            return false;
        }
        // Project vertices on local plane
        Vector2 local_P = ToLocalPlane(point);
        Vector2[] local_verts = new Vector2[verts.Length];
        for (int i=0; i<verts.Length; i++) {
            local_verts[i] = ToLocalPlane(verts[i]);
        }

        // Return true only if point is at the same side of all lines
        bool point_side = GetPointSide(local_P, local_verts[0], local_verts[1]);
        for (int i=1; i<local_verts.Length-1; i++) {
            if (GetPointSide(local_P, local_verts[i], local_verts[i+1]) != point_side) {
                return false;
            }
        }
        return GetPointSide(local_P, local_verts[local_verts.Length-1], local_verts[0]) == point_side;
    }

    public Sticker GetHitSticker(Ray ray) {
        // Prevent rays from hitting from behind the surface
        if (Vector3.Dot(normal, ray.direction) >= 0) {
            return null;
        }

        // Get intersection point of ray and the face plane
        if (!face_plane.Raycast(ray, out float intersection_distance))
        {
            return null;    // If ray is parallel to plane
        }

        // If outside of face, no sticker is hit
        Vector3 intersection_point = ray.GetPoint(intersection_distance);
        if (! IsInsideShape(intersection_point, vertices)) {
            return null;
        }

        // Check if point is intern (center+wings) or extern (edges+corners)
        if (IsInsideShape(intersection_point, intern_vertices)) {
            // If point is intern (center+wings)
            if (IsInsideShape(intersection_point, center.GetShapeVertices(this))) {
                return center.GetSticker(this);
            }
            for (int i=0; i<5; i++) {
                if (IsInsideShape(intersection_point, inner_wings[i].GetShapeVertices(this))) {
                    return inner_wings[i].GetSticker(this);
                }
                if (IsInsideShape(intersection_point, outer_wings[i].GetShapeVertices(this))) {
                    return outer_wings[i].GetSticker(this);
                }
            }
        }
        else {
            // If point is extern (edges+corners)
            for (int i=0; i<5; i++) {
                if (IsInsideShape(intersection_point, inner_edges[i].GetShapeVertices(this))) {
                    return inner_edges[i].GetSticker(this);
                }
                if (IsInsideShape(intersection_point, outer_edges_left[i].GetShapeVertices(this))) {
                    return outer_edges_left[i].GetSticker(this);
                }
                if (IsInsideShape(intersection_point, outer_edges_right[i].GetShapeVertices(this))) {
                    return outer_edges_right[i].GetSticker(this);
                }
                if (IsInsideShape(intersection_point, corners[i].GetShapeVertices(this))) {
                    return corners[i].GetSticker(this);
                }
            }
        }
        Debug.Log("Fail");
        return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
