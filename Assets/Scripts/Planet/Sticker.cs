using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour
{
    public Face belongs_face;
    public Piece belongs_piece;

    private Vector3[] projection_vectors;
    private Face[] projection_faces;
    private bool[] deep_rotations;

    // public Biome biome;

    public static Sticker Create(Face belongs_face, Piece belongs_piece) {
        GameObject sticker_obj = new GameObject("Sticker", typeof(Sticker));
        sticker_obj.transform.SetParent(belongs_piece.transform);
        Sticker sticker = sticker_obj.GetComponent<Sticker>();
        sticker.belongs_face = belongs_face;
        sticker.belongs_piece = belongs_piece;
        return sticker;
    }

    public void SetProjections(Vector3[] projection_vectors, Face[] projection_faces, bool[] deep_rotations) {
        if (projection_vectors.Length != projection_faces.Length || projection_vectors.Length != deep_rotations.Length) {
            Debug.LogError("Projections arrays should have the same size");
        }
        this.projection_vectors = projection_vectors;
        this.projection_faces = projection_faces;
        this.deep_rotations = deep_rotations;
    }

    public void GetProjections(out Vector3[] projection_vectors, out Face[] projection_faces, out bool[] deep_rotations) {
        projection_vectors = this.projection_vectors;
        projection_faces = this.projection_faces;
        deep_rotations = this.deep_rotations;
    }
}