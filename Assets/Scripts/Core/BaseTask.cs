using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    // The experiment controller
    protected ExperimentController expController;
    // The type of task
    protected string experimentMode;
    // Current step of the task
    protected int currentStep;
    // Number of steps this task has
    protected int maxSteps;
    // Are we out of steps
    protected bool finished;
    // Has the task been setup
    protected bool ready;

    //the current trial num
    protected int currentTrial;
    //the total trials
    protected int totalTrials;

    public BaseTask()
    {
        expController = ExperimentController.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual bool IncrementStep()
    {
        currentStep++;

        //TODO add time
        finished = currentStep == maxSteps;

        return finished;
    }

    public int CurrentStep
    {
        get { return currentStep; }
        set { currentStep = value; }
    }

    public int MaxSteps
    {
        get { return maxSteps; }
        set { maxSteps = value; }
    }

    public bool Finished
    {
        get { return finished; }
        set { finished = value; }
    }

    public string ExperimentMode
    {
        get { return experimentMode; }
        set { experimentMode = value; }
    }

    /// <summary>
    /// Set up the task and related assets
    /// </summary>
    public abstract void SetUp();

    /// <summary>
    /// Begin the task
    /// </summary>
    public abstract void TaskBegin();

    /// <summary>
    /// End the task
    /// </summary>
    public abstract void TaskEnd();

    /// <summary>
    /// Log parameters when a trial ends
    /// </summary>
    public abstract void LogParameters();
}
