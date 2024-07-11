using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SplineMesh : MonoBehaviour
{
    //Declaring an array to hold the points/knots of the bezier curve
    [SerializeField]Transform[] controlPoints = new Transform[4];

    //t value to get a point along the curve
    [Range(0, 1)][SerializeField] float t = 0;

    //Function to get the position of a specific point
    Vector3 GetPos(int i ) => controlPoints[i].position;

    public void OnDrawGizmos()
    {
        for( int i = 0; i < controlPoints.Length; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.5f);
        }

        Handles.DrawBezier(
            GetPos(0),
            GetPos(3),
            GetPos(1),
            GetPos(2),
            Color.white, EditorGUIUtility.whiteTexture, 1f);

        Vector3 testPoint = GetBezierPoint(t);
        Quaternion testOrientation = GetBezierOrientation(t);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(testPoint, 0.2f);
        Handles.PositionHandle(testPoint, testOrientation);
        Gizmos.color = Color.white;
    }

    Vector3 GetBezierPoint(float t)
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

        //returning the point which forms the berzier curve
        return Vector3.Lerp(d ,e , t);
    }

    Vector3 GetBezierTangent(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return (e-d).normalized;
    }

    Quaternion GetBezierOrientation(float t)
    {
        Vector3 tangent = GetBezierTangent(t);

        return Quaternion.LookRotation(tangent);
    }
}
