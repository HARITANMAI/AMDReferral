using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Mesh2D : ScriptableObject
{
    [System.Serializable]
    public class Vertex
    {
        public Vector2 points;
        public Vector2 normal;
        public float u;
    }

    //Making array to store vertices of the 2D mesh
    public Vertex[] vertices;
    public int[] lineIndices;


}
