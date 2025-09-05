using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    private int _waterLevel = -1;
    public int WaterLevel {get {return _waterLevel;} private set {_waterLevel = value;} }

    private int _groundHeight = 2;
    public int GroundHeight {get {return _groundHeight;} private set {_groundHeight = value;} }
}
