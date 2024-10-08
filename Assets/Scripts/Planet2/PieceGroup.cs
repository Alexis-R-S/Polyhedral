using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceGroup : MonoBehaviour
{
    [System.Serializable]
    struct Placeholders {
        [SerializeField] public PiecePlaceholder[] placeholders;
    }

    [SerializeField] Planet belongs_planet;
    [SerializeField] Placeholders[] swaps;
    [SerializeField] Placeholders[] cutPieces;
    [SerializeField] float rotationAngle = 90;
    [SerializeField] float animationDuration = 2;
    bool ownsRotation = false;  // Only 1 rotation at a time


    // Note: current_rotation is [-1, 1]. At current_rotation=1, piece is rotated to the next (NOT a full rotation)
    float current_rotation = 0;
    

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

    private void UnclaimRotation() {
        if (belongs_planet.isGroupRotating && ownsRotation) {
            belongs_planet.isGroupRotating = false;
            ownsRotation = false;
        }
    }

    private void Swap(bool cw) {
        if (!ClaimRotation()) {   // If another group is already rotating
            return;
        }

        for (int i=0; i<swaps.Length; i++) {
            PiecePlaceholder[] p = swaps[i].placeholders;
            if (p.Length < 2) {
                continue;
            }

            if (cw) {
                Piece first = p[p.Length-1].piece;
                for (int j=p.Length-2; j>=0; j--) {
                    p[j+1].SetPiece(p[j]);
                }
                p[0].SetPiece(first);
            }
            else {
                Piece first = p[0].piece;
                for (int j=1; j<p.Length; j++) {
                    p[j-1].SetPiece(p[j]);
                }
                p[p.Length-1].SetPiece(first);
            }
        }
    }

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

    public void ConfirmRotation(bool cw) {
        if (!ClaimRotation()) {   // If another group is already rotating
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

    public void CancelRotation() {
        if (!ClaimRotation()) {     // If another group is already rotating
            return;
        }
        StartCoroutine(SmoothCancelRotation());
    }
}