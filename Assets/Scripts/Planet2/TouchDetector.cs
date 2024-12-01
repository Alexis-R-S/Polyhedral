using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TouchDetector : MonoBehaviour
{
    [SerializeField] PieceGroup[] affectedGroups;
    [SerializeField] Vector3[] _Directions;
    public Vector3[] Directions {
        get {
            return _Directions;
        }
        private set {
            _Directions = value;
        }
    }

    void Awake() {
        // Normalize every direction vector
        for (int i=0; i<Directions.Length; i++) {
            if (Directions[i] != Vector3.zero) {
                Directions[i] = Directions[i].normalized;
            }
        }
    }

    // Note: angle_prop is [-1, 1]. At angle_prop=1, piece should be rotated to the next (NOT a full rotation)
    public void SetRotation(int axis_index, float angle_prop) {
        // Debug.Log(angle_prop);
        affectedGroups[axis_index].SetRotation(angle_prop);
    }

    public void CancelRotation(int axis_index) {
        affectedGroups[axis_index].CancelRotation();
    }

    // cw: clockwise or not
    public void ConfirmRotation(int axis_index, bool cw) {
        // Debug.Log("CONFIRM " + (cw ? "" : "counter-") + "clockwise");
        affectedGroups[axis_index].ConfirmRotation(cw);
    }
}
