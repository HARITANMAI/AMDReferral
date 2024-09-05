using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;


[ExecuteInEditMode()]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineMeshGeneration : MonoBehaviour
{
    [SerializeField] SplineContainer container;
    [SerializeField, Range(1, 100)] private int segments = 1;
    [SerializeField, Range(0, 10)] private int pillarSegmentSeperation = 0;
    [SerializeField, Range(2, 6)] private int pillarWidthControl = 4;
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField, Range(2f, 200f)] float width = 20f;

    public bool applyThickness = false;
    [SerializeField, Range(2f, 200f)] float thickness = 20f;
    [SerializeField, Range(0f, 5f)] float thicknessAngle = 1f;

    public bool startAnimation = false;
    [SerializeField] GameObject animObj;
    [SerializeField, Range(0.01f, 10f)] float animSpeed = 0.01f;

    Mesh mesh;
    private float3 position;
    private float3 tangent;
    private float3 upVector;

    //Lists to draw gizmos
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> verticesCol = new List<Vector3>();
    int vCount = 0;

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

        if (startAnimation)
        {
            t += (0.01f * animSpeed);
            t %= 1f;
            container.Evaluate(t, out position, out tangent, out upVector);
            animObj.transform.position = position;
            animObj.transform.rotation = Quaternion.LookRotation(tangent, upVector);
        }
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
            bezierPointY.Normalize();

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

        vCount = verts.Count;

        //IMPLEMENTING THICKNESS OF THE SPLINE MESH
        if (applyThickness)
        {
            //Debug.Log("THICKNESS IS BEING CALLED!");
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;

                //Getting the XYZ Orientation
                Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t)); //I'm calling the point on the curve at value 't' as bezier point throughout the code
                Vector3 bezierPointY = container.EvaluateUpVector(t);
                Vector3 bezierPointZ = container.EvaluateTangent(t);
                Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);
                bezierPointX.Normalize();
                bezierPointY.Normalize();

                // Change X width to modify the angle
                Vector3 upPoint1 = bezierPoint + (bezierPointX * width * thicknessAngle) + (bezierPointY * thickness);
                Vector3 upPoint2 = bezierPoint - (bezierPointX * width * thicknessAngle) + (bezierPointY * thickness);

                verts.Add(upPoint1);
                verts.Add(upPoint2);
            }
        }

        //Setting up triangles
        List<int> tris = new List<int>();
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

        //Thickness Tris
        if (applyThickness)
        {
            for (int i = 0; i < segments; i++)
            {
                //Multiplying first vertex by 2 since difference between the first vertex of adjacent segements is 2
                int nv0 = i * 2 + vCount;
                int nv1 = nv0 + 1;
                int nv2 = nv0 + 2;
                int nv3 = nv0 + 3;
                // % ensures the value doesn't cross the maximum available vertices
                //int v2 = (v0 + 2) % vCount;
                //int v3 = (v0 + 3) % vCount;

                tris.Add(nv3);
                tris.Add(nv2);
                tris.Add(nv0);

                tris.Add(nv0);
                tris.Add(nv1);
                tris.Add(nv3);

                int v0 = i * 2;
                int v1 = v0 + 1;
                int v2 = v0 + 2;
                int v3 = v0 + 3;

                //Right Face
                tris.Add(nv2);
                tris.Add(v2);
                tris.Add(v0);

                tris.Add(nv2);
                tris.Add(v0);
                tris.Add(nv0);

                //Left Face
                tris.Add(nv1);
                tris.Add(v1);
                tris.Add(v3);

                tris.Add(nv1);
                tris.Add(v3);
                tris.Add(nv3);
            }
        }

        vCount = verts.Count;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        verticesCol.Clear();

        //SUPPORTS COLUMNS/PILLARS UNDER THE ROAD
        GeneratePillar(-width/1.2f, verts, tris);
        GeneratePillar(width/1.2f, verts, tris);

        //Debug.Log($"VERTICES COUNT: {verts.Count}");
        //Debug.Log($"TRI-INDICES COUNT: {tris.Count}");
    }

    void GeneratePillar(float pillarDisplacement,List<Vector3> verts, List<int> tris)
    {
        for (int i = 1; i < segments; i += 2)
        {
            i += pillarSegmentSeperation;
            float t = i / (float)(segments);
            //Vector3 pillarOrigin = transform.InverseTransformPoint(container.EvaluatePosition(t));

            //Getting Bezier point, it's XYZ coords and reducing the y to make it not overlap the road mesh
            Vector3 bezierPoint = transform.InverseTransformPoint(container.EvaluatePosition(t));
            bezierPoint.y -= 1f;

            Vector3 bezierPointZ = container.EvaluateTangent(t);
            Vector3 bezierPointY = container.EvaluateUpVector(t);
            Vector3 bezierPointX = Vector3.Cross(bezierPointY, bezierPointZ);

            //X and Z needs to be normalized to give accurate mesh vertex coordinates
            bezierPointX.Normalize();
            bezierPointZ.Normalize();

            //Modifying the pillar placement's origin point
            bezierPoint = bezierPoint + (bezierPointX * pillarDisplacement);

            //Four points that form a square around the bezier point which is the center of each segement
            Vector3 point1 = bezierPoint + (bezierPointX * width / pillarWidthControl) + (bezierPointZ * width / pillarWidthControl); //Top right
            Vector3 point2 = bezierPoint - (bezierPointX * width / pillarWidthControl) + (bezierPointZ * width / pillarWidthControl); //Top left
            Vector3 point3 = bezierPoint + (bezierPointX * width / pillarWidthControl) - (bezierPointZ * width / pillarWidthControl); //Bottom right
            Vector3 point4 = bezierPoint - (bezierPointX * width / pillarWidthControl) - (bezierPointZ * width / pillarWidthControl); //Bottom left

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
            Vector3 point5 = groundPoint + (bezierPointX * width / pillarWidthControl) + (bezierPointZ * width / pillarWidthControl);
            Vector3 point6 = groundPoint - (bezierPointX * width / pillarWidthControl) + (bezierPointZ * width / pillarWidthControl);
            Vector3 point7 = groundPoint + (bezierPointX * width / pillarWidthControl) - (bezierPointZ * width / pillarWidthControl);
            Vector3 point8 = groundPoint - (bezierPointX * width / pillarWidthControl) - (bezierPointZ * width / pillarWidthControl);

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
            //I CALLED THESE VERTEX INDICES AS TRIS HERE INITIALLY AND FORGOT TO CHANGE THE NAMING

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
    }


    //Function for drawing gizmo spheres at the pillar's vertices
    void DebugPillarVertices(float t, Vector3 bezierPointX, Vector3 bezierPointZ)
    {
        //Gizmo Debug Vertices for visual representation
        //The gizmos seem to be always in local transform, so I'm not getting the Inverse here
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
            Gizmos.DrawSphere(vertices[i], 1f);
        }

        //Display column/pillar vertices
        for (int i = 0; i < verticesCol.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(verticesCol[i], 1f);
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
        Gizmos.DrawSphere(position, 1f);
        Gizmos.color = Color.white;
    }



    //EDITOR HANDLE
    [CustomEditor(typeof(SplineMeshGeneration)), CanEditMultipleObjects]
    public class SplineMeshGenerationEditor : Editor
    {
        private void OnSceneGUI()
        {
            SplineMeshGeneration spline = (SplineMeshGeneration)target;
            EditorGUI.BeginChangeCheck();

            //X Axis for segements, Y Axis for width
            Vector3 handlePos = spline.transform.position + new Vector3(spline.segments, spline.width, spline.animSpeed);
            handlePos = Handles.PositionHandle(handlePos, spline.transform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, $"Changed segements of {spline.gameObject.name}");
                int newSegments = Mathf.Clamp(Mathf.RoundToInt(handlePos.x - spline.transform.position.x), 1, 100);
                spline.segments = newSegments;

                float newWidth = Mathf.Clamp(handlePos.y - spline.transform.position.y, 1f, 200f);
                spline.width = newWidth;

                float newSpeed = Mathf.Clamp(handlePos.z - spline.transform.position.z, 0.01f, 10f);
                spline.animSpeed = newSpeed;

                spline.GenerateMesh();
            }

            Handles.color = Color.white;
            Handles.DrawLine(spline.transform.position, handlePos);
        }
    }
}