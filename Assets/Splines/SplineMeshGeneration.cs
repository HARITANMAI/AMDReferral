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
    [SerializeField, Range(1, 64)] private int segments;
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField, Range(2, 200f)] float width;
    Mesh mesh;
    float3 position;
    float3 tangent;
    float3 upVector;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> verticesCol = new List<Vector3>();

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "RoadSegment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
        container = GetComponent<SplineContainer>();
    }

    //Adding these 3 to ensure that the mesh change when the spline is modify in the scene
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
        List<Vector3> verts = new List<Vector3>();
        verts.Clear();
        vertices.Clear();

        for (int i = 0; i <= segments; i++)
        {
            //Getting t/percentage value on the current curve
            float t = i/(float)segments;

            //Used InverseTransformPoint to fix the bug where verts were being set in the world space
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t));

            //Getting the XYZ Orientation
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            Vector3 bezierPointZ = container.EvaluateTangent(t);
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);
            bezierPointX.Normalize();

            //Calculating the vertex in local space from the point on the curve
            Vector3 point1 = bezierPoint + (bezierPointX * width);
            Vector3 point2 = bezierPoint - (bezierPointX * width);

            //Adding the 2 vertices to the left and right side of the bezier curve
            verts.Add(point1);
            verts.Add(point2);

            //Debugging Gizmos Spheres, using p1 and p2 would generate the debug gizmos according to world space
            Vector3 debugPoint = container.EvaluatePosition(t);
            Vector3 point3 = debugPoint + (bezierPointX * width);
            Vector3 point4 = debugPoint - (bezierPointX * width);
            vertices.Add(point3);
            vertices.Add(point4);
        }

        //Debug.Log($"VERTICES COUNT BEFORE TRIANGLE: {verts.Count}");

        //Setting up triangles
        List<int> tris = new List<int>();
        int vCount = verts.Count;
        for(int i = 0; i < segments; i++)
        {
            int v0  = i * 2;
            int v1 = v0 + 1;
            int v2 = (v0 + 2) % vCount;
            int v3 = (v0 + 3) % vCount;

            tris.Add(v3);
            tris.Add(v2);
            tris.Add(v0);

            tris.Add(v0);
            tris.Add(v1);
            tris.Add(v3);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        //SUPPORTS COLUMNS/PILLARS UNDER THE ROAD
        List<Vector3> vertsPillar = new List<Vector3>();
        vertsPillar.Clear();
        verticesCol.Clear();

        for (int i = 1; i < segments; i += 2)
        {
            float t = i / (float)(segments);

            //Getting Bezier point and it's XYZ coords
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t));
            Vector3 bezierPointZ = container.EvaluateTangent(t);
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);
            bezierPointX.Normalize();

            //Four points that form a square around the bezier point which is the center of each segement
            Vector3 point1 = bezierPoint + (bezierPointX * width / 4) + (bezierPointY * width / 4);
            Vector3 point2 = bezierPoint - (bezierPointX * width / 4) + (bezierPointY * width / 4);
            Vector3 point3 = bezierPoint + (bezierPointX * width / 4) - (bezierPointY * width / 4);
            Vector3 point4 = bezierPoint - (bezierPointX * width / 4) - (bezierPointY * width / 4);

            vertsPillar.Add(point1);
            vertsPillar.Add(point2);
            vertsPillar.Add(point3);
            vertsPillar.Add(point4);

            //Raycasting to check until where the pillars should extend to
            //Vector3 groundPoint = Vector3.zero;
            //if(Physics.Raycast(bezierPoint, Vector3.down, out RaycastHit hit))
            //{
            //    groundPoint = hit.point;
            //}
            //else
            //{
            //    Debug.Log($"The raycast did not hit anything");
            //}

            //Vector3 point5 = groundPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            //Vector3 point6 = groundPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            //Vector3 point7 = groundPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
            //Vector3 point8 = groundPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);


            //Gizmo Debug Vertices for visual representation
            //Vector3 groundPoint = container.EvaluatePosition(t);
            //bezierPointY.Normalize();
            Vector3 debugPoint = container.EvaluatePosition(t);
            bezierPointZ.Normalize();

            Vector3 debugPoint1 = debugPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 debugPoint2 = debugPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 debugPoint3 = debugPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
            Vector3 debugPoint4 = debugPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);

            debugPoint.y -= 1000f; 
            Vector3 debugPoint5 = debugPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 debugPoint6 = debugPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 debugPoint7 = debugPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
            Vector3 debugPoint8 = debugPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);

            verticesCol.Add(debugPoint1);
            verticesCol.Add(debugPoint2);
            verticesCol.Add(debugPoint3);
            verticesCol.Add(debugPoint4);
            verticesCol.Add(debugPoint5);
            verticesCol.Add(debugPoint6);
            verticesCol.Add(debugPoint7);
            verticesCol.Add(debugPoint8);
        }
    }
    private void OnDrawGizmos()
    {
        //Display spline mesh vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(vertices[i], 10f);
        }

        //Display column/pillar vertices
        for (int i = 0; i < verticesCol.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(verticesCol[i], 10f);
        }

        //Display 't' value, segements, pillar position
        for (int i = 0; i <= segments; i++)
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

            //Gizmos.color = Color.blue;
            //Gizmos.DrawSphere(leftPos, 10f);
            //Gizmos.DrawSphere(rightPos, 10f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(leftPos, rightPos);

            Vector3 calcPos = position;
            calcPos.y = -100;
            Gizmos.DrawLine(position, calcPos);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 10f);
        Gizmos.color = Color.white;
    }
}
