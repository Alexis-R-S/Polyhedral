using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    protected bool isBuilt = false;
    protected Vector3[] vertices;
    public static GameObject base_piece = null;
    private static Color[] test_colors = null;

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

    public void RotateAround(Face face, float angle) {
        transform.RotateAround(face.transform.position, face.normal, angle);
    }

    public virtual void GetDirectionVectors(Face face, out Vector3[] vectors, out Face[] faces) {
        vectors = null;
        faces = null;
    }

    public virtual IEnumerable<Piece> listAdjacents() { yield break; }
    public virtual Vector3[] GetShapeVertices(Face face) { return null; }
    public virtual void SetVertices(Face face, int side) {}
    public virtual Sticker GetSticker(Face face) { return null; }

    public static T Create<T> (string name, Transform parent) where T : Piece {
        if (base_piece == null) {
            base_piece = Resources.Load<GameObject>("Piece");
            test_colors = new Color[6] {
                new Color(255, 128, 128),
                new Color(255, 255, 128),
                new Color(128, 255, 128),
                new Color(128, 255, 255),
                new Color(128, 128, 255),
                new Color(255, 128, 255)
            };
        }
        GameObject piece_obj = Instantiate(base_piece, parent);
        piece_obj.name = name;
        piece_obj.GetComponent<Renderer>().materials = new [] { new Material(Resources.Load<Material>("test materials/test material "+Random.Range(1, 7)))};
        return piece_obj.AddComponent<T>();
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
