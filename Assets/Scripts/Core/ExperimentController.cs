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

    /// <summary>
    ///Dictionary of the lists for the experiment
    /// </summary>
    Dictionary<string, List<object>> expLists = new Dictionary<string, List<object>>();
    /// <summary>
    /// List of the task objects that run the trials
    /// </summary>
    List<BaseTask> tasks = new List<BaseTask>();
    /// <summary>
    /// The prefabs for the scene to be instanced
    /// </summary>
    public GameObject[] scenePrefabs;
    /// <summary>
    /// Dictionary of scene prefabs, key is prefab name defined in scenePrefabs
    /// </summary>
    Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    /// <summary>
    /// The VR controller
    /// </summary>
    public GameObject vrCtlr;
    /// <summary>
    /// The prefab to spawn if using VR
    /// </summary>
    public GameObject vrPrefab;
    /// <summary>
    /// The active task
    /// </summary>
    BaseTask currentTask;
    //The total number of trials for the experiment
    int totalNumOfTrials = 0;
    //The totla number of blocks
    int totalNumOfBlocks = 0;
    //Is the experiment using VR
    bool useVR = false;
    //Has the task been setup yet
    bool taskReady = false;
    //Is the experiment running
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
        // experiment starts and the parameters are made in UXF
        if (isRunning)
        {
            if (currentTask.Finished)
            {
                session.EndCurrentTrial();
            }


            if (Input.GetKey(KeyCode.J))
            {
                CenterOVRPlayerController();
            }
        }
    }

    public static ExperimentController Instance
    {
        get
        {
            if (!instance)
                Debug.LogError("ExperimentController is unitialized");
            
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
    /// <summary>
    /// UXF SessionBegin method
    /// </summary>
    public void SessionBegin(Session session)
    {
        this.session = session;

        expGenerator.GenerateBlocks(session);
        tasks.Capacity = TotalNumOfTrials;
        expGenerator.GenerateTasks();

        // if using VR create the VR controller
        if (useVR == true)
        {
            Debug.Log("The experiment is being run in VR");
            //vrCtlr = Instantiate(vrPrefab);
            //Camera.SetupCurrent(GameObject.Find("CenterEyeAnchor").GetComponent<Camera>());
            Camera.SetupCurrent(GameObject.Find("Main Camera").GetComponent<Camera>());
            //Debug.Log(Camera.main);
            InputHandler.Instance.FindHandAnchors();
        }

        //define the scene prefabs
        foreach (GameObject g in scenePrefabs)
        {
            prefabs[g.name] = g;
        }

        currentTask = tasks[0];
        currentTask.enabled = true;
        isRunning = true;


        BeginNextTrial();
    }

    /// <summary>
    /// Center the OVRPlayerController to the dock location
    /// </summary>
    public void CenterOVRPlayerController()
    {
        vrCtlr.transform.position = new Vector3(currentTask.Dock.transform.position.x, currentTask.Dock.transform.position.y, currentTask.Dock.transform.position.z);
    }

    /// <summary>
    /// Begin the next trial, if available. On the final trial this call will end the experiment.
    /// </summary>
    public void BeginNextTrial()
    {
        //for the first trial
        if (session.currentTrialNum == 0)
        {
            session.FirstTrial.Begin();
        }
        //every other trial
        else if (session.currentTrialNum < totalNumOfTrials)
        {
            session.BeginNextTrialSafe();
        }
        else if (session.currentTrialNum == totalNumOfTrials) 
        {
            isRunning = false;
            session.End();
        }
        PlayerPrefs.SetInt("currentTrial", session.currentTrialNum);
        PlayerPrefs.SetInt("currentBlock", session.CurrentBlock.number);
    }
    /// <summary>
    /// UXF TrialBegin method
    /// </summary>
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
    /// <summary>
    /// UXF TrialEnd method
    /// </summary>
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
    /// <summary>
    /// UXF OnSessionEnd method
    /// </summary>
    public void OnSessionEnd()
    {
        //Application.Quit();
        //Display end screen
        currentTask.enabled = true;
        currentTask.SetUp();
        currentTask.TaskBegin();
    }

}
