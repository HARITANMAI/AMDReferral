using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


[ExecuteInEditMode()]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineMeshGeneration : MonoBehaviour
{
    [SerializeField] SplineContainer container;
    [SerializeField, Range(2, 64)] private int segments = 10;
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField, Range(0, 200f)] private float width = 1.0f;
    [SerializeField, Range(0, 2.0f)] private float height = 1.0f;

    float3 position;
    float3 tangent;
    float3 upVector;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "RoadSegment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
        container = GetComponent<SplineContainer>();
        verts.Clear();
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline arg1, int arg2, SplineModification arg3)
    {
        verts.Clear();
        GenerateMesh();
    }


    private void Update()
    {
        container.Evaluate(t, out position, out tangent, out upVector);

        GenerateMesh();
    }

    void GenerateMesh()
    {
        //Clearing to prevent errors like triangle mesh referring to vertices which changed,removed,etc.
        mesh.Clear();
       
        //Setting up the vertices
        //List<Vector3> verts = new List<Vector3>();
        for(int i = 0; i <= segments; i++)
        {
            //Getting the point at t value on the curve
            float t = i/(float)segments;
            Vector3 bezierPoint = container.EvaluatePosition(t);
            //Getting the Y Orientation
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            //Getting the Z Orientation
            Vector3 bezierPointZ = container.EvaluateTangent(t);
            //Getting the X Orientation
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);
            bezierPointX.Normalize();

            //Calculating the vertex in local space from the point on the curve
            Vector3 point1 = bezierPoint + (bezierPointX * width);
            Vector3 point2 = bezierPoint - (bezierPointX * width);

            //Adding the 2 vertices to the left and right side of the bezier curve
            verts.Add(point1);
            verts.Add(point2);
        }
        mesh.SetVertices(verts);

        Debug.Log($"VERTICES COUNT BEFORE TRIANGLES {verts.Count}");

        //Setting up triangles
        List<int> tris = new List<int>();
        for(int i = 0; i < segments; i++)
        {
            int offset = i;

            int v1 = offset;
            int v2 = offset + 1;
            int v3 = offset + 2;
            int v4 = offset + 3;

            tris.Add(v1);
            tris.Add(v3);
            tris.Add(v2);

            tris.Add(v2);
            tris.Add(v3);
            tris.Add(v4);
        }
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
    }


    private void OnDrawGizmos()
    {
        for(int i = 0; i < verts.Count; i++) 
        {
            Gizmos.DrawSphere(verts[i], 5f);
        }

        for (int i = 0; i < segments; i++)
        {
            float percent = i / (float)segments;

            Vector3 position = container.EvaluatePosition(percent);
            Vector3 upVector = container.EvaluateUpVector(percent);
            Vector3 forwardVector = container.EvaluateTangent(percent);
            Vector3 rightVector = Vector3.Cross(upVector, forwardVector).normalized;

            //upVector.Normalize();
            //forwardVector.Normalize();

            Vector3 leftPos = position - (rightVector * width);
            Vector3 rightPos = position + (rightVector * width);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(leftPos, 1f);
            Gizmos.DrawSphere(rightPos, 1f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(leftPos, rightPos);

            Vector3 calcPos = position;
            calcPos.y = -100;

            Gizmos.DrawLine(position, calcPos);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 3f);
        Gizmos.color = Color.white;
    }
}
