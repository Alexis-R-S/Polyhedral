using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Planet : MonoBehaviour
{
    [SerializeField] private Face[] faces;
    Camera cam;
    [SerializeField] private float camSpeed = 1.0f;
    [SerializeField] private float minScreenMotionToRotate = 1.0f;
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private float minRotationToConfirm = 36;     // Degrees

    [SerializeField] private InputAction pressAction, screenPosAction;
    private Vector3 curScreenPos;
    private bool isDragging;
    

    public static readonly float goldenRatio = (1+Mathf.Sqrt(5))/2;
    public static readonly float edgeSize = 3;

    private static int _unique_face_id = 0;
    public static int GetUniqueFaceID {
        get {
            _unique_face_id += 1;
            return _unique_face_id;
        }
    }

    void Awake() {
        cam = Camera.main;
        cam.transform.LookAt(transform.position);
        screenPosAction.Enable();
        pressAction.Enable();
        screenPosAction.performed += context => { curScreenPos = context.ReadValue<Vector2>(); };
        pressAction.performed += _ => { StartCoroutine(Drag()); };
        pressAction.canceled += _ => { isDragging = false; };
    }

    private IEnumerator Drag() {
        if (isDragging) {
            yield break;
        }
        isDragging = true;
        Vector3 frameScreenPos = curScreenPos;
        Vector3 startScreenPos = frameScreenPos;
        Vector3 lastScreenPos = frameScreenPos;
        Ray ray = cam.ScreenPointToRay(frameScreenPos);

        // Drag
        Sticker hit = null;
        for (int i=0; i<12; i++) {
            hit = faces[i].GetHitSticker(ray);
            if (hit != null) {
                break;
            }
        }

        if (hit == null || hit.belongs_piece.GetType() == typeof(CenterPiece)) {
            // Rotate camera around planet

            Vector3 rotationAxis;
            while (isDragging) { 
                // Dragging
                frameScreenPos = curScreenPos;
                if (frameScreenPos != lastScreenPos) {
                    rotationAxis = cam.transform.up*(frameScreenPos-lastScreenPos).x - cam.transform.right*(frameScreenPos-lastScreenPos).y;
                    cam.transform.RotateAround(transform.position, rotationAxis, rotationAxis.magnitude*camSpeed);
                    lastScreenPos = frameScreenPos;
                }
                yield return null;
            }
        }
        else {
            // Rotate a face

            // Get motion vectors, projected on screen
            hit.GetProjections(out Vector3[] proj_vects, out Face[] proj_faces, out bool[] proj_deepness);
            Vector3 centerPlanetOnScreen = cam.WorldToScreenPoint(transform.position);
            Vector3[] proj_dirs = new Vector3[proj_vects.Length];
            for (int i=0; i<proj_vects.Length; i++) {
                proj_dirs[i] = cam.WorldToScreenPoint(transform.position + proj_vects[i]) - centerPlanetOnScreen;
            }

            Vector3 finalProjDir = Vector3.zero;
            Face finalProjFace = null;
            bool finalDeepness = false;
            Vector3 screenPosMotion;
            float rot_angle;

            while (isDragging) {
                // Dragging
                frameScreenPos = curScreenPos;
                if (frameScreenPos != lastScreenPos) {
                    screenPosMotion = frameScreenPos - startScreenPos;
                    // If final direction is not defined yet
                    if (finalProjDir == Vector3.zero) {
                        // If current screen position is far enough from the start position
                        if (screenPosMotion.magnitude > minScreenMotionToRotate) {
                            // Find closest motion vector (compare directions)
                            int best_index = -1;
                            float best_score = -1;
                            float current_score = -1;
                            for (int i=0; i<proj_dirs.Length; i++) {
                                current_score = Mathf.Abs( Vector3.Dot(proj_dirs[i].normalized, screenPosMotion) );
                                if (current_score > best_score) {
                                    best_score = current_score;
                                    best_index = i;
                                }
                            }
                            finalProjDir = proj_dirs[best_index];
                            finalProjFace = proj_faces[best_index];
                            finalDeepness = proj_deepness[best_index];
                        }
                    }
                    
                    // If final direction is already defined
                    else {
                        rot_angle = Vector3.Dot(finalProjDir, screenPosMotion)*rotationSpeed;
                        rot_angle = Mathf.Max(-1, Mathf.Min(1, rot_angle))*72;
                        finalProjFace.SetRotation(finalDeepness, rot_angle);
                    }
                    lastScreenPos = frameScreenPos;
                }
                yield return null;
            }

            // Drop
            if (finalProjFace != null) {
                // finalProjFace.ConfirmRotation(finalDeepness, minRotationToConfirm);
                finalProjFace.SetRotation(finalDeepness, 0);
            }
        }

        isDragging = false;
    }


    void Build() {
        faces = new Face[12];

        // I'm sorry about this part readability.
        // See https://en.wikipedia.org/wiki/Regular_dodecahedron to know more about those coordinates
        
        // Calculate vertexes coordinates
        Vector3[] verts = new Vector3[20] {
            new Vector3(1, 1, 1),       // W-Green-Pu
            new Vector3(1, 1, -1),      // W-R-B
            new Vector3(1, -1, 1),      // Green-Ly-C
            new Vector3(1, -1, -1),     // R-Ly-Pi
            new Vector3(-1, -1, 1),     // O-C-Grey
            new Vector3(-1, -1, -1),    // L-Pi-Grey
            new Vector3(-1, 1, 1),      // Y-Pu-O
            new Vector3(-1, 1, -1),     // Y-B-L
            new Vector3(0, -goldenRatio, 1/goldenRatio),    // C-Ly-Grey
            new Vector3(0, -goldenRatio, -1/goldenRatio),   // Pi-Ly-Grey
            new Vector3(0, goldenRatio, 1/goldenRatio),     // Pu-W-Y
            new Vector3(0, goldenRatio, -1/goldenRatio),    // B-W-Y
            new Vector3(1/goldenRatio, 0, goldenRatio),     // Pu-C-Green
            new Vector3(-1/goldenRatio, 0, goldenRatio),    // Pu-C-O
            new Vector3(1/goldenRatio, 0, -goldenRatio),    // Pi-B-R
            new Vector3(-1/goldenRatio, 0, -goldenRatio),   // Pi-B-L
            new Vector3(-goldenRatio, 1/goldenRatio, 0),    // O-L-Y
            new Vector3(-goldenRatio, -1/goldenRatio, 0),   // O-L-Grey
            new Vector3(goldenRatio, 1/goldenRatio, 0),     // Green-R-W
            new Vector3(goldenRatio, -1/goldenRatio, 0)     // Green-R-Ly
        };

        // Above coordinates correspond to edge size 2/goldenRatio. We are working we edge size 3.
        float sizeRatio = 3*goldenRatio/2;
        for (int i=0; i<20; i++) {
            verts[i] *= sizeRatio;
        }

        // Setup faces (vertexes are given in the same order as adjacent faces)
        faces[0] = Face.Create( new Vector3(1, goldenRatio, 0), new [] {verts[11], verts[10], verts[0], verts[18], verts[1]}, this);
        faces[1] = Face.Create( new Vector3(-1, goldenRatio, 0), new [] {verts[10], verts[11], verts[7], verts[16], verts[6]}, this);
        faces[2] = Face.Create( new Vector3(0, 1, goldenRatio), new [] {verts[0], verts[10], verts[6], verts[13], verts[12]}, this);
        faces[3] = Face.Create( new Vector3(goldenRatio, 0, 1), new [] {verts[18], verts[0], verts[12], verts[2], verts[19]}, this);
        faces[4] = Face.Create( new Vector3(goldenRatio, 0, -1), new [] {verts[1], verts[18], verts[19], verts[3], verts[14]}, this);
        faces[5] = Face.Create( new Vector3(0, 1, -goldenRatio), new [] {verts[11], verts[1], verts[14], verts[15], verts[7]}, this);
        faces[6] = Face.Create( new Vector3(-goldenRatio, 0, -1), new [] {verts[16], verts[7], verts[15], verts[5], verts[17]}, this);
        faces[7] = Face.Create( new Vector3(-goldenRatio, 0, 1), new [] {verts[13], verts[6], verts[16], verts[17], verts[4]}, this);
        faces[8] = Face.Create( new Vector3(0, -1, goldenRatio), new [] {verts[2], verts[12], verts[13], verts[4], verts[8]}, this);
        faces[9] = Face.Create( new Vector3(1, -goldenRatio, 0), new [] {verts[3], verts[19], verts[2], verts[8], verts[9]}, this);
        faces[10] = Face.Create( new Vector3(0, -1, -goldenRatio), new [] {verts[15], verts[14], verts[3], verts[9], verts[5]}, this);
        faces[11] = Face.Create( new Vector3(-1, -goldenRatio), new [] {verts[17], verts[5], verts[9], verts[8], verts[4]}, this);

        // Build faces, using known relations between faces (those of a dodecahedron)
        faces[0].adjacent_faces = new [] {faces[1], faces[2], faces[3], faces[4], faces[5]};   // W
        faces[1].adjacent_faces = new [] {faces[0], faces[5], faces[6], faces[7], faces[2]};   // Y
        faces[2].adjacent_faces = new [] {faces[0], faces[1], faces[7], faces[8], faces[3]};   // Pu
        faces[3].adjacent_faces = new [] {faces[0], faces[2], faces[8], faces[9], faces[4]};   // Green
        faces[4].adjacent_faces = new [] {faces[0], faces[3], faces[9], faces[10], faces[5]};  // R
        faces[5].adjacent_faces = new [] {faces[0], faces[4], faces[10], faces[6], faces[1]};  // B
        faces[6].adjacent_faces = new [] {faces[1], faces[5], faces[10], faces[11], faces[7]}; // L
        faces[7].adjacent_faces = new [] {faces[2], faces[1], faces[6], faces[11], faces[8]};  // O
        faces[8].adjacent_faces = new [] {faces[3], faces[2], faces[7], faces[11], faces[9]};  // C
        faces[9].adjacent_faces = new [] {faces[4], faces[3], faces[8], faces[11], faces[10]}; // Ly
        faces[10].adjacent_faces = new [] {faces[5], faces[4], faces[9], faces[11], faces[6]}; // Pi
        faces[11].adjacent_faces = new [] {faces[6], faces[10], faces[9], faces[8], faces[7]}; // Grey

        for(int i=0; i<12; i++) {
            faces[i].Build();
        }
        for (int i=0; i<12; i++) {
            faces[i].BuildPieces();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Build();
    }
}
