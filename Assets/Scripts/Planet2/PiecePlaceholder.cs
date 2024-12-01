using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlaceholder : MonoBehaviour
{
    public Planet belongs_planet = null;
    public Piece piece = null;
    PiecePlaceholder[] adjacents;

    void Awake() {
        if (belongs_planet == null) {
            belongs_planet = transform.parent.GetComponent<Planet>();
        }
        if (piece != null) {
            piece.placeholder = this;
        }
        piece.Init();
    }

    public void SetPiece(PiecePlaceholder placeholder) {
        SetPiece(placeholder.piece);
    }

    public void SetPiece(Piece p) {
        piece = p;
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
    }
}
