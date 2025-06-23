using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// handler class for building meshes

public class MeshBuilder
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector2> UVs = new List<Vector2>();
    private List<int> Indices = new List<int>();
    private List<int> SubMeshes = new List<int>();
    private int NumOfSubMeshes = 0;

    public void AddTriangle(int index0, int index1, int index2, int subMesh)
    {
        Indices.Add(index0);
        Indices.Add(index1);
        Indices.Add(index2);

        SubMeshes.Add(subMesh);

        NumOfSubMeshes = subMesh+1 > NumOfSubMeshes ? subMesh+1: NumOfSubMeshes;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.subMeshCount = NumOfSubMeshes;

        mesh.vertices = Vertices.ToArray();

        for(int i = 0; i < NumOfSubMeshes; i++)
        {
            List<int> IndicesInSubMesh = new List<int>();
            for (int j = 0; j < SubMeshes.Count; j++)
                if(SubMeshes[j] == i)
                {
                    IndicesInSubMesh.Add(Indices[j*3]);
                    IndicesInSubMesh.Add(Indices[j*3 + 1]);
                    IndicesInSubMesh.Add(Indices[j*3 + 2]);
                }
            mesh.SetTriangles(IndicesInSubMesh, i);
        }

        if(Normals.Count == Vertices.Count)
            mesh.normals = Normals.ToArray();

        if(UVs.Count == Vertices.Count)
            mesh.uv = UVs.ToArray();

        mesh.RecalculateBounds();

        return mesh;
    }
}