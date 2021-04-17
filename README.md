# Intro to Procedural Modeling with Unity

## はじめに / Introduction

このレポジトリはTokyo AEC Dev Groupで行うIntro to Procedrual Modeling with Unityのミートアップで参考として使うファイルが含まれています。

本ワークショップではUnityを使ってプロシージャルにメッシュをどうやって作るかの基礎を教えます。
ワークショップは次のような手順で進めていきます。

 1. Unity上でのメッシュのデータ構造を知る
 2. 四角をプロシージャルに作ってみる
 3. 頂点の位置をリアルタイムに変えてみる
 4. メッシュの変形（移動、回転、スケール）を行ってみる
 5. 複数の四角を描写してみる
 6. 複数の四角をリアルタイムに動かしてみる
 7. 球体をプロシージャルに作ってみる（時間があれば）

## 動作環境

 - Unity 2017~
 - Visual Studio 2019

 ## 1. Unity上でのメッシュのデータ構造を知る
 
 Unityのメッシュは基本的には次のようなデータを与えることで作ることができます。
 
 vertices (Vector3[]) : 頂点の位置の配列。
 triangles (int[]) : 三角形を示す頂点の番号（３つずつ）の配列。
 normals (Vector3[]) : 各頂点の法線方向の配列。verticesの配列と同じ数だけある。
 uv (Vector2[]) : 各三角形の頂点におけるUVの値。テクスチャ配置に使われる。verticesの配列と同じ数だけある。
 
 normals（法線）に関しては、verticesとtrianglesを先にMeshに与えてあげれば、
 Mesh.RecalculateNormals()という関数を使って自動的に取得することもできます。
 
 
 ## 2. 四角をプロシージャルに作ってみる
 
 まずは簡単な四角いメッシュをプロシージャルに作ってみます。Unityでは基本的にメッシュは三角形の面の集合として描かれるので、四角を描写したい場合は三角形に分解して作ってあげる必要があります。四角形の場合は二つの三角形を作ればいいことになります。
 
 ```csharp
 
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
  ```
 
## 3. 頂点の位置をリアルタイムに変えてみる
 
既存のメッシュの頂点位置を変えたい場合は、Update関数の中でメッシュのverticesを置き換えることで可能となります。
 
 ```csharp
　
 var verts = mesh.vertices[];
 verts[0] = new Vector3(0 0, Mathf.Cos(Time.time * 2f) * 4.0f);
 mesh.vertices = verts;
 ```
 
## 4. 複数の四角を描写してみる
 
 四角を描く部分を関数化しておけば、複数描写も簡単です。

```csharp
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
```
 
 ## 5. メッシュの変形（移動、回転、スケール）を行ってみる
 
 メッシュの変形は頂点の位置を変えるのと考え方は基本的には同じで、動かしたい頂点（メッシュの現在の頂点位置、あるいは指定の頂点位置）に対して移動、回転、スケールの変形を行います。
 それぞれの頂点の位置（ベクトル）に対して、
 移動は基本的にはベクトルとの足し算、
 回転は四元数（Quaternion）との掛け算、
 スケールは数値あるいはベクトルとの掛け算
 を行うことで変形を行うことが可能です。
 
 Unityでは4x4の行列データを使って三つの変形を一度に行うことも可能です。
 
 一点気を付けなければいけないのは、回転とスケールに関しては回転の中心点、あるいはスケールの中心点を意識しないとあらぬ方向に回転・スケールがされるという点です。
 
```csharp
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
```
 
 
 ## 6. 複数の四角をリアルタイムに動かしてみる
 
 作ったメッシュをリストの中に事前に入れておけば複数同時にコントロールすることも容易です。
 
 ```csharp
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
```
 
 
 ## 7. 球体をプロシージャルに作ってみる（時間があれば）
 
 最後に球体をプロシージャルに作ってみましょう。簡単な三角関数の組み合わせだけで実現可能です。
 
```csharp
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
```
 
 ## 全コード
 
```charp
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

```
