using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Abstract class to define a task, all task objects should inherit from this task
/// </summary>
public abstract class BaseTask : MonoBehaviour
{
    // The experiment controller
    protected ExperimentController expController;
    // The type of task
    protected string taskType;
    // Current step of the task
    protected int currentStep;
    // Number of steps this task has
    protected int maxSteps;
    /// <summary>
    /// Have we reached the final step
    /// </summary>
    protected bool finished;
    /// <summary>
    /// Has the task been setup by calling SetUp()
    /// </summary>
    protected bool ready;
    /// <summary>
    /// The home for the experiment
    /// </summary>
    protected GameObject home { get; set; }
    public GameObject Home { get { return home; } }
    /// <summary>
    /// The docking position for the experiment
    /// </summary>
    protected GameObject dock { get; set; }
    public GameObject Dock { get { return home; } }

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
    /// <summary>
    /// Increment the step and return if we have reached the final step
    /// </summary>
    public virtual bool IncrementStep()
    {

        currentStep++;

        //TODO add time
        finished = currentStep == maxSteps;

        if (finished)
            currentTrial++;

        //Debug.Log(name + "step: " + currentStep);

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

    public string TaskType
    {
        get { return taskType; }
        set { taskType = value; }
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
