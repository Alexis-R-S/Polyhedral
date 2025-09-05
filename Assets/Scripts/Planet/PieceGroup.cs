using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceGroup : MonoBehaviour
{
    // TODO: When rotation is confirmed, disable going in the opposite direction

    [System.Serializable]
    struct Placeholders {
        [SerializeField] public PiecePlaceholder[] placeholders;
    }

    [SerializeField] Planet belongs_planet; // The planet this group belongs to
    [SerializeField] Placeholders[] swaps;  
    [SerializeField] Placeholders[] cutPieces;  // Pieces that are cut off during rotation. Is this useful? TODO: check usefulness
    [SerializeField] float rotationAngle = 90;  // Angle to rotate when rotating to the next piece
    [SerializeField] float animationDuration = 2;   // Duration of the rotation animation, in seconds
    bool ownsRotation = false;  // Whether the group owns the rotation or the (Only 1 rotation is allowed at a time on the planet)


    // Note: current_rotation is [-1, 1]. At current_rotation=1, piece is rotated to the next (NOT a full rotation)
    float current_rotation = 0;
    
    // Claim rotation for this group. Returns true if successful, false if another group is already rotating
    private bool ClaimRotation() {
        if (belongs_planet.isGroupRotating) {
            return ownsRotation;
        }
        else {
            belongs_planet.isGroupRotating = true;
            ownsRotation = true;
            return true;
        }
    }

    // Release rotation claim
    private void UnclaimRotation()
    {
        if (belongs_planet.isGroupRotating && ownsRotation)
        {
            belongs_planet.isGroupRotating = false;
            ownsRotation = false;
        }
    }

    // Swap pieces in the group clockwise if cw is true, counterclockwise if false
    private void Swap(bool cw)
    {
        if (!ClaimRotation())
        {   // If another group is already rotating
            return;
        }

        for (int i = 0; i < swaps.Length; i++)
        {
            PiecePlaceholder[] p = swaps[i].placeholders;
            if (p.Length < 2)
            {
                continue;
            }

            if (cw)
            {
                Piece first = p[p.Length - 1].piece;
                for (int j = p.Length - 2; j >= 0; j--)
                {
                    p[j + 1].SetPiece(p[j]);
                }
                p[0].SetPiece(first);
            }
            else
            {
                Piece first = p[0].piece;
                for (int j = 1; j < p.Length; j++)
                {
                    p[j - 1].SetPiece(p[j]);
                }
                p[p.Length - 1].SetPiece(first);
            }
        }
    }

    // Smoothly animate rotation back to where it was before rotation started
    IEnumerator SmoothCancelRotation() {
        float start_rotation = Mathf.Abs(current_rotation);
        float time_remaining = start_rotation*animationDuration;
        float f_cw = current_rotation > 0 ? 1 : -1;
        float frameRotation;
        while (time_remaining > 0f) {
            time_remaining -= Time.deltaTime;
            frameRotation = f_cw*(1-(animationDuration-time_remaining)/animationDuration);
            frameRotation = f_cw == 1 ? Mathf.Max(frameRotation, 0) : Mathf.Min(frameRotation, 0);
            SetRotation(frameRotation);

            yield return null;
        }
        SetRotation(0);
        UnclaimRotation();
        yield break;
    }

    // Smoothly animate rotation to the next piece, then swap pieces
    IEnumerator SmoothEndRotation(bool cw) {
        float start_rotation = Mathf.Abs(current_rotation);
        float time_remaining = animationDuration - start_rotation*animationDuration;
        float f_cw = cw ? 1 : -1;
        float frameRotation;
        while (time_remaining > 0f) {
            time_remaining -= Time.deltaTime;
            frameRotation = Mathf.Clamp(f_cw*(animationDuration-time_remaining)/animationDuration, -1, 1);
            SetRotation(frameRotation);

            yield return null;
        }

        SetRotation(cw ? 1 : -1);
        Swap(cw);
        current_rotation = 0;
        UnclaimRotation();
        yield break;
    }

    // Confirm rotation in the given direction (cw = clockwise)
    public void ConfirmRotation(bool cw)
    {
        if (!ClaimRotation())
        {   // If another group is already rotating
            return;
        }

        StartCoroutine(SmoothEndRotation(cw));
    }


    // Note: angle_prop is [-1, 1]. At angle_prop=1, piece should be rotated to the next (NOT a full rotation)
    // Graphic modification only
    public void SetRotation(float angle_prop) {
        if (!ClaimRotation()) {   // If another group is already rotating
            return;
        }

        foreach (Placeholders placeholders in swaps) {
            foreach (PiecePlaceholder pholder in placeholders.placeholders) {
                pholder.piece.transform.RotateAround(transform.position, transform.up, rotationAngle*(angle_prop - current_rotation));
            }
        }
        current_rotation = angle_prop;
    }

    // Cancel rotation, smoothly animating back to the original position
    public void CancelRotation()
    {
        if (!ClaimRotation())
        {     // If another group is already rotating
            return;
        }
        StartCoroutine(SmoothCancelRotation());
    }
}