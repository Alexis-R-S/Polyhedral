using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] string planetType;   // e.g. "icosphere_3"
    public string Type {get { return planetType;}}
    public bool isGroupRotating = false;    // Whether a group on the planet is currently rotating or not

    [SerializeField] private int _perlinSeed = 8000;    // Seed for the perlin noise used for all pieces of this planet
    public int perlinSeed {get {return _perlinSeed;} private set {_perlinSeed = value;}}

    void Awake() {
        isGroupRotating = false;
    }
}
