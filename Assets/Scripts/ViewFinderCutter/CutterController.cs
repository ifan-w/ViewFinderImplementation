using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CutterController : MonoBehaviour
{
    public float PlaneDistance = 1.0f;
    public GameObject CuttingReference;
    public Camera PerspectiveCamera;

    public GameObject[] OnCut()
    {
        CuttableObjectBase[] cuttables = GameObject.FindObjectsOfType<CuttableObjectBase>();
        Tuple<Vector3, Vector3>[] cuttingPlanes = GenCuttingPlanesPerspective();
        List<GameObject> outputs = new List<GameObject>();
        foreach(CuttableObjectBase cuttable in cuttables)
        {
            Mesh[] result = cuttable.CutMesh(cuttingPlanes);
            var objs = HandleCuttedMeshesOnlyNeg(cuttable.gameObject, result[0], result[1]);
            if(objs != null)
            {
                foreach(var obj in objs)
                {
                    outputs.Add(obj);
                }
            }
        }
        return outputs.ToArray();
    }
    public Tuple<Vector3, Vector3>[] GenCuttingPlanes1()
    {
        Tuple<Vector3, Vector3>[] planes = new Tuple<Vector3, Vector3>[1];
        planes[0] = new Tuple<Vector3, Vector3>(
            CuttingReference.transform.up,
            CuttingReference.transform.position
        );
        return planes;
    }
    public Tuple<Vector3, Vector3>[] GenCuttingPlanesPerspective()
    {
        float width = Screen.width, height = Screen.height, depth = PerspectiveCamera.nearClipPlane;
        Debug.LogFormat("Width {0}, Height {1}", width, height);
        Vector3[] screenPoints = {
            PerspectiveCamera.ScreenToWorldPoint(new Vector3(width * 0.66f, height * 0.33f, depth)),
            PerspectiveCamera.ScreenToWorldPoint(new Vector3(width * 0.66f, height * 0.66f, depth)),
            PerspectiveCamera.ScreenToWorldPoint(new Vector3(width * 0.33f, height * 0.66f, depth)),
            PerspectiveCamera.ScreenToWorldPoint(new Vector3(width * 0.33f, height * 0.33f, depth))
        };
        Tuple<Vector3, Vector3>[] planes = new Tuple<Vector3, Vector3>[4];
        for(int i = 0; i < 4; i++)
        {
            Vector3 norm = Vector3.Cross(
                screenPoints[i] - CuttingReference.transform.position,
                screenPoints[(i + 1) % 4] - screenPoints[i]
            );
            planes[i] = new Tuple<Vector3, Vector3>(
                norm,
                CuttingReference.transform.position
            );
            // for(int j = 0; j < 10; j++)
            // {
            //     Debug.DrawLine(CuttingReference.transform.position, Vector3.Lerp(screenPoints[i], screenPoints[(i+1)%4], ((float)j) / 10), Color.green, 10.0f);
            // }
            // // Debug.DrawLine(CuttingReference.transform.position, screenPoints[i], Color.green, 10.0f);
            // Debug.DrawRay(
            //     Vector3.Lerp(screenPoints[i], screenPoints[(i+1)%4], 0.5f),
            //     norm * 10,
            //     Color.red,
            //     10.0f
            // );
            // Debug.DrawRay(CuttingReference.transform.position, CuttingReference.transform.forward * 10, Color.blue, 10.0f);
        }
        return planes;
    }
    public Tuple<Vector3, Vector3>[] GenCuttingPlanes4()
    {
        Tuple<Vector3, Vector3>[] planes = new Tuple<Vector3, Vector3>[4];
        planes[0] = new Tuple<Vector3, Vector3>(
            CuttingReference.transform.up,
            CuttingReference.transform.position - CuttingReference.transform.up * PlaneDistance
        );
        planes[1] = new Tuple<Vector3, Vector3>(
            CuttingReference.transform.right,
            CuttingReference.transform.position - CuttingReference.transform.right * PlaneDistance
        );
        planes[2] = new Tuple<Vector3, Vector3>(
            -CuttingReference.transform.up,
            CuttingReference.transform.position + CuttingReference.transform.up * PlaneDistance
        );
        planes[3] = new Tuple<Vector3, Vector3>(
            -CuttingReference.transform.right,
            CuttingReference.transform.position + CuttingReference.transform.right * PlaneDistance
        );
        return planes;
    }

    public GameObject[] HandleCuttedMeshesOnlyNeg(GameObject obj, Mesh posMesh, Mesh negMesh)
    {
        Debug.LogFormat("Cut Result: Pos {0}, Neg {1}", posMesh != null, negMesh != null);
        GameObject[] result = null;
        if(posMesh != null)
        {
            GameObject resultObj = Instantiate(obj);
            resultObj.GetComponent<MeshFilter>().sharedMesh = posMesh;
            resultObj.GetComponent<MeshCollider>().sharedMesh = posMesh;
            resultObj.SetActive(false);
            result = new GameObject[1];
            result[0] = resultObj;
        }
        if(negMesh != null)
        {
            GameObject cuttedObj = Instantiate(obj);
            cuttedObj.GetComponent<MeshFilter>().sharedMesh = negMesh;
            cuttedObj.GetComponent<MeshCollider>().sharedMesh = negMesh;
        }
        Destroy(obj);
        return result;
    }
    public GameObject[] HandleCuttedMeshesOnlyPos(GameObject obj, Mesh posMesh, Mesh negMesh)
    {
        Debug.LogFormat("Cut Result: Pos {0}, Neg {1}", posMesh != null, negMesh != null);
        GameObject[] result = null;
        if(negMesh != null)
        {
            GameObject resultObj = Instantiate(obj);
            resultObj.GetComponent<MeshFilter>().sharedMesh = negMesh;
            resultObj.GetComponent<MeshCollider>().sharedMesh = negMesh;
            resultObj.SetActive(false);
            result = new GameObject[1];
            result[0] = resultObj;
        }
        if(posMesh != null)
        {
            GameObject cuttedObj = Instantiate(obj);
            cuttedObj.GetComponent<MeshFilter>().sharedMesh = posMesh;
            cuttedObj.GetComponent<MeshCollider>().sharedMesh = posMesh;
        }
        Destroy(obj);
        return result;
    }
}
