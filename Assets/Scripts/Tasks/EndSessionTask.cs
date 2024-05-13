using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndSessionTask : BaseTask
{
    Camera endCamera;
    GameObject instructionPrefab;
    TMP_Text instructionText;

    public EndSessionTask()
    {
        taskType = "endsessions";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            IncrementStep();
            TaskEnd();
            enabled = false;
        }
    }

    public override void LogParameters()
    {

    }

    public override void SetUp()
    {
        currentStep = 0;
        totalTrials = expController.Session.CurrentBlock.trials.Count;
        maxSteps = 1;

        instructionPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/" +prefabName), expController.transform);;

        endCamera = GameObject.Find("InstructionCamera").GetComponent<Camera>();
        if (ExperimentController.Instance.UseVR == false)
            Camera.SetupCurrent(endCamera);

        instructionText = GameObject.Find("InstructionText").GetComponent<TMP_Text>();
        instructionText.text = "You have completed all trials\n[Press anything to finish]";
    }

    public override void TaskBegin()
    {

    }

    public override void TaskEnd()
    {
        Destroy(instructionPrefab);
        Application.Quit();
    }
}
