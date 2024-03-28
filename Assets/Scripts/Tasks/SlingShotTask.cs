using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotTask : BaseTask
{
    List<float> targetAngles = new List<float>();
    GameObject slingshotPrefab;
    GameObject cursor;
    GameObject slingshotGameObject;
    Slingshot slingshot;
    GameObject target;
    Target targetScript;
    Camera slingshotCamera;

    //velocity mag to consider the shot ball has stopped
    const float BALL_LOW_VEL_THRES = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //grab the slingshot
            case 0:
                if (slingshot.IsGrabbed && slingshot.IsAwayFromHome)
                    IncrementStep();
                break;
            //pull back the slingshot and release
            case 1:
                if (!slingshot.IsLoaded)
                    IncrementStep();
                break;
            //track ball movement
            case 2:
                if (slingshot.SlingFired)
                {
                    //float ballFromSlingDist = Vector3.Distance(slingshot.ShotBall.transform.position, target.transform.position);
                    //Debug.Log(ballFromSlingDist);
                    if (slingshot.ShotBall.GetComponent<Rigidbody>().velocity.magnitude <= BALL_LOW_VEL_THRES)
                    {
                        slingshot.ReloadSlingshot();
                        IncrementStep();
                    }
                }
                break;
        }
    }

    public override void SetUp()
    {
        currentStep = 0;
        currentTrial = 0;
        totalTrials = expController.Session.CurrentBlock.trials.Count;
        maxSteps = 3;
        finished = false;

        slingshotPrefab = Instantiate(ExperimentController.Instance.Prefabs["SlingShotPrefab"]);
        slingshotPrefab.transform.position = Vector3.zero;

        slingshotCamera = GameObject.Find("SlingshotCamera").GetComponent<Camera>();
        dock = GameObject.Find("Dock");
        cursor = GameObject.Find("Cursor");
        CursorController.Instance.Cursor = cursor;
        slingshotGameObject = GameObject.Find("Slingshot");
        slingshot = slingshotGameObject.GetComponent<Slingshot>();
        target = GameObject.Find("Bullseye");

        if (target)
            targetScript = target.GetComponent<Target>();
        else
            Debug.LogWarning("NO TARGET FOUND");

        if (ExperimentController.Instance.UseVR == false)
        {
            Camera.SetupCurrent(slingshotCamera);
        }
        else
        {
            slingshotCamera.gameObject.SetActive(false);
        }
    }

    public override void TaskBegin()
    {
        currentStep = 0;
        finished = false;
        targetScript.ResetTarget();

        //if the target angles have not been set yet
        if (targetAngles.Count == 0)
        {
            targetAngles = expController.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }

        target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        target.transform.Translate(new Vector3(0.0f, 0.0f, 5.0f));
    }


    public override void TaskEnd()
    {
        Destroy(slingshotPrefab);
    }

    public override void LogParameters()
    {

    }
}
