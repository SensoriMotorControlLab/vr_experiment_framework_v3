using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
/// <summary>
/// Reach task
/// </summary>
public class ReachTask : BaseTask
{
    List<float> targetAngles = new List<float>();
    GameObject reachPrefab, reachSurface;
    Camera reachCamera;
    GameObject target;
    GameObject cursor;

    const float DOCK_DIST = 0.025f;

    public ReachTask()
    {
        taskType = "reach_to_target";
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //reach to dock
            case 0:
                if (Vector3.Distance(cursor.transform.position,dock.transform.position) <= 0.01f)
                {
                    dock.SetActive(false);
                    home.SetActive(true);
                    IncrementStep();
                }

                break;
            //reach to home
            case 1:
                if (Vector3.Distance(cursor.transform.position, home.transform.position) <= 0.01f)
                {
                    home.SetActive(false);
                    target.SetActive(true);
                    IncrementStep();
                }
                break;
            //reach to target
            case 2:
                if (Vector3.Distance(cursor.transform.position, target.transform.position) <= 0.01f)
                {
                    target.SetActive(false);
                    IncrementStep();
                    //expController.Session.EndCurrentTrial();
                }
                break;
        }
    }

    public override void LogParameters()
    {
        Session session = expController.Session;

        session.CurrentTrial.result["type"] = session.CurrentBlock.settings.GetString("task");
        session.CurrentTrial.result["home_pos"] = home.transform.position;
        session.CurrentTrial.result["target_angle"] = session.CurrentBlock.settings.GetFloatList("target_angle")[session.currentTrialNum-1];
        session.CurrentTrial.result["target_size_m"] = target.transform.localScale.x;
        session.CurrentTrial.result["cursor_pos"] = cursor.transform.position;
    }

    public override void SetUp()
    {
        currentStep = 0;
        currentTrial = 0;
        totalTrials = expController.Session.CurrentBlock.trials.Count;
        maxSteps = 3;
        finished = false;

        //create prefab and zero it
        reachPrefab = Instantiate(expController.Prefabs["ReachPrefab"],expController.transform);
        reachPrefab.transform.position = Vector3.zero;

        reachCamera = GameObject.Find("ReachCamera").GetComponent<Camera>();

        if (ExperimentController.Instance.UseVR == false)
        {
            Camera.SetupCurrent(reachCamera);
        }
        else
        {
            reachCamera.gameObject.SetActive(false);
        }
        reachSurface = GameObject.Find("ReachSurface");
        cursor = GameObject.Find("Cursor");
        CursorController.Instance.Cursor = cursor;
        home = GameObject.Find("Home");
        dock = GameObject.Find("Dock");
        target = GameObject.Find("Target");

        //set up dock position and hide it
        dock.transform.position = reachPrefab.transform.position - expController.transform.forward * DOCK_DIST;
        dock.SetActive(false);

        //set up home position and hide it
        home.transform.position = reachPrefab.transform.position;
        home.SetActive(false);

        //set up target position and hide it
        target.transform.position = reachPrefab.transform.position;
        target.SetActive(false);

        reachPrefab.transform.position = new Vector3(reachPrefab.transform.position.x,CursorController.Instance.GetHandPosition().y, reachPrefab.transform.position.z);
    }

    public override void TaskBegin()
    {
        currentStep = 0;
        finished = false;
        target.transform.position = Vector3.zero;

        //if the target angles have not been set yet
        if(targetAngles.Count == 0)
        {
            targetAngles = expController.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }

        target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        target.transform.Translate(new Vector3(0.0f, 0.0f, 0.075f));
        dock.SetActive(true);
    }

    public override void TaskEnd()
    {
        Destroy(reachPrefab);
    }

    
}
