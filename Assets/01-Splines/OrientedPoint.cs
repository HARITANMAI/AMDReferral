using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct OrientedPoint
{
    public Vector3 pos;
    public Quaternion rot;

    public OrientedPoint(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }

    public OrientedPoint(Vector3 pos, Vector3 forward)
    {
        this.pos = pos;
        this.rot = Quaternion.LookRotation(forward);
    }

    //Defining a local space position to determine the width of the road from the knot which will then translate to world space
    public Vector3 LocalToWorld(Vector3 LocalSpacePosition)
    {
        return pos + rot * LocalSpacePosition;
    }

    public Vector3 LocalToWorldVect(Vector3 LocalSpacePosition)
    {
        return rot * LocalSpacePosition;
    }
}
