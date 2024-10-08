using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public bool isGroupRotating = false;

    void Awake() {
        isGroupRotating = false;
    }
}
