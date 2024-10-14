using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectTransporterTask : BaseTask
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject xrHands;

    [SerializeField]
    GameObject objectResetPlane;

    [SerializeField]
    Target leftGoal;
    [SerializeField]
    Target rightGoal;
    [SerializeField]
    GameObject grabbedObject;

    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject leftHandCtrl;
    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject rightHandCtrl;

    [SerializeField]
    GameObject PrefabCamera;

    Vector3 homePos;

    float startTime = 0.0f;
    float endTime = 0.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbedObject.GetComponent<Tool>().IsGrabbed)
        {
            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbedObject.transform.position = new Vector3(cursor.transform.position.x, grabbedObject.transform.position.y, cursor.transform.position.z);
        }
        else
        {
            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        switch (currentStep)
        {
            //Check for initial grab, record time for start
            case 0:
                if (IsGrabbed())
                {
                    startTime = Time.time;
                    IncrementStep();
                }
                break;
            //Check for which goal hit
            case 1:
                {
                    if (leftGoal.TargetHit)
                    {
                        //Check if correct
                        endTime = Time.time;
                        dock.SetActive(true);
                        IncrementStep();
                    }
                    else if (rightGoal.TargetHit)
                    {
                        //Check if correct
                        endTime = Time.time;
                        dock.SetActive(true);
                        IncrementStep();
                    }
                }
                break;
            //Return to dock
            case 2:
                {
                    if (dock.GetComponent<Target>().TargetHit)
                    {
                        IncrementStep();
                    }
                }
                break;

        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        startTime = 0.0f;
        endTime = 0.0f;

        homePos = grabbedObject.transform.position;

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);
        xrHands.SetActive(ExperimentController.Instance.UseVR);

        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start

        SetupXR();
        leftGoal.ResetTarget();
        rightGoal.ResetTarget();
        dock.GetComponent<Target>().ResetTarget();
        dock.SetActive(false);

        grabbedObject.transform.position = homePos;
        grabbedObject.transform.rotation = Quaternion.identity;
    }

    void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<Tool>().enabled = false;
            //Enable XRInteractableScript
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
            cursor.SetActive(false);

            //Get which hand is being used
            rightHand.SetActive(true);
            rightHandCtrl.SetActive(false);
            leftHand.SetActive(false);
            leftHandCtrl.SetActive(false);

            objectResetPlane.SetActive(true);    // Comment out for now 
            dock.GetComponent<Target>().SetProjectile(rightHand);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

        }
        else
        {
            grabbedObject.GetComponent<Tool>().enabled = true;
            //Disable XRInteractableScript
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
            grabbedObject.transform.GetChild(0).gameObject.SetActive(false);
            cursor.SetActive(true);
            dock.GetComponent<Target>().SetProjectile(cursor);

            rightHand.SetActive(true);
            rightHandCtrl.SetActive(false);
            leftHand.SetActive(false);
            leftHandCtrl.SetActive(false);

            objectResetPlane.SetActive(false);
            dock.GetComponent<Target>().SetProjectile(cursor);

            //Switch Camera to 2D
            PrefabCamera.SetActive(true);
        }
    }

    bool IsGrabbed()
    {
        if (ExperimentController.Instance.UseVR)
        {
            return grabbedObject.GetComponent<XRGrabInteractable>().isSelected;
        }
        else
        {
            return grabbedObject.GetComponent<Tool>().IsGrabbed;
        }
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
