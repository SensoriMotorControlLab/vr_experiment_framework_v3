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
        taskType = "instruction";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //once there is a key down input
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

        instructionPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/" +prefabName), expController.transform);;

        instructionCam = GameObject.Find("InstructionCamera").GetComponent<Camera>();
        if (ExperimentController.Instance.UseVR == false)
            Camera.SetupCurrent(instructionCam);

        instructionText = GameObject.Find("InstructionText").GetComponent<TMP_Text>();
        //in the JSON the per_block is just the key to the text key-value
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
