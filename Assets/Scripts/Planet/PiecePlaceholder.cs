using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlaceholder : MonoBehaviour
{
    public Planet belongs_planet = null;    // The planet this placeholder belongs to
    public Piece piece = null;       // The piece currently in this placeholder
    PiecePlaceholder[] adjacents;   // Adjacent placeholders


    void Awake() {
        if (belongs_planet == null)
        {
            belongs_planet = transform.parent.GetComponent<Planet>();
        }
        if (piece != null) {
            piece.placeholder = this;
        }
        piece.Init();
    }

    // Get the piece from another placeholder and set it to this one
    public void SetPiece(PiecePlaceholder placeholder)
    {
        SetPiece(placeholder.piece);
    }

    // Set the piece to this placeholder
    public void SetPiece(Piece p)
    {
        piece = p;
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
    }
}
