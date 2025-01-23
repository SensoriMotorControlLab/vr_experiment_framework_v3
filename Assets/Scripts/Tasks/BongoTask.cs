using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UXF;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

public class BongoTask: BaseTask
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    List<MultipleTarget> goals = new List<MultipleTarget>();

    [SerializeField]
    List<MeshFilter> goalMeshes = new List<MeshFilter>();
    [SerializeField]
    List<Material> targetMaterials = new List<Material>();
    [SerializeField]
    GameObject spawnParent;
    [SerializeField]
    List<GameObject> spawnLocations = new List<GameObject>();

    List<float> stepTime = new List<float>();
    /*
    [SerializeField]
    Target leftGoal;
    [SerializeField]
    Target rightGoal;
    [SerializeField]
    Target middleGoal;
    */

    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject leftHandCtrl;
    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject rightHandCtrl;
    [SerializeField]
    GameObject bongoTargetPrefab;
    [SerializeField]
    Target targetOutOfBounds;

    Queue<GameObject> activeTargets = new Queue<GameObject>();
    Queue<GameObject> spawnedObjects = new Queue<GameObject>();

    GameObject leftOuterTarget;
    GameObject leftInnerTarget;
    GameObject rightInnerTarget;
    GameObject rightOuterTarget;

    List<string> jsonSpawnLocation = new List<string>();

    float targetSpeed = 0.0003f;
    float targetSpawnDelay = 1.0f;

    [SerializeField]
    GameObject PrefabCamera;

    [SerializeField]
    GameObject MainCamera;

    [SerializeField]
    GameObject directRight;
    [SerializeField]
    GameObject directLeft;

    [SerializeField]
    AudioClip LO_Bongo;
    [SerializeField]
    AudioClip LI_Bongo;
    [SerializeField]
    AudioClip RI_Bongo;
    [SerializeField]
    AudioClip RO_Bongo;
    [SerializeField]
    AudioClip buttonClickSFX;

    [SerializeField]
    GameObject Scoreboard;
    [SerializeField]
    TextMeshProUGUI ScoreTXT;
    [SerializeField]
    TextMeshProUGUI TrialTXT;
    [SerializeField]
    TextMeshProUGUI TimeTXT;


    private bool trial_active = false;
    private string tool_x = ""; // String to store all X positions instead of list to solve log parameter issues 
    private string tool_y = ""; 
    private string tool_z = ""; 



    float startTime = 0.0f;
    float endTime = 0.0f;

    int toolType = 0;
    int goalType = 0;
    static int totalScore = 0;

    bool hitTarget = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //Check if dock is pressed to start trial
            case 0:
                {
                    if (dock.GetComponent<Target>().TargetHit && dock.GetComponent<Target>().IsColliding && 
                        (ExperimentController.Instance.UseVR == true ? true : Input.GetMouseButtonDown(0)))
                    {
                        dock.GetComponent<Target>().ResetTarget();
                        audioSource.clip = buttonClickSFX;
                        audioSource.Play();
                        dock.GetComponent<Target>().enabled = false;
                        dock.GetComponent<MeshCollider>().enabled = false;
                        dock.SetActive(false);

                        StartCoroutine(PlayFeedback(0.5f));

                        SpawnTargets();

                        Debug.Log("Button is pressed");
                    }
                }
                break;
            //Start moving objects
            case 1:
                {
                    StartCoroutine(MoveTargets());
                    IncrementStep();
                }
                break;
            //Check if all targets are active and for button click
            case 2:
                {
                    //Move targets into negative Z-axis
                    foreach(GameObject g in activeTargets)
                    {
                        Vector3 pos = g.transform.position;
                        g.transform.position = new Vector3(pos.x, pos.y, pos.z -= targetSpeed);
                    }

                    //Check if bongo is pressed
                    foreach(MultipleTarget g in goals)
                    {
                        if(g.IsToolCollding && g.IsTargetCollding && 
                            (ExperimentController.Instance.UseVR == true ? true : Input.GetMouseButtonDown(0)))
                        {
                            GameObject hitTarget = g.CollidingTarget;
                            g.targets.Remove(hitTarget);
                            Destroy(hitTarget);
                            g.ResetState();
                            activeTargets.Dequeue();
                            //TODO tally score
                        }
                    }

                    if (targetOutOfBounds.TargetHit || targetOutOfBounds.IsColliding)
                    {
                        GameObject o = activeTargets.Dequeue();
                        Destroy(o);
                        targetOutOfBounds.ResetTarget();
                    }

                    if(activeTargets.Count == 0 && spawnedObjects.Count == 0)
                    {
                        //dock.SetActive(true);
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

        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        directRight = GameObject.Find("RH Direct Interactor");
        directLeft = GameObject.Find("LH Direct Interactor");


        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");

        if (ExperimentController.Instance.UseVR)
        {
            dock.GetComponent<Target>().SetProjectile(directRight);

            foreach(MultipleTarget g in goals)
            {
                g.tools.Add(directRight);
                g.tools.Add(directLeft);
            }
        }
        else
        {
            dock.GetComponent<Target>().SetProjectile(cursor);
            CursorController.Instance.planeOffset = new Vector3(0.0f, -spawnParent.transform.position.y, 0.0f);

            foreach (MultipleTarget g in goals)
            {
                g.tools.Add(cursor);
            }
        }

        MainCamera = GameObject.Find("Main Camera");
        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start
        stepTime.Clear();
        UpdateScoreboard(ExperimentController.Instance.Session.currentTrialNum, totalScore, endTime - startTime);

        dock.SetActive(true);
        dock.GetComponent<Target>().enabled = true;
        dock.GetComponent<MeshCollider>().enabled = true;
        dock.GetComponent<Target>().ResetTarget();

        foreach (MultipleTarget t in goals)
        {
            t.ResetState();
        }

        targetOutOfBounds.ResetTarget();
        
        foreach (MultipleTarget g in goals)
        {
            g.targets.Clear();
        }
        if (jsonSpawnLocation.Count == 0)
        {
            jsonSpawnLocation = ExperimentController.Instance.Session.CurrentBlock.settings.GetStringList("target_location");
        }

        //List<int> currentBlockTrials = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("trials_in_block");
        //int currentBlockNum = ExperimentController.Instance.Session.currentBlockNum - 1;
        //string currentBlockLoc = jsonSpawnLocation[(ExperimentController.Instance.Session.currentTrialNum - 1) % currentBlockTrials[currentBlockNum]];

        //if(currentBlockLoc.Length != goals.Count)
        //{
        //    Debug.LogError("The number of locations in the JSON is not the same as the number of goals");
        //}

        ////Spawn all the targets
        //foreach(char c in currentBlockLoc)
        //{
        //    int val = int.Parse(c.ToString());

        //    GameObject t = Instantiate(bongoTargetPrefab);
        //    t.name = "Bongo Target " + c;
        //    t.transform.position = spawnLocations[val - 1].transform.position;
        //    t.GetComponent<MeshRenderer>().material = targetMaterials[val - 1];
        //    t.GetComponent<MeshRenderer>().enabled = false;

        //    spawnedObjects.Enqueue(t);
        //}



        if (!ExperimentController.Instance.UseVR) // scoreboard direction
        {
            Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
        }
    }

    IEnumerator PlayFeedback(float endDelayTime = 0.0f)
    {
        float delayTime = 0.0f;

        while (delayTime <= endDelayTime)
        {
            delayTime += Time.deltaTime;
            yield return null;
        }

        IncrementStep();
        yield return new WaitForEndOfFrame();
    }

    IEnumerator MoveTargets()
    {
        float delayTime = 0.0f;

        while (spawnedObjects.Count > 0)
        {
            while (delayTime <= targetSpawnDelay)
            {
                delayTime += Time.deltaTime;
                yield return null;
            }

            GameObject obj = spawnedObjects.Dequeue();
            obj.GetComponent<MeshRenderer>().enabled = true;
            activeTargets.Enqueue(obj);
            delayTime = 0.0f;
        }

        yield return new WaitForEndOfFrame();
    }


    private void SpawnTargets()
    {
        List<int> currentBlockTrials = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("trials_in_block");
        int currentBlockNum = ExperimentController.Instance.Session.currentBlockNum - 1;
        string currentBlockLoc = jsonSpawnLocation[(ExperimentController.Instance.Session.currentTrialNum - 1) % currentBlockTrials[currentBlockNum]];

        if (currentBlockLoc.Length != goals.Count)
        {
            Debug.LogError("The number of locations in the JSON is not the same as the number of goals");
        }

        foreach (char c in currentBlockLoc)
        {
            int val = int.Parse(c.ToString());

            GameObject t = Instantiate(bongoTargetPrefab,gameObject.transform);
            t.name = "Bongo Target " + c;
            t.transform.position = spawnLocations[val - 1].transform.position;
            t.GetComponent<MeshRenderer>().material = targetMaterials[val - 1];
            t.GetComponent<MeshRenderer>().enabled = false;
            goals[val - 1].targets.Add(t);

            spawnedObjects.Enqueue(t);
        }
    }

    void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            
            //rightHand = InputHandler.Instance.GetDominantHandGameObject();
            rightHand = GameObject.Find("Right Hand");
            directRight = GameObject.Find("RH Direct Interactor");
            leftHand = GameObject.Find("Left Hand");
            directLeft = GameObject.Find("LH Direct Interactor");
            //dock.GetComponent<Target>().SetProjectile(direct);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

            cursor.SetActive(false);

            // Centers player
            ExperimentController.Instance.CentreOVRPlayerHand();
        }
        else
        {
            
            cursor.SetActive(true);
            //dock.GetComponent<Target>().SetProjectile(cursor);


            //dock.GetComponent<Target>().SetProjectile(cursor);

            //Switch Camera to 2D
            PrefabCamera.SetActive(true);
            if (MainCamera != null)
            {
                MainCamera.SetActive(false);
            }
        }
    }

    

    public override void TaskEnd()
    {
        //clean up
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;


        if (ExperimentController.Instance.UseVR)
        {
            session.CurrentTrial.result["hand"] = "r";
            session.CurrentTrial.result["cursor_active"] = "N/A";
        }
        else
        {
            session.CurrentTrial.result["hand"] = "N/A";
            session.CurrentTrial.result["cursor_active"] = "True";
        }

        session.CurrentTrial.result["tool_x_coordinates"] = tool_x;
        session.CurrentTrial.result["tool_y_coordinates"] = tool_y;
        session.CurrentTrial.result["tool_z_coordinates"] = tool_z;



        session.CurrentTrial.result["correct_target"] = hitTarget;

        if (toolType == 1)
        {
            session.CurrentTrial.result["tool_type"] = "cube";
        }
        else
        {
            session.CurrentTrial.result["tool_type"] = "sphere";
        }
        
        if (goalType == 1)
        {
            session.CurrentTrial.result["goal_type"] = "cube";
        }
        else
        {
            session.CurrentTrial.result["goal_type"] = "sphere";
        }
        session.CurrentTrial.result["total_score"] = totalScore;
        session.CurrentTrial.result["start_grabbed_time"] = startTime;
        session.CurrentTrial.result["goal_hit_time"] = endTime;
        session.CurrentTrial.result["total_time"] = (endTime - startTime);




        for (int i = 0; i < stepTime.Count; i++)
        {
            session.CurrentTrial.result["step_" + i + "_time"] = stepTime[i];
        }
    }

    private void UpdateScoreboard(int trialNumber, int score, float totalTime)
    {
        int seconds = Mathf.FloorToInt(totalTime);
        int milliseconds = Mathf.FloorToInt((totalTime - seconds) * 1000);

        // Format the time as "Seconds:Milliseconds"
        string formattedTime = $"{seconds:00}.{milliseconds:000}";

        // Find child objects (requires proper hierarchy structure)
        TextMeshProUGUI scoreText = Scoreboard.transform.Find("ScoreTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trialText = Scoreboard.transform.Find("TrialTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeText = Scoreboard.transform.Find("TimeTXT").GetComponent<TextMeshProUGUI>();

        // Update the Text fields
        scoreText.text = $"Score: {score}";
        trialText.text = $"Trial: {trialNumber}";
        timeText.text = $"Time: {formattedTime}";
    }
}
