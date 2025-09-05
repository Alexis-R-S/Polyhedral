using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;

public class PieceMeshDeformer
{
    public const float HEIGHT_UNIT = 1f;
    MeshInfo meshInfo;


    public PieceMeshDeformer(MeshInfo meshInfo)
    {
        this.meshInfo = meshInfo;

        meshInfo.planeUnitX.Normalize();
        meshInfo.planeUnitY.Normalize();
        meshInfo.planeNormal.Normalize();
        meshInfo.planeOrigin *= meshInfo.planeNormal.magnitude;
    }

    // Deform the given mesh knowing the height of the ground
    public void Deform(Mesh mesh, int groundHeight, float perlinSeed, PerlinOpt perlinOpt)
    {
        Vector3[] verts = mesh.vertices;

        // Calculate coordinates of corners on the 2D mesh
        Vector2[] cornersProj = new Vector2[meshInfo.iCorners.Length];
        for (int i = 0; i < meshInfo.iCorners.Length; i++)
        {
            cornersProj[i] = getXYFromPlane(verts[meshInfo.iCorners[i]]);
        }

        Vector2 centerProj = getXYFromPlane(meshInfo.planeOrigin * meshInfo.planeNormal);
        Vector2 plane_XY;
        for (int i = 0; i < verts.Length; i++)
        {
            plane_XY = getXYFromPlane(verts[i]);

            float attenuation = gaussDistrib(distanceToCenter(cornersProj, centerProj, plane_XY), perlinOpt);
            float perlin_height = perlinNoise(plane_XY, perlinSeed, perlinOpt, 1);
            verts[i] = setHeightToPlane(verts[i], perlin_height * attenuation * groundHeight * HEIGHT_UNIT);
        }


        mesh.vertices = verts;
    }


    // Applies orthogonal projection of point on the plane of the mesh
    private Vector3 toPlane(Vector3 point)
    {
        // Project point to plane
        float projector = (meshInfo.planeNormal.x * point.x + meshInfo.planeNormal.y * point.y + meshInfo.planeNormal.z * point.z + meshInfo.planeOrigin)
                        / (Mathf.Pow(meshInfo.planeNormal.x, 2) + Mathf.Pow(meshInfo.planeNormal.y, 2) + Mathf.Pow(meshInfo.planeNormal.z, 2));

        return new Vector3(point.x - meshInfo.planeNormal.x * projector, point.y - meshInfo.planeNormal.y * projector, point.z - meshInfo.planeNormal.z * projector);
    }

    // Return the point, with moved height on the orthogonal line
    private Vector3 setHeightToPlane(Vector3 point, float height)
    {
        return toPlane(point) + height * meshInfo.planeNormal;
    }

    // Project the point on the mesh plane. Return the projected vector.
    private Vector2 getXYFromPlane(Vector3 point)
    {
        Vector3 originToPoint = point - meshInfo.planeOrigin * meshInfo.planeNormal;
        return new Vector2(
            Vector3.Dot(originToPoint, meshInfo.planeUnitX),
            Vector3.Dot(originToPoint, meshInfo.planeUnitY)
        );
    }

    // Return the distance of the point to the given segment
    private float distanceToSegment(Vector2 point, Vector2 seg1, Vector2 seg2)
    {
        // Calculate triangle vectors
        Vector2 line1 = seg1 - point;
        Vector2 line2 = seg2 - point;
        Vector2 segment = seg2 - seg1;

        // calculate double the area of the triangle 
        float doubleArea = Mathf.Abs(line1.x * line2.y - line1.y * line2.x);

        // calculate height of the triangle
        return doubleArea / segment.magnitude;
    }

    // Return the distance of a point to the edges of the mesh. (2D only, works on the mesh plane)
    // Proportional to the center-edge distance (must be within [0;1])
    private float distanceToCenter(Vector2[] cornersProj, Vector2 centerProj, Vector2 pointProj)
    {
        float distance = 0;
        for (int i = 0; i < cornersProj.Length; i++)
        {
            int next_i = (i + 1) % cornersProj.Length;  // Index of next corner

            // Calculate distance of point and center to each edge.
            float centerDistance = distanceToSegment(centerProj, cornersProj[i], cornersProj[next_i]);
            float pointDistance = distanceToSegment(pointProj, cornersProj[i], cornersProj[next_i]);
            float propDistance = pointDistance / centerDistance;

            // Only keep largest distance to the center.
            distance = Mathf.Max(distance, 1 - propDistance);
        }

        return distance;
    }
    
    // Gauss distribution centered on 0 (with a maximum)
    private float gaussDistrib(float x, PerlinOpt perlinOpt)
    {
        return Mathf.Min(perlinOpt.height * Mathf.Exp(-Mathf.Pow(x * perlinOpt.width, perlinOpt.power)), perlinOpt.max);
    }

    private float perlinNoise(Vector2 coords, float seed, PerlinOpt perlinOpt, int detail)
    {
        return Mathf.PerlinNoise(coords.x * perlinOpt.frequencies[0] + seed, coords.y * perlinOpt.frequencies[0] + seed); // TODO
    }
} 