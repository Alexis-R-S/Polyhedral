using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceMeshDeformer
{
    public const float HEIGHT_UNIT = 1f;
    public const float SEED_MODULO = 1000f;
    public const float SLOPE_CALCULATION_DISTANCE = 0.01f; // Distance used to calculate slope for erosion
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
        // TODO: detail controlled by smoothness of the piece
        // TODO: optimize by caching projections and distances? (AI suggestion)
        // TODO: erosion

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
            float eroded_perlin_height = erosionAt(plane_XY, perlinSeed, perlinOpt, 5, 0.1f, 0.01f);
            verts[i] = setHeightToPlane(verts[i], eroded_perlin_height * attenuation * groundHeight * HEIGHT_UNIT);
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

    // Return the fractal Perlin noise at the given coordinates
    private float perlinNoiseFractalAt(Vector2 coords, float seed, PerlinOpt perlinOpt, int detail)
    {
        int depth = Mathf.Min(detail, perlinOpt.detail);    // Limit depth to available options
        float pointHeight = 0;

        for (int i = 0; i < depth; i++)
        {
            pointHeight += perlinNoiseAt(coords, seed, perlinOpt.frequencies[i], perlinOpt.amplitudes[i]);
        }

        return pointHeight;
    }

    // Return the Perlin noise at the given coordinates
    private float perlinNoiseAt(Vector2 coords, float seed, float freq, float amp)
    {
        return amp * Mathf.PerlinNoise(coords.x * freq + seed % SEED_MODULO + SEED_MODULO, coords.y * freq + (5 * seed % SEED_MODULO) + SEED_MODULO);
    }

    // Slope-based erosion at the given coordinates
    private float erosionAt(Vector2 coords, float seed, PerlinOpt perlinOpt, int detail, float slopeTreshold, float erosionFactor)
    {
        float height = perlinNoiseFractalAt(coords, seed, perlinOpt, detail);
        Vector2 height_dl = new Vector2(
            perlinNoiseFractalAt(new Vector2(coords.x + SLOPE_CALCULATION_DISTANCE, coords.y), seed, perlinOpt, detail),
            perlinNoiseFractalAt(new Vector2(coords.x, coords.y + SLOPE_CALCULATION_DISTANCE), seed, perlinOpt, detail)
        );
        float slope = ((height_dl - new Vector2(height, height)) / SLOPE_CALCULATION_DISTANCE).magnitude;

        if (slope > slopeTreshold)
        {
            height -= (slope - slopeTreshold) * erosionFactor;
        }
        return height;
    }
}