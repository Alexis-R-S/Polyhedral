using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInfo
{
    public int[] iCorners;  // Indices of the corners of the mesh
    public Vector3 planeNormal; // Normal vector of the plane of the mesh
    public float planeOrigin;   // Origin of the plane of the mesh (d in plane equation)
    public Vector3 planeUnitX;  // Unit vector in the plane of the mesh (arbitrary axis)
    public Vector3 planeUnitY;  // Unit vector in the plane of the mesh (arbitrary axis, not always orthogonal to planeUnitX)
    public float widthFactor;   // Factor to multiply the width of the mesh in the plane to get the real width (in 3D)
}
