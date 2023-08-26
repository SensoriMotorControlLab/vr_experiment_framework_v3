using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InstructionTask : BaseTask
{
    GameObject instructionPrefab;
    Camera instructionCam;
    TMP_Text instructionText;

    public InstructionTask()
    {
        experimentMode = "instruction";
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

        instructionPrefab = Instantiate(expController.Prefabs["InstructionPrefab"], expController.transform);

        instructionCam = GameObject.Find("InstructionCamera").GetComponent<Camera>();
        if (ExperimentController.Instance.UseVR == false)
            Camera.SetupCurrent(instructionCam);

        instructionText = GameObject.Find("InstructionText").GetComponent<TMP_Text>();
        string insKey = expController.Session.CurrentBlock.settings.GetString("instruction");
        string insString = expController.Session.settings.GetString(insKey);
        instructionText.text = insString;
    }

    public override void TaskBegin()
    {
        
    }

    public override void TaskEnd()
    {
        Destroy(instructionPrefab);
    }
}
