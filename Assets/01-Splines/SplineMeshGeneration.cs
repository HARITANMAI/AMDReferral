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
    [SerializeField, Range(1, 100)] private int segments;
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField, Range(2, 200f)] float width;
    Mesh mesh;
    float3 position;
    float3 tangent;
    float3 upVector;

    //Lists to draw gizmos
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
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t)); //I'm calling the point on the curve at value 't' as bezier point throughout the code

            //Getting the XYZ Orientation
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            Vector3 bezierPointZ = container.EvaluateTangent(t);
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);
            bezierPointX.Normalize();

            //Calculating the vertices in local space from the point on the curve
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


        //Setting up triangles
        List<int> tris = new List<int>();
        int vCount = verts.Count;
        for(int i = 0; i < segments; i++)
        {
            //Multiplying first vertex by 2 since difference between the first vertex of adjacent segements is 2
            int v0  = i * 2;
            int v1 = v0 + 1;
            int v2 = v0 + 2;
            int v3 = v0 + 3;
            // % ensures the value doesn't cross the maximum available vertices
            //int v2 = (v0 + 2) % vCount;
            //int v3 = (v0 + 3) % vCount;

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
        verticesCol.Clear();
        for (int i = 1; i < segments; i += 2)
        {
            float t = i / (float)(segments);

            //Getting Bezier point, it's XYZ coords and reducing the y to make it not overlap the road mesh
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t));
            bezierPoint.y -= 4f;

            Vector3 bezierPointZ = container.EvaluateTangent(t);
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);

            //X and Z needs to be normalized to give accurate mesh vertex coordinates
            bezierPointX.Normalize();
            bezierPointZ.Normalize();

            //Four points that form a square around the bezier point which is the center of each segement
            Vector3 point1 = bezierPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4); //Top right
            Vector3 point2 = bezierPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4); //Top left
            Vector3 point3 = bezierPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4); //Bottom right
            Vector3 point4 = bezierPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4); //Bottom left

            verts.Add(point1);
            verts.Add(point2);
            verts.Add(point3);
            verts.Add(point4);

            //Raycasting to check until where the pillars should extend to
            Vector3 castPoint = container.EvaluatePosition(t);
            Vector3 groundPoint = bezierPoint;

            if (Physics.Raycast(castPoint, Vector3.down, out RaycastHit hit))
            {
                groundPoint = transform.InverseTransformPoint(hit.point);
                //Debug.Log($"The raycast hit at location: {groundPoint}");
            }
            else
            {
                Debug.Log($"The raycast did not hit anything");
            }

            //Pillars will go downward instead of bending towards the world origin
            groundPoint = new Vector3(bezierPoint.x, groundPoint.y - 10f, bezierPoint.z);

            //Four points that form a square at the ground point where the raycast hit
            Vector3 point5 = groundPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 point6 = groundPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
            Vector3 point7 = groundPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
            Vector3 point8 = groundPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);

            verts.Add(point5);
            verts.Add(point6);
            verts.Add(point7);
            verts.Add(point8);

            DebugPillarVertices(t, bezierPointX, bezierPointZ);

            //Setting up triagnle indices to connect the verts to form a pillar mesh
            //These new tri indices needs to be set at vertices that come after the road mesh so we add the total vert count to the 1st index
            int tri0 = 0 + vCount;
            int tri1 = tri0 + 1;
            int tri2 = tri0 + 2;
            int tri3 = tri0 + 3;

            //Bottom square indices
            int tri4 = tri0 + 4;
            int tri5 = tri0 + 5;
            int tri6 = tri0 + 6;
            int tri7 = tri0 + 7;

            //Top Triangle
            tris.Add(tri1);
            tris.Add(tri2);
            tris.Add(tri3);

            tris.Add(tri0);
            tris.Add(tri2);
            tris.Add(tri1);

            //Bottom Triangle
            tris.Add(tri7);
            tris.Add(tri6);
            tris.Add(tri5);

            tris.Add(tri5);
            tris.Add(tri6);
            tris.Add(tri4);

            //Right Triangle
            tris.Add(tri0);
            tris.Add(tri4);
            tris.Add(tri6);

            tris.Add(tri6);
            tris.Add(tri2);
            tris.Add(tri0);

            //Left Triangle
            tris.Add(tri7);
            tris.Add(tri5);
            tris.Add(tri1);

            tris.Add(tri3);
            tris.Add(tri7);
            tris.Add(tri1);

            //Front Triangle
            tris.Add(tri5);
            tris.Add(tri4);
            tris.Add(tri0);

            tris.Add(tri0);
            tris.Add(tri1);
            tris.Add(tri5);

            //Back Triangle
            tris.Add(tri6);
            tris.Add(tri3);
            tris.Add(tri2);

            tris.Add(tri7);
            tris.Add(tri3);
            tris.Add(tri6);

            //8 vertices per pillar are being added every iteration to the max vertex count
            vCount += 8;
        }
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);

        Debug.Log($"VERTICES COUNT: {verts.Count}");
        Debug.Log($"TRI-INDICES COUNT: {tris.Count}");
    }

    //Function for drawing gizmo spheres at the pillar's vertices
    void DebugPillarVertices(float t, Vector3 bezierPointX, Vector3 bezierPointZ)
    {
        //Gizmo Debug Vertices for visual representation
        Vector3 debugPoint = container.EvaluatePosition(t);
        Vector3 debugPoint1 = debugPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
        Vector3 debugPoint2 = debugPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
        Vector3 debugPoint3 = debugPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
        Vector3 debugPoint4 = debugPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);

        Vector3 groundDebugPoint = debugPoint;

        if (Physics.Raycast(debugPoint, Vector3.down, out RaycastHit hit))
        {
            groundDebugPoint = hit.point;
        }

        Vector3 debugPoint5 = groundDebugPoint + (bezierPointX * width / 4) + (bezierPointZ * width / 4);
        Vector3 debugPoint6 = groundDebugPoint - (bezierPointX * width / 4) + (bezierPointZ * width / 4);
        Vector3 debugPoint7 = groundDebugPoint + (bezierPointX * width / 4) - (bezierPointZ * width / 4);
        Vector3 debugPoint8 = groundDebugPoint - (bezierPointX * width / 4) - (bezierPointZ * width / 4);

        verticesCol.Add(debugPoint1);
        verticesCol.Add(debugPoint2);
        verticesCol.Add(debugPoint3);
        verticesCol.Add(debugPoint4);
        verticesCol.Add(debugPoint5);
        verticesCol.Add(debugPoint6);
        verticesCol.Add(debugPoint7);
        verticesCol.Add(debugPoint8);
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

        //Display 't' value, segements, pillar positions
        for (int i = 0; i <= segments; i++)
        {
            float percent = i / (float)segments;

            Vector3 position = container.EvaluatePosition(percent);
            Vector3 upVector = container.EvaluateUpVector(percent);
            Vector3 forwardVector = container.EvaluateTangent(percent);
            Vector3 rightVector = Vector3.Cross(upVector, forwardVector).normalized;

            Vector3 leftPos = position - (rightVector * width);
            Vector3 rightPos = position + (rightVector * width);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(leftPos, rightPos);

            //Draws line on every alternate segment
            if( i % 2 != 0)
            {
                Vector3 calcPos = position;
                calcPos.y = -100;
                Gizmos.DrawLine(position, calcPos);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 20f);
        Gizmos.color = Color.white;
    }
}
