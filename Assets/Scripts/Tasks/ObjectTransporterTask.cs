using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ObjectTransporterTask : BaseTask
{
    [SerializeField]
    GameObject objectResetPlane;

    [SerializeField]
    Target leftGoal;
    [SerializeField]
    Target rightGoal;
    [SerializeField]
    GameObject grabbedObject;
    [SerializeField]
    GameObject toolPrefab;

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

    [SerializeField]
    GameObject MainCamera;

    [SerializeField]
    GameObject direct;
    Vector3 homePos;

    float startTime = 0.0f;
    float endTime = 0.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
                    if (!ExperimentController.Instance.UseVR)
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
                    }
                    if (leftGoal.TargetHit)
                    {
                        //Check if correct
                        endTime = Time.time;
                        dock.SetActive(true);
                        grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                        if(ExperimentController.Instance.UseVR)
                        {
                            grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
                            grabbedObject.GetComponent<XRBaseGrabTransformer>().enabled = false;
                        }
                        IncrementStep();
                    }
                    else if (rightGoal.TargetHit)
                    {
                        //Check if correct
                        endTime = Time.time;
                        dock.SetActive(true);
                        grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                        if(ExperimentController.Instance.UseVR)
                        {
                            grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
                            grabbedObject.GetComponent<XRBaseGrabTransformer>().enabled = false;
                        }
                        IncrementStep();
                    }
                }
                break;
            //Return to dock
            case 2:
                {
                    if (ExperimentController.Instance.UseVR)
                    {
                        // VR Mode: Check if hands are close enough to the dock
                        //float leftHandDistance = Vector3.Distance(leftHand.transform.position, dock.transform.position);
                        //float rightHandDistance = Vector3.Distance(rightHand.transform.position, dock.transform.position);

                        //Debug.Log("Left Hand Distance: " + leftHandDistance);
                        //Debug.Log("Right Hand Distance: " + rightHandDistance);

                        // Check if either hand is within the dock's proximity (0.1f threshold)
                        Target dockTarget = dock.GetComponent<Target>();

                        if (dockTarget.IsColliding && dockTarget.TargetHit)
                        {
                            Debug.Log("Dock TargetHit: " + dockTarget.TargetHit);
                            Debug.Log("Dock IsColliding: " + dockTarget.IsColliding);
                            Destroy(grabbedObject);
                            IncrementStep();
                        }
                    }
                    else
                    {
                        // Non-VR Mode: Check if the dock's Target has been hit
                        Target dockTarget = dock.GetComponent<Target>();

                        // Log whether the dock was hit and if it's still colliding
                        Debug.Log("Dock TargetHit: " + dockTarget.TargetHit);
                        Debug.Log("Dock IsColliding: " + dockTarget.IsColliding);

                        // Proceed if the dock's target has been hit and is still colliding
                        if (dockTarget.TargetHit && dockTarget.IsColliding)
                        {
                            Debug.Log("Cursor hit the dock.");
                            Destroy(grabbedObject);
                            IncrementStep();
                        }
                    }

                    break;
                }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        startTime = 0.0f;
        endTime = 0.0f;

        homePos = grabbedObject.transform.position;
        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        direct = GameObject.Find("Direct Interactor");

        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");

        MainCamera = GameObject.Find("Main Camera");

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);

        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start

        if(grabbedObject == null)
        {
            grabbedObject = Instantiate(toolPrefab, homePos, Quaternion.identity);
            leftGoal.GetComponent<Target>().SetProjectile(grabbedObject);
            rightGoal.GetComponent<Target>().SetProjectile(grabbedObject);
        }

        //SetupXR();
        grabbedObject.transform.position = homePos;
        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        grabbedObject.transform.rotation = Quaternion.identity;

        leftGoal.ResetTarget();
        rightGoal.ResetTarget();
        dock.GetComponent<Target>().ResetTarget();
        dock.SetActive(false);
        if(ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
        }


        //if (ExperimentController.Instance.UseVR) {
        //    rhCollider = rightHand.transform.GetChild(1).gameObject;
        //}


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
            //rightHand.SetActive(true);
            //rightHandCtrl.SetActive(false);
            //leftHand.SetActive(false);
            //leftHandCtrl.SetActive(false);

            objectResetPlane.SetActive(true);    // Comment out for now 
            //rightHand = InputHandler.Instance.GetDominantHandGameObject();
            rightHand = GameObject.Find("Right Hand");
            direct = GameObject.Find("Direct Interactor");
            dock.GetComponent<Target>().SetProjectile(direct);

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


            objectResetPlane.SetActive(false);
            dock.GetComponent<Target>().SetProjectile(cursor);

            //Switch Camera to 2D
            PrefabCamera.SetActive(true);
            if (MainCamera != null)
            {
                MainCamera.SetActive(false);
            }
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
