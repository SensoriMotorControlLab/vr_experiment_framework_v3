using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneObjToObj : MonoBehaviour
{
    [SerializeField]
    Plane plane;
    [SerializeField]
    GameObject planeGO;
    MeshFilter planeMesh;
    [SerializeField]
    GameObject startGO;
    MeshFilter startMesh;
    [SerializeField]
    GameObject endGO;
    MeshFilter endMesh;

    // Start is called before the first frame update
    void Start()
    {
        if (!planeGO)
            Debug.Log("Plane not set");
        if (!startGO)
            Debug.Log("Start point GameObject not set");
        if (!endGO)
            Debug.Log("End point GameObject not set");

        planeMesh = planeGO.GetComponent<MeshFilter>();
        startMesh = startGO.GetComponent<MeshFilter>();
        endMesh = endGO.GetComponent<MeshFilter>();

        endGO.transform.localScale = startGO.transform.localScale;
        endGO.transform.position = new Vector3(endGO.transform.position.x, startGO.transform.position.y, endGO.transform.position.z);

        Vector3 startToEnd = endGO.transform.position - startGO.transform.position;
        float dist = Vector3.Distance(startGO.transform.position, endGO.transform.position);
        Vector3 startToEndDir = startToEnd.normalized;
        Vector3 halfWayPoint = startGO.transform.position + (startToEnd / 2);

        Quaternion lookRotation = Quaternion.LookRotation(startToEndDir, planeGO.transform.up);

        planeGO.transform.position = halfWayPoint;
        planeGO.transform.rotation = lookRotation;
        planeGO.transform.localScale = new Vector3(planeGO.transform.localScale.x * (startMesh.mesh.bounds.size.y * startGO.transform.localScale.y), planeGO.transform.localScale.y, dist * planeGO.transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
