using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
/// <summary>
/// Handles everything regarding the experiment (ex. moving to next trial, calls to generate blocks, setting up and ending the experiment)
/// </summary>
public class ExperimentController : MonoBehaviour
{
    private static ExperimentController instance = null;
    private ExperimentGenerator expGenerator = null;
    private Session session;

    //Dictionary of the lists for the experiment
    Dictionary<string, List<object>> expLists = new Dictionary<string, List<object>>();
    //List of the task objects that run the trials
    List<BaseTask> tasks = new List<BaseTask>();
    public GameObject[] scenePrefabs;
    Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    //The active 
    BaseTask currentTask;
    //The total number of trials for the experiment
    int totalNumOfTrials = 0;
    //The totla number of blocks
    int totalNumOfBlocks = 0;
    //Is the experiment using VR
    bool useVR = false;
    //Has the task been setup yet
    bool taskReady = false;
    bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
            instance = this;

        expGenerator = new ExperimentGenerator();
    }

    // Update is called once per frame
    void Update()
    {
        // when the experiment is running
        // this is here because the experimentcontroller object exists before the
        // experiment starts and the parameters are made
        if (isRunning)
        {
            if (currentTask.Finished)
            {
                session.EndCurrentTrial();
            }
        }
    }

    public static ExperimentController Instance
    {
        get
        {
            if (!instance)
                Debug.LogWarning("ExperimentController is unitialized");
            
            return instance; 
        }
    }

    public List<BaseTask> Tasks
    {
        get { return tasks; }
    }

    public Session Session
    {
        get { return session; }
    }

    public ExperimentGenerator ExperimentGenerator
    {
        get { return expGenerator; }
    }


    public Dictionary<string, List<object>> ExperimentLists
    {
        get { return expLists; }
    }


    public int TotalNumOfTrials
    {
        get { return totalNumOfTrials; }
        set { totalNumOfTrials = value; }
    }

    public int TotalNumOfBlocks
    {
        get { return totalNumOfBlocks; }
        set { totalNumOfBlocks = value; }
    }

    public bool UseVR
    {
        get { return useVR; }
        set { useVR = value; }
    }

    public Dictionary<string,GameObject> Prefabs
    {
        get { return prefabs; }
    }

    public void SessionBegin(Session session)
    {
        this.session = session;

        expGenerator.GenerateBlocks(session);
        tasks.Capacity = TotalNumOfTrials;
        expGenerator.GenerateTasks();

        // disable VR prefab if not using VR
        if (useVR == false)
            GameObject.Find("OVRPlayerController").SetActive(false);

        //define the scene prefabs
        foreach(GameObject g in scenePrefabs)
        {
            prefabs[g.name] = g;
        }

        currentTask = tasks[0];
        currentTask.enabled = true;
        isRunning = true;

        BeginNextTrial();
    }

    public void BeginNextTrial()
    {
        //for the first trial
        if (session.currentTrialNum == 0)
            session.FirstTrial.Begin();
        //every other trial
        else if (session.currentTrialNum < totalNumOfTrials)
            session.BeginNextTrialSafe();
        else if (session.currentTrialNum == totalNumOfTrials) 
        {
            isRunning = false;
            session.End();
        }
    }

    public void TrialBegin()
    {
        //if the task hasn't been setup yet
        if (!taskReady)
        {
            currentTask.enabled = true;
            currentTask.SetUp();
            taskReady = true;
        }
        currentTask.TaskBegin();
    }

    public void TrialEnd()
    {
        //TODO Log Parameters

        //if this is the last trial of the block
        if (session.CurrentTrial == session.CurrentBlock.lastTrial)
        {
            //end current task and disable
            currentTask.TaskEnd();
            currentTask.enabled = false;

            //if there are more blocks to go through
            if (session.CurrentBlock.number < tasks.Count)
            {
                //move on to the next task and prepare it
                currentTask = tasks[session.CurrentBlock.number];
                taskReady = false;
            }
        }

        BeginNextTrial();
    }

    public void OnSessionEnd()
    {
        Application.Quit();
    }

}
