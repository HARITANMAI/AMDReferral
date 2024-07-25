using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcPlane : MonoBehaviour
{
    private MeshFilter m_Filter;
    private Mesh m_Mesh;
    private List<Vector3> m_Vertices;
    private List<int> m_Tris;

    [SerializeField] private Vector3Int m_GridSize;
    [SerializeField] private Vector3 m_CellOffset;

    public Vector3 CellOffset
    {
        get { return m_CellOffset; }
        set { m_CellOffset = value; }
    }

    private void Awake()
    {
        m_Filter = GetComponent<MeshFilter>();
        m_Mesh = new Mesh { name = "Proc Mesh" };
        GenMesh();
        m_Filter.mesh = m_Mesh;
    }

    public void OnValidate()

    {
        if (m_Mesh == null)
        {
            m_Filter = GetComponent<MeshFilter>();
            m_Mesh = new Mesh { name = "Proc Mesh" };
        }

        GenMesh();
        m_Filter.mesh = m_Mesh;
    }

    private void GenMesh()
    {



        m_Vertices = new List<Vector3>();



        m_Tris = new List<int>();







        for (int i = 0; i < m_GridSize.x; i++)



        {



            for (int j = 0; j < m_GridSize.y; j++)



            {



                for (int k = 0; k < m_GridSize.z; k++)



                {



                    GenCube(new Vector3(i * m_CellOffset.x, j * m_CellOffset.y, k * m_CellOffset.z), m_Vertices.Count,



                    ref m_Vertices, ref m_Tris);



                }



            }



        }







        m_Mesh.Clear();



        m_Mesh.SetVertices(m_Vertices);



        m_Mesh.SetTriangles(m_Tris, 0);



        //m_Mesh.SetNormals(m_Vertices);



        m_Mesh.RecalculateNormals();



    }



    private static void GenCube(Vector3 RefPos, int RefTri, ref List<Vector3> vertexArray, ref List<int> triArray)



    {



        //-z



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, -0.5f));



        triArray.AddRange(new int[] { RefTri + 0, RefTri + 1, RefTri + 3, RefTri + 1, RefTri + 2, RefTri + 3 });







        //+z



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, 0.5f));



        triArray.AddRange(new int[] { RefTri + 4, RefTri + 7, RefTri + 5, RefTri + 5, RefTri + 7, RefTri + 6 });







        //-x



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, 0.5f));



        triArray.AddRange(new int[] { RefTri + 8, RefTri + 9, RefTri + 11, RefTri + 9, RefTri + 10, RefTri + 11 });







        //+x



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, 0.5f));



        triArray.AddRange(new int[] { RefTri + 12, RefTri + 15, RefTri + 13, RefTri + 13, RefTri + 15, RefTri + 14 });







        //-y



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, -0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, -0.5f, 0.5f));



        triArray.AddRange(new int[] { RefTri + 16, RefTri + 17, RefTri + 19, RefTri + 17, RefTri + 18, RefTri + 19 });







        //+y



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, -0.5f));



        vertexArray.Add(RefPos + new Vector3(0.5f, 0.5f, 0.5f));



        vertexArray.Add(RefPos + new Vector3(-0.5f, 0.5f, 0.5f));



        triArray.AddRange(new int[] { RefTri + 20, RefTri + 23, RefTri + 21, RefTri + 21, RefTri + 23, RefTri + 22 });



    }

}



[CustomEditor(typeof(ProcPlane)), CanEditMultipleObjects]
public class ProcPlaneEditor : Editor
{
    private void OnSceneGUI()
    {
        ProcPlane thing = (ProcPlane)target;
        EditorGUI.BeginChangeCheck();

        Vector3 newCellOffset = Handles.PositionHandle(thing.transform.position + thing.transform.TransformDirection(thing.CellOffset), thing.transform.rotation);
        //  Vector3 newGridSize = Handles.ScaleHandle(thing.GridSize, thing.transform.position + thing.transform.TransformVector(-thing.CellOffset), thing.transform.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(thing, $"Changed the Cell Offset of {thing.gameObject.name}");
            thing.CellOffset = thing.transform.InverseTransformDirection(newCellOffset) - thing.transform.position;
            //thing.GridSize = new Vector3Int(Mathf.FloorToInt(newGridSize.x), Mathf.FloorToInt(newGridSize.y), Mathf.FloorToInt(newGridSize.z));
            thing.OnValidate();
        }
    }
}
