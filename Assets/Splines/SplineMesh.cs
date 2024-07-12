using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class SplineMesh : MonoBehaviour
{
    //Declaring an array to hold the points/knots of the bezier curve
    [SerializeField]Transform[] controlPoints = new Transform[4];

    //t value to get a point along the curve
    [Range(0, 1)][SerializeField] float t = 0;

    //Function to get the position of a specific point
    Vector3 GetPos(int i ) => controlPoints[i].position;

    //Reference to the 'SO' where the elements that hold position of the road shape are stored
    [SerializeField] Mesh2D shape2D;

    public void OnDrawGizmos()
    {
        //Drawings debug spheres at all the custom set points in the world space
        for( int i = 0; i < controlPoints.Length; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.5f);
        }

        //Function which draws the curve of given points
        Handles.DrawBezier(
            GetPos(0),
            GetPos(3),
            GetPos(1),
            GetPos(2),
            Color.white, 
            EditorGUIUtility.whiteTexture,
            1f);

        //Setting a handle and debug sphere at the bezier point
        Gizmos.color = Color.red;
        OrientedPoint testPoint = GetBezierPoint(t);
        Handles.PositionHandle(testPoint.pos, testPoint.rot);

        //Drawing debug spheres at an offset from the bezier point of the curve
        float radius = 0.3f;
        void DrawPoint(Vector2 localPos) => Gizmos.DrawSphere(testPoint.LocalToWorld(localPos), radius);

        //Linq stuff idk
        Vector3[] verts = shape2D.vertices.Select(v => testPoint.LocalToWorld(v.points)).ToArray();

        //Iterate through every vertex and draw the mesh
        for (int i = 0;i < shape2D.lineIndices.Length;i++) 
        {
            Vector3 a = verts[shape2D.lineIndices[i]];
            Vector3 b = verts[shape2D.lineIndices[i+1]];    

            Gizmos.DrawLine(a, b);

            DrawPoint(shape2D.vertices[i].points * 3f);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(testPoint.pos, 0.2f);
    }

    OrientedPoint GetBezierPoint(float t)
    {
        //Have the points which you need to evaluate through
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);    
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        //abc here are the 3 points on the each sides of the berzier curve made with 4 points
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        //dc are points on the line between ab and bc
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        //Getting the bezier point and it's orientation/rotation
        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos,tangent);
    }
}
