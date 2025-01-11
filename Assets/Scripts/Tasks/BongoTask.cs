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

public class BongoTask: BaseTask
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    List<Target> goals = new List<Target>();

    [SerializeField]
    List<MeshFilter> goalMeshes = new List<MeshFilter>();

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
    GameObject PrefabCamera;

    [SerializeField]
    GameObject MainCamera;

    [SerializeField]
    GameObject direct;

    [SerializeField]
    AudioClip correctSFX;
    [SerializeField]
    AudioClip incorrectSFX;
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
        if (ExperimentController.Instance.UseVR)
        {
          
        } else
        {
            Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
        }

        



        switch (currentStep)
        {
            //Check for initial grab, record time for start
            case 0:
                
                //startTime = Time.time;
                //trial_active = true;
                //IncrementStep();
                //stepTime.Add(Time.time);
                
                break;
            case 1:
                {
                    
                    
                    
                }
                break;

            case 2:
                {

                    break;
                }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;

        startTime = 0.0f;
        endTime = 0.0f;

        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        direct = GameObject.Find("RH Direct Interactor");

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
        stepTime.Clear();
        

       


        UpdateScoreboard(ExperimentController.Instance.Session.currentTrialNum, totalScore, endTime - startTime);

        foreach (Target t in goals)
        {
            t.ResetTarget();
        }
        
        //If not rotated
        
        foreach (Target t in goals)
        {
            t.SetProjectile(null);
        }

        

    

        /*
        leftGoal.ResetTarget();
        middleGoal.ResetTarget();
        rightGoal.ResetTarget();
        */

        

        

        
        //if (ExperimentController.Instance.UseVR) {
        //    rhCollider = rightHand.transform.GetChild(1).gameObject;
        //}


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

    void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            
            //rightHand = InputHandler.Instance.GetDominantHandGameObject();
            rightHand = GameObject.Find("Right Hand");
            direct = GameObject.Find("RH Direct Interactor");
            //dock.GetComponent<Target>().SetProjectile(direct);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

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
        session.CurrentTrial.result["rotation"] = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");

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
