using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShotTask : BaseTask
{
    GameObject slingShotPrefab;
    GameObject cursor;
    GameObject sling;
    Camera slingShotCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void SetUp()
    {
        slingShotPrefab = Instantiate(ExperimentController.Instance.Prefabs["SlingShotPrefab"]);
        slingShotPrefab.transform.position = Vector3.zero;

        slingShotCamera = GameObject.Find("SlingShotCamera").GetComponent<Camera>();
        cursor = GameObject.Find("Cursor");
        sling = GameObject.Find("Cube");


        if (ExperimentController.Instance.UseVR == false)
        {
            Camera.SetupCurrent(slingShotCamera);
            CursorController.Instance.Cursor = cursor;
        }
    }

    public override void TaskBegin()
    {

    }


    public override void TaskEnd()
    {

    }

    public override void LogParameters()
    {

    }
}
