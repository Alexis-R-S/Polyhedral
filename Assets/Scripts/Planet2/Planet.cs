using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] string planetType;
    public string Type {get { return planetType;}}
    public bool isGroupRotating = false;

    void Awake() {
        isGroupRotating = false;
    }
}
