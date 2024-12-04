using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Deformation {
    curve1
} 

public class PieceMeshDeformer
{
    public const float HEIGHT_UNIT = 0.1f;
    int[] noticableVertices;    // Indices of noticable vertices
    int[] iCorners;     // Indices of corners
    public Vector3 planeNormal;
    public float planeOrigin;


    public PieceMeshDeformer(MeshInfo meshInfo) {
        noticableVertices = meshInfo.noticableVertices;
        iCorners = meshInfo.iCorners;
        planeNormal = meshInfo.planeNormal.normalized;
        planeOrigin = meshInfo.planeOrigin* meshInfo.planeNormal.magnitude;
    }


    public void Deform(Mesh mesh, int[] noticableHeights, int[] cornersHeights, Deformation defType = Deformation.curve1) {
        if (noticableHeights.Length != noticableVertices.Length) {
            Debug.LogError("Given noticable heights has incorrect length.");
        }

        Vector3[] verts = mesh.vertices;

        // Update noticable vertices
        for (int i_notic=0; i_notic<noticableVertices.Length; i_notic++) {
            int i_vert=noticableVertices[i_notic];
            verts[i_vert] = setHeightToPlane(verts[i_vert], HEIGHT_UNIT*noticableHeights[i_notic]);
        }

        float weightedSum;
        float sumOfWeights;
        float weight;
        bool isNoticable;
        Vector3 projPoint;
        for (int i=0; i<verts.Length; i++) {
            projPoint = toPlane(verts[i]);

            // Each vert is a weighted average of noticable vertices. Weight is 1/d² (d is the distance vert-noticableVert, on y and z only)
            weightedSum = 0;
            sumOfWeights = 0;
            isNoticable = false;
            for (int j=0; j<noticableVertices.Length; j++) {
                // If at some point we find out vertex belongs to noticable vertices.
                if (noticableVertices[j] == i) {
                    isNoticable = true;
                    break;
                }
                // If vertex is NOT noticable, continue to calculate weighted average
                else {
                    weight = getWeight(defType, Vector3.Distance(projPoint, toPlane(verts[noticableVertices[j]])));    // TODO : only x and z
                    weightedSum += weight * HEIGHT_UNIT*noticableHeights[j] ;
                    sumOfWeights += weight;
                }
            }

            // If vertex does not belongs to noticables
            if (!isNoticable) {
                // Add a weight for edge of mesh
                weightDistanceOfEdges(defType, projPoint, verts, cornersHeights, ref weightedSum, ref sumOfWeights);

                // end average calculation (if denominator is not 0)
                if (sumOfWeights == 0) {
                    Debug.LogError("Noticable vertices not found.");
                }
                else {
                    verts[i] = setHeightToPlane(projPoint, weightedSum / sumOfWeights);
                }
            }
        }
        mesh.vertices = verts;
    }

    // Update sumOfWeight and weightedSum to take distance to edges into account.
    private void weightDistanceOfEdges(Deformation defType, Vector3 projPoint, in Vector3[] verts, in int[] cornersHeights, ref float weightedSum, ref float sumOfWeights) {
        // For each corner (C1 of index i_1), and the following (C2 of index i_2)
        for (int i_1=0; i_1<iCorners.Length; i_1++) {
            int i_2 = (i_1+1)%iCorners.Length;

            // edgeVect is C2 - C1
            Vector3 edgeVect = verts[iCorners[i_2]] - verts[iCorners[i_1]];

            // progression is the proportion by which orthogonal projection of projPoint is located on edgeVect
            float progression = (Vector3.Dot(edgeVect, projPoint - verts[iCorners[i_1]]) / edgeVect.sqrMagnitude);
            
            // edgePoint is the orthogonal projection of projPoint on edgeVect
            Vector3 edgePoint = progression * edgeVect + verts[iCorners[i_1]];

            // distance of projPoint to edge
            float distance = Vector3.Distance(edgePoint, projPoint);

            float weight = getWeight(defType, distance);
            if (weight == Mathf.Infinity) {
                // If point is on edge, then it only depends on heights of C1 and C2
                weightedSum = getEdgeHeight(cornersHeights[i_1], cornersHeights[i_2], progression);
                sumOfWeights = 1;
                return;
            }
            weightedSum += weight * getEdgeHeight(cornersHeights[i_1], cornersHeights[i_2], progression);
            sumOfWeights += weight;
        }
    }

    // heights of corner1 C1 and corner2 C2 are ints (uses HEIGHT_UNIT)
    // progression depends on where the edge E is located between the two corners.
    // example: C1 - - - - - - E - - C2    : progression is 0,7 (70 %)
    private float getEdgeHeight(int height_corner1, int height_corner2, float progression) {
        return progression*height_corner2*HEIGHT_UNIT + (1-progression)*height_corner1*HEIGHT_UNIT;
    }

    // Applies orthogonal projection of point on the plane of the mesh
    private Vector3 toPlane(Vector3 point) {
        // Project point to plane
        float projector = (planeNormal.x*point.x + planeNormal.y*point.y + planeNormal.z*point.z + planeOrigin)
                        / (Mathf.Pow(planeNormal.x, 2) + Mathf.Pow(planeNormal.y, 2) + Mathf.Pow(planeNormal.z, 2));

        return new Vector3(point.x - planeNormal.x*projector, point.y - planeNormal.y*projector, point.z - planeNormal.z*projector);
    }

    private Vector3 setHeightToPlane(Vector3 point, float height) {
        return toPlane(point) + height*planeNormal;
    }

    private float getWeight(Deformation defType, float distance) {
        switch (defType) {
            case Deformation.curve1:
                return 1/Mathf.Pow(distance, 2);

            default:
                return 0;
        }
    }
}