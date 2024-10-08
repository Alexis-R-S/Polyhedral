using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlaceholder : MonoBehaviour
{
    // public TouchDetector Detector {get; private set;}
    PiecePlaceholder[] adjacents;
    public Piece piece;

    public void SetPiece(PiecePlaceholder placeholder) {
        SetPiece(placeholder.piece);
    }

    public void SetPiece(Piece p) {
        piece = p;
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
    }
}
