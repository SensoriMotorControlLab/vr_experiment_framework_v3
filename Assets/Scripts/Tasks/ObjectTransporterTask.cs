using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UXF;

public class ObjectTransporterTask : BaseTask
{
    [SerializeField]
    GameObject objectResetPlane;

    [SerializeField]
    List<Target> goals = new List<Target>();
    [SerializeField]
    Target resetPlane;
    [SerializeField]
    List<MeshFilter> goalMeshes = new List<MeshFilter>();

    List<string> goalMeshesVal = new List<string>();

    /*
    [SerializeField]
    Target leftGoal;
    [SerializeField]
    Target rightGoal;
    [SerializeField]
    Target middleGoal;
    */
    [SerializeField]
    MeshFilter SquareGoalMesh;

    [SerializeField]
    MeshFilter SphereGoalMesh;

    [SerializeField]
    GameObject grabbedObject;
    [SerializeField]
    GameObject grabbedObjectVisable;

    [SerializeField]
    MeshFilter toolMesh;

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

    private int session_count = 0; // var for counting the centering function each new block

    float startTime = 0.0f;
    float endTime = 0.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.Instance.UseVR)
        {
            if (resetPlane.TargetHit)
            {
                Debug.Log("Collided with reset plane");
                grabbedObject.transform.position = homePos;
                resetPlane.ResetTarget();
            }
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

                    foreach(Target t in goals)
                    {
                        if (t.TargetHit)
                        {
                            //Check if correct
                            endTime = Time.time;
                            dock.SetActive(true);
                            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                            if (ExperimentController.Instance.UseVR)
                            {
                                grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
                                grabbedObject.GetComponent<XRBaseGrabTransformer>().enabled = false;
                            }
                            IncrementStep();
                        }
                    }
                    
                    float rotation = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");
                    if (rotation != 0)
                    {
                        grabbedObjectVisable.transform.position = Quaternion.Euler(0, -rotation, 0) * (grabbedObject.transform.position - homePos) + homePos;
                        grabbedObjectVisable.transform.rotation = grabbedObject.transform.rotation;
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
        direct = GameObject.Find("RH Direct Interactor");

        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");

        MainCamera = GameObject.Find("Main Camera");

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);

        if (goalMeshesVal.Count == 0)
        {
            goalMeshesVal = ExperimentController.Instance.Session.CurrentBlock.settings.GetStringList("target_location");
        }

        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start

        if(grabbedObject == null)
        {
            grabbedObject = Instantiate(toolPrefab, homePos, Quaternion.identity);
            
        }

        //SetupXR();
        grabbedObject.transform.position = homePos;
        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        grabbedObject.transform.rotation = Quaternion.identity;

        grabbedObjectVisable.transform.position = homePos;
        grabbedObjectVisable.transform.rotation = grabbedObject.transform.rotation;

        foreach (Target t in goals)
        {
            t.ResetTarget();
        }
        resetPlane.ResetTarget();
        resetPlane.SetProjectile(grabbedObject);

        float rotation = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");

        //If not rotated
        if (rotation == 0)
        {
            foreach (Target t in goals)
            {
                t.SetProjectile(grabbedObject);
            }

            grabbedObjectVisable.SetActive(false);
        }

        //If rotated
        else
        {

            grabbedObjectVisable.SetActive(true);

            foreach (Target t in goals)
            {
                t.SetProjectile(grabbedObjectVisable);
            }

        }

        /*
        leftGoal.ResetTarget();
        middleGoal.ResetTarget();
        rightGoal.ResetTarget();
        */

        foreach (MeshFilter m in goalMeshes)
        {
            m.sharedMesh = null;
        }

        dock.GetComponent<Target>().ResetTarget();
        dock.SetActive(false);

        string valString = goalMeshesVal[(ExperimentController.Instance.Session.currentTrialNum  - 1) % 4];

        int counter = 0;
        foreach(char c in valString)
        {
            int v = int.Parse(c.ToString());

            if (v == 1)
            {
                goalMeshes[counter].GetComponent<MeshFilter>().sharedMesh = SquareGoalMesh.sharedMesh;
            }
            else if(v == 2)
            {
                goalMeshes[counter].GetComponent<MeshFilter>().sharedMesh = SphereGoalMesh.sharedMesh;
            }
            else if(v == 0)
            {
                goals[counter].SetProjectile(null);
            }

            counter++;
        }

        if(ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
        }

        //Setting mesh for grabbed object and visual object
        toolMesh = grabbedObject.GetComponent<MeshFilter>();
        string meshName = (string) ExperimentController.Instance.ExperimentLists["mesh"][ExperimentController.Instance.Session.currentBlockNum-1];
        switch (meshName) 
            {
            case "Cube":
                toolMesh.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                grabbedObjectVisable.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                grabbedObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                grabbedObjectVisable.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                break;

            case "Sphere":
                toolMesh.sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                grabbedObjectVisable.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                grabbedObject.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
                grabbedObjectVisable.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

                break;

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
            direct = GameObject.Find("RH Direct Interactor");
            dock.GetComponent<Target>().SetProjectile(direct);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

            // Centers player
            //ExperimentController.Instance.CentreOVRPlayerHand();
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
