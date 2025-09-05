using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSettings : MonoBehaviour
{
    // TODO: implement saving/loading settings to disk

    // User settings
    private int _noiseDetail = 2;
    public int noiseDetail {
        get {
            return _noiseDetail;
        }
        set {
            _noiseDetail = value;
            // TODO : update detail to max detail, and ACTUALLY USE IT
        }
    }

    // Singleton pattern
    public static UserSettings Instance = null;
    void Start() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        Instance = this;
    }
}
