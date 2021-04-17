using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMesh : MonoBehaviour
{
    private List<Mesh> meshes;
    private List<Vector3[]> initVerticesList;

    public Material mat;
    // Start is called before the first frame update
    void Start()
    {

        meshes = new List<Mesh>();
        initVerticesList = new List<Vector3[]>();

        //CreateGrid(10, 10);
        CreateSphere(Vector3.zero, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            var initVerts = initVerticesList[i];
            Translate(meshes[i], new Vector3(0, 0, Mathf.Cos(Time.time * 2) * 3f), initVerts);
            LocalRotate(meshes[i], Quaternion.AngleAxis(Time.time * 60, new Vector3(0, 0, 1)), meshes[i].vertices);
            LocalScale(meshes[i], Mathf.Sin(Time.time) * 0.5f + 1.0f, meshes[i].vertices);
        }
    }

    private void CreateSphere(Vector3 center, float rad)
    {
        int res = 16;
        List<Vector3> vertexList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        for (int i = 0; i < res; i++) {
            var ang1 = Mathf.PI * i / (float)(res - 1);
            var x1 = Mathf.Cos(ang1) * rad;
            var y1 = Mathf.Sin(ang1) * rad;

            for(int n=0; n<res * 2; n++)
            {
                var ang2 = Mathf.PI * 2.0f * n / (float)(res * 2);
                var x2 = Mathf.Cos(ang2) * y1;
                var y2 = x1;
                var z2 = Mathf.Sin(ang2) * y1;

                var vert = new Vector3(x2, y2, z2);
                vertexList.Add(vert);
                Vector2 uv = new Vector2(ang1 / Mathf.PI, ang2 / (Mathf.PI * 2f));
                uvList.Add(uv);
            }
        }

        List<int> triangles = new List<int>();
        for(int i=0; i<res-1; i++)
        {
            int i1 = i;
            int i2 = i + 1;

            for(int n=0; n<res * 2; n++)
            {
                int index11 = (res * 2) * i1 + n;
                int index12 = (res * 2) * i1 + (n + 1) % (res * 2);
                int index21 = (res * 2) * i2 + n;
                int index22 = (res * 2) * i2 + (n + 1) % (res * 2);

                triangles.Add(index11);
                triangles.Add(index22);
                triangles.Add(index21);
                triangles.Add(index11);
                triangles.Add(index12);
                triangles.Add(index22);
            }
        }

        CreateMesh(vertexList.ToArray(), triangles.ToArray(), uvList.ToArray());

    }

    private void CreateQuad(Vector3 center)
    {
        

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1,-1, 0) + center,
            new Vector3(1, -1, 0) + center,
            new Vector3(1, 1, 0) + center,
            new Vector3(-1, 1, 0) + center
        };

        int[] triangles = new int[]
        {
            0,1,2,
            0,2,3
        };

        Vector2[] uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f)
        };

        CreateMesh(vertices, triangles, uv);
    }

    private void CreateMesh(Vector3[] vertices, int[] triangles, Vector2[] uv)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        meshes.Add(mesh);
        initVerticesList.Add(vertices);


        GameObject gb = new GameObject();
        var meshFilter = gb.AddComponent<MeshFilter>();
        var meshRenderer = gb.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = mat;
    }

    private void CreateGrid(int x, int y)
    {
        for(int i=0; i<x; i++)
        {
            for(int n=0; n<y; n++)
            {
                CreateQuad(new Vector3(i * 3, n * 3, 0));
            }
        }
    }

    

    private void Translate(Mesh msh, Vector3 trans, Vector3[] initVerts = null)
    {
        Vector3[] newVertices = new Vector3[msh.vertices.Length];
        var currentVetices = initVerts == null ? (Vector3[])msh.vertices.Clone() : initVerts;
        for (int i = 0; i < currentVetices.Length; i++)
        {
            var vert = currentVetices[i];

            vert += trans;
            newVertices[i] = vert;
        }
        msh.vertices = newVertices;
        msh.RecalculateNormals();
    }

    private void LocalRotate(Mesh msh, Quaternion rot, Vector3[] initVerts = null)
    {
        Vector3[] newVertices = new Vector3[msh.vertices.Length];
        var currentVetices = initVerts == null ? (Vector3[])msh.vertices.Clone() : initVerts;
        for (int i = 0; i < currentVetices.Length; i++)
        {
            msh.RecalculateBounds();
            var cen = msh.bounds.center;
            var vert = currentVetices[i];

            Quaternion rotQuat = rot;
            vert -= cen;
            vert = rotQuat * vert;
            vert += cen;
            newVertices[i] = vert;
        }
        msh.vertices = newVertices;
        msh.RecalculateNormals();
    }

    
    private void LocalScale(Mesh msh, float scale, Vector3[] initVerts = null)
    {
        Vector3[] newVertices = new Vector3[msh.vertices.Length];
        var currentVetices = initVerts == null ? (Vector3[])msh.vertices.Clone() : initVerts;
        for (int i = 0; i < currentVetices.Length; i++)
        {
            msh.RecalculateBounds();
            var cen = msh.bounds.center;
            var vert = currentVetices[i];

            vert -= cen;
            vert *= scale;
            vert += cen;
            newVertices[i] = vert;
        }
        msh.vertices = newVertices;
        msh.RecalculateNormals();
    }
}
