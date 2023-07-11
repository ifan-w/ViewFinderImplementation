using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInfo
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector3> norms;
    public List<Vector2> uvs;
    public List<Vector4> tangents;
    public Dictionary<int, int> origin2localIdx;
    public MeshInfo(Mesh mesh)
    {
        vertices = new List<Vector3>(mesh.vertices);
        triangles = new List<int>(mesh.triangles);
        norms = new List<Vector3>(mesh.normals);
        uvs = new List<Vector2>(mesh.uv);
        tangents = new List<Vector4>(mesh.tangents);
        origin2localIdx = new Dictionary<int, int>();
    }
    public MeshInfo()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        norms = new List<Vector3>();
        uvs = new List<Vector2>();
        tangents = new List<Vector4>();
        origin2localIdx = new Dictionary<int, int>();
    }
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.tangents = tangents.ToArray();
        return mesh;
    }

    public Vector3 Vertices(int idx)
    {
        return vertices[idx];
    }
    public int Triangles(int idx)
    {
        return triangles[idx];
    }
    public Vector3 Norms(int idx)
    {
        return norms[idx];
    }
    public Vector2 UVs(int idx)
    {
        return uvs[idx];
    }
    public Vector3 Tangents(int idx)
    {
        return tangents[idx];
    }

    public int AddVertice(int idx, MeshInfo origin)
    {
        if(origin2localIdx.ContainsKey(idx)) { return origin2localIdx[idx]; }
        int localIdx = vertices.Count;
        origin2localIdx[idx] = localIdx;
        vertices.Add(origin.Vertices(idx));
        norms.Add(origin.Norms(idx));
        uvs.Add(origin.UVs(idx));
        tangents.Add(origin.Tangents(idx));
        return localIdx;
    }
    public int AddVertice(Vector3 vertice, Vector3 norm, Vector2 uv, Vector4 tangent)
    {
        int idx = vertices.Count;
        vertices.Add(vertice);
        norms.Add(norm);
        uvs.Add(uv);
        tangents.Add(tangent);
        return idx;
    }
    public void AddTriangle(int idx0, int idx1, int idx2)
    {
        triangles.Add(idx0);
        triangles.Add(idx1);
        triangles.Add(idx2);
    }
    public int DuplicateVertice(int idx)
    {
        int resultIdx = vertices.Count;
        vertices.Add(vertices[idx]);
        norms.Add(norms[idx]);
        uvs.Add(uvs[idx]);
        tangents.Add(tangents[idx]);
        return resultIdx;
    }
}
