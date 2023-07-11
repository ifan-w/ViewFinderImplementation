using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class CuttableObjectBase : MonoBehaviour
{
    // return 2 mesh, 0: positive, 1: negative
    public Mesh[] CutMesh(Tuple<Vector3, Vector3>[] planes)
    {
        Mesh currentMesh = GetComponent<MeshFilter>().mesh;
        List<MeshInfo> negMeshes = new List<MeshInfo>();
        MeshInfo posMesh = new MeshInfo(currentMesh);
        foreach(var plane in planes)
        {
            Plane cuttingPlane = new Plane(
                transform.localToWorldMatrix.transpose * plane.Item1,
                transform.InverseTransformPoint(plane.Item2)
            );
            Debug.Assert(posMesh != null);
            MeshInfo[] result = _CutMesh(
                cuttingPlane,
                posMesh
            );
            if(result[1] != null)
            {
                negMeshes.Add(result[1]);
            }
            posMesh = result[0];
            if(posMesh == null)
            {
                break;
            }
        }
        Mesh[] totalResult = new Mesh[2];
        if(posMesh != null)
        {
            totalResult[0] = posMesh.ToMesh();
        }
        else
        {
            totalResult[0] = null;
        }
        totalResult[1] = CombineMeshes(negMeshes);
        return totalResult;
    }
    private Mesh CombineMeshes(List<MeshInfo> meshes)
    {
        if(meshes.Count == 0) { return null; }
        Mesh mesh = new Mesh();
        int vertCount = 0, triCount = 0;
        foreach(var m in meshes)
        {
            vertCount += m.vertices.Count;
            triCount += m.triangles.Count;
        }
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[triCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        Vector4[] tangents = new Vector4[vertCount];
        int vertOffset = 0, triOffset = 0;
        foreach(var m in meshes)
        {
            Array.Copy(m.vertices.ToArray(), 0, vertices, vertOffset, m.vertices.Count);
            Array.Copy(m.triangles.ToArray(), 0, triangles, triOffset, m.triangles.Count);
            Array.Copy(m.norms.ToArray(), 0, normals, vertOffset, m.vertices.Count);
            Array.Copy(m.uvs.ToArray(), 0, uvs, vertOffset, m.vertices.Count);
            Array.Copy(m.tangents.ToArray(), 0, tangents, vertOffset, m.vertices.Count);
            for(int i = triOffset; i < triOffset + m.triangles.Count; i++)
            {
                triangles[i] += vertOffset;
            }
            vertOffset += m.vertices.Count;
            triOffset += m.triangles.Count;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.tangents = tangents;
        return mesh;
    }
    private MeshInfo[] _CutMesh(Plane cuttingPlane, MeshInfo origin)
    {
        MeshInfo[] result = new MeshInfo[2];
        
        MeshInfo posSideInfo = new MeshInfo();
        MeshInfo negSideInfo = new MeshInfo();
        Dictionary<int, int> posEdgeIdx2NegEdgeIdx = new Dictionary<int, int>();
        List<int> posEdges = new List<int>();

        bool[] sides = new bool[origin.vertices.Count];
        for(int i = 0; i < sides.Length; i++)
        {
            sides[i] = cuttingPlane.GetSide(origin.vertices[i]);
        }

        for(int i = 0; i < origin.triangles.Count; i+=3)
        {
            int idx0 = origin.Triangles(i), idx1 = origin.Triangles(i+1), idx2 = origin.Triangles(i+2);
            if(sides[idx0] == sides[idx1] && sides[idx0] == sides[idx2])
            {
                if(sides[idx0])
                {
                    idx0 = posSideInfo.AddVertice(idx0, origin);
                    idx1 = posSideInfo.AddVertice(idx1, origin);
                    idx2 = posSideInfo.AddVertice(idx2, origin);
                    posSideInfo.AddTriangle(idx0, idx1, idx2);
                }
                else
                {
                    idx0 = negSideInfo.AddVertice(idx0, origin);
                    idx1 = negSideInfo.AddVertice(idx1, origin);
                    idx2 = negSideInfo.AddVertice(idx2, origin);
                    negSideInfo.AddTriangle(idx0, idx1, idx2);
                }
            }
            else
            {
                int topIdx = idx0, nextIdx = idx1, prevIdx = idx2;
                if(sides[idx0] == sides[idx2])
                {
                    topIdx = idx1;
                    nextIdx = idx2;
                    prevIdx = idx0;
                }
                else if(sides[idx0] == sides[idx1])
                {
                    topIdx = idx2;
                    nextIdx = idx0;
                    prevIdx = idx1;
                }
                
                float intersectionNext = 0;
                float intersectionNext0 = -cuttingPlane.distance - Vector3.Dot(cuttingPlane.normal, origin.Vertices(topIdx));
                float intersectionNext1 = Vector3.Dot(cuttingPlane.normal, origin.Vertices(nextIdx) - origin.Vertices(topIdx));
                if(
                    (origin.Vertices(nextIdx) - origin.Vertices(topIdx)).magnitude >= 1e-6f &&
                    MathF.Abs(intersectionNext0) >= 1e-6f &&
                    Mathf.Abs(intersectionNext1) >= 1e-6f
                )
                {
                    intersectionNext = intersectionNext0 / intersectionNext1;
                }
                intersectionNext = Mathf.Clamp01(intersectionNext);
                // Debug.Assert(intersectionNext >= 0);
                // if(intersectionNext > 1)
                // {
                //     Debug.LogFormat(
                //         "line: {0}, d1 {1}, d2 {2}",
                //         origin.Vertices(nextIdx) - origin.Vertices(topIdx),
                //         -cuttingPlane.distance - Vector3.Dot(cuttingPlane.normal, origin.Vertices(topIdx)),
                //         Vector3.Dot(cuttingPlane.normal, origin.Vertices(nextIdx) - origin.Vertices(topIdx))
                //     );
                // }
                // Debug.Assert(intersectionNext <= 1);
                Vector3 verticeNext = (1 - intersectionNext) * origin.Vertices(topIdx) + intersectionNext * origin.Vertices(nextIdx);
                Vector3 normNext = ((1 - intersectionNext) * origin.Norms(topIdx) + intersectionNext * origin.Norms(nextIdx)).normalized;
                Vector2 uvNext = (1 - intersectionNext) * origin.UVs(topIdx) + intersectionNext * origin.UVs(nextIdx);
                Vector4 tangentNext = (1 - intersectionNext) * origin.Tangents(topIdx) + intersectionNext * origin.Tangents(nextIdx);

                float intersectionPrev = 0;
                float intersectionPrev0 = -cuttingPlane.distance - Vector3.Dot(cuttingPlane.normal, origin.Vertices(topIdx));
                float intersectionPrev1 = Vector3.Dot(cuttingPlane.normal, origin.Vertices(prevIdx) - origin.Vertices(topIdx));
                if(
                    (origin.Vertices(prevIdx) - origin.Vertices(topIdx)).magnitude >= 1e-6f &&
                    Mathf.Abs(intersectionPrev0) >= 1e-6f &&
                    Mathf.Abs(intersectionPrev1) >= 1e-6f
                )
                {
                    intersectionPrev = intersectionPrev0 / intersectionPrev1;
                }
                intersectionPrev = Mathf.Clamp01(intersectionPrev);
                // Debug.Assert(intersectionPrev >= 0);
                // if(intersectionPrev > 1)
                // {
                //     Debug.LogFormat(
                //         "line: {0}, d1 {1}, d2 {2}",
                //         origin.Vertices(prevIdx) - origin.Vertices(topIdx),
                //         -cuttingPlane.distance - Vector3.Dot(cuttingPlane.normal, origin.Vertices(topIdx)),
                //         Vector3.Dot(cuttingPlane.normal, origin.Vertices(prevIdx) - origin.Vertices(topIdx))
                //     );
                // }
                // Debug.Assert(intersectionPrev <= 1);
                Vector3 verticePrev = (1 - intersectionPrev) * origin.Vertices(topIdx) + intersectionPrev * origin.Vertices(prevIdx);
                Vector3 normPrev = ((1 - intersectionPrev) * origin.Norms(topIdx) + intersectionPrev * origin.Norms(prevIdx)).normalized;
                Vector2 uvPrev = (1 - intersectionPrev) * origin.UVs(topIdx) + intersectionPrev * origin.UVs(prevIdx);
                Vector4 tangentPrev = (1 - intersectionPrev) * origin.Tangents(topIdx) + intersectionPrev * origin.Tangents(prevIdx);

                int nextPosIdx = posSideInfo.AddVertice(verticeNext, normNext, uvNext, tangentNext);
                int prevPosIdx = posSideInfo.AddVertice(verticePrev, normPrev, uvPrev, tangentPrev);
                posEdgeIdx2NegEdgeIdx[nextPosIdx] = negSideInfo.AddVertice(verticeNext, normNext, uvNext, tangentNext);
                posEdgeIdx2NegEdgeIdx[prevPosIdx] = negSideInfo.AddVertice(verticePrev, normPrev, uvPrev, tangentPrev);
                if(sides[topIdx])
                {
                    posEdges.Add(prevPosIdx);
                    posEdges.Add(nextPosIdx);
                    topIdx = posSideInfo.AddVertice(topIdx, origin);
                    nextIdx = negSideInfo.AddVertice(nextIdx, origin);
                    prevIdx = negSideInfo.AddVertice(prevIdx, origin);
                    posSideInfo.AddTriangle(topIdx, nextPosIdx, prevPosIdx);
                    negSideInfo.AddTriangle(nextIdx, prevIdx, posEdgeIdx2NegEdgeIdx[prevPosIdx]);
                    negSideInfo.AddTriangle(nextIdx, posEdgeIdx2NegEdgeIdx[prevPosIdx], posEdgeIdx2NegEdgeIdx[nextPosIdx]);
                }
                // in negative side
                else
                {
                    posEdges.Add(nextPosIdx);
                    posEdges.Add(prevPosIdx);
                    topIdx = negSideInfo.AddVertice(topIdx, origin);
                    nextIdx = posSideInfo.AddVertice(nextIdx, origin);
                    prevIdx = posSideInfo.AddVertice(prevIdx, origin);
                    negSideInfo.AddTriangle(topIdx, posEdgeIdx2NegEdgeIdx[nextPosIdx], posEdgeIdx2NegEdgeIdx[prevPosIdx]);
                    posSideInfo.AddTriangle(nextIdx, prevIdx, prevPosIdx);
                    posSideInfo.AddTriangle(nextIdx, prevPosIdx, nextPosIdx);
                }
            }
        }

        List<int> orderedEdges = new List<int>();
        for(int posEdgeStartIdx = 0; posEdgeStartIdx < posEdges.Count - 2;)
        {
            orderedEdges.Clear();
            orderedEdges.Add(posEdges[posEdgeStartIdx]);
            orderedEdges.Add(posEdges[posEdgeStartIdx + 1]);
            Vector3 curEnd = posSideInfo.Vertices(posEdges[posEdgeStartIdx + 1]);
            int i = posEdgeStartIdx + 2;
            for(; i < posEdges.Count; i += 2)
            {
                int j = i;
                for(; j < posEdges.Count; j+=2)
                {
                    if((curEnd - posSideInfo.Vertices(posEdges[j])).magnitude < 1e-6)
                    {
                        curEnd = posSideInfo.Vertices(posEdges[j+1]);
                        orderedEdges.Add(posEdges[j+1]);
                        posEdges[j] = posEdges[i];
                        posEdges[j+1] = posEdges[i+1];
                        break;
                    }
                }
                if(j >= posEdges.Count)
                {
                    break;
                }
            }
            posEdgeStartIdx = i;

            if(orderedEdges.Count < 3) { continue; }
            int posOriginIdx0 = orderedEdges[0];
            int posOriginIdx1 = orderedEdges[1];
            int negOriginIdx0 = posEdgeIdx2NegEdgeIdx[posOriginIdx0];
            int negOriginIdx1 = posEdgeIdx2NegEdgeIdx[posOriginIdx1];
            int posCuttingEdgeIdx0 = posSideInfo.DuplicateVertice(posOriginIdx0);
            int posCuttingEdgeIdx1 = posSideInfo.DuplicateVertice(posOriginIdx1);
            posSideInfo.norms[posCuttingEdgeIdx0] = -cuttingPlane.normal;
            posSideInfo.norms[posCuttingEdgeIdx1] = -cuttingPlane.normal;
            int negCuttingEdgeIdx0 = negSideInfo.DuplicateVertice(negOriginIdx0);
            int negCuttingEdgeIdx1 = negSideInfo.DuplicateVertice(negOriginIdx1);
            negSideInfo.norms[negCuttingEdgeIdx0] = cuttingPlane.normal;
            negSideInfo.norms[negCuttingEdgeIdx1] = cuttingPlane.normal;
            for(i = 1; i < orderedEdges.Count - 1; i++)
            {
                int posOriginIdx2 = orderedEdges[i+1];
                int negOriginIdx2 = posEdgeIdx2NegEdgeIdx[posOriginIdx2];
                int posCuttingEdgeIdx2 = posSideInfo.DuplicateVertice(posOriginIdx2);
                posSideInfo.norms[posCuttingEdgeIdx2] = -cuttingPlane.normal;
                int negCuttingEdgeIdx2 = negSideInfo.DuplicateVertice(negOriginIdx2);
                negSideInfo.norms[negCuttingEdgeIdx2] = cuttingPlane.normal;
                posSideInfo.AddTriangle(posCuttingEdgeIdx0, posCuttingEdgeIdx1, posCuttingEdgeIdx2);
                negSideInfo.AddTriangle(negCuttingEdgeIdx0, negCuttingEdgeIdx2, negCuttingEdgeIdx1);
                posCuttingEdgeIdx1 = posCuttingEdgeIdx2;
                negCuttingEdgeIdx1 = negCuttingEdgeIdx2;
            }
        }

        if(posSideInfo.vertices.Count > 0)
        {
            result[0] = posSideInfo;
        }
        else
        {
            result[0] = null;
        }
        if(negSideInfo.vertices.Count > 0)
        {
            result[1] = negSideInfo;
        }
        else
        {
            result[1] = null;
        }
        return result;
    }
}
