using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode()]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRollerCoaster : MonoBehaviour
{
    [SerializeField] SplineContainer container;
    [SerializeField, Range(1, 128)] private int segments;
    [SerializeField, Range(0, 5)] private int segmentGap;
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField, Range(2, 200f)] float width;
    Mesh mesh;
    float3 position;
    float3 tangent;
    float3 upVector;
    List<Vector3> vertices = new List<Vector3>();

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
            float t = i / (float)segments;

            //Used InverseTransformPoint to fix the bug where verts were being set in the world space
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t));

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

            //Debugging Gizmos Spheres, using p1 and p2 would generate the debug gizmos according to world space
            Vector3 debugPoint = container.EvaluatePosition(t);
            Vector3 point3 = debugPoint + (bezierPointX * width);
            Vector3 point4 = debugPoint - (bezierPointX * width);
            vertices.Add(point3);
            vertices.Add(point4);
        }

        Debug.Log($"VERTICES COUNT BEFORE TRIANGLE: {verts.Count}");

        //Setting up triangles
        List<int> tris = new List<int>();
        int vCount = verts.Count;
        for (int i = 0; i < segments; i++)
        {
            int v0 = i * 2;
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

        //SUPPORT COLUMNS OF THE ROAD
        //for(int i = 1; i < segments/2; i += 2)
        //{
        //    float t = i / (float)(segments);
        //}
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(vertices[i], 10f);
        }

        for (int i = 1; i <= segments; i += 4)
        {
            float percent = i / (float)segments;

            Vector3 position = container.EvaluatePosition(percent);
            Vector3 upVector = container.EvaluateUpVector(percent);
            Vector3 forwardVector = container.EvaluateTangent(percent);
            Vector3 rightVector = Vector3.Cross(upVector, forwardVector).normalized;

            Vector3 rightPos = position + (rightVector * width);
            Vector3 leftPos = position - (rightVector * width);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(leftPos, rightPos);

            Vector3 rightCol = position + (rightVector * (width / 1.5f));
            Vector3 leftCol = position - (rightVector * (width / 1.5f));
            Vector3 rightColDown = rightCol;
            Vector3 leftColDown = leftCol;
            rightColDown.y = -10f;
            leftColDown.y = -10f;
            Gizmos.DrawLine(leftCol, leftColDown);
            Gizmos.DrawLine(rightCol, rightColDown);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 20f);
        Gizmos.color = Color.white;
    }
}
