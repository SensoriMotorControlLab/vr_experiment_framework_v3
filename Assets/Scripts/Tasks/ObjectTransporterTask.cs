using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTransporterTask : BaseTask
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject xrHands;
    
    [SerializeField] 
    List<Target> goalChecks = new List<Target>();
    [SerializeField]
    GameObject grabbedObject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);
        xrHands.SetActive(ExperimentController.Instance.UseVR);

        if (ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<Tool>().enabled = false;
            //Enable XRInteractableScript
            //grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
            cursor.SetActive(false);
        }
        else
        {
            grabbedObject.GetComponent<Tool>().enabled = true;
            //Disable XRInteractableScript
            //grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
            cursor.SetActive(true);
        }
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start
    }
    
    public override void TaskEnd()
    {
        //clean up
        base.TaskEnd();
    }

    public override void LogParameters()
    {

    }
}
