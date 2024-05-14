using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndSessionTask : BaseTask
{
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
        maxSteps = 1;

        instructionText = GameObject.Find("InstructionText").GetComponent<TMP_Text>();
        instructionText.text = "You have completed all trials\n[Press anything to finish]";
    }

    public override void TaskBegin()
    {

    }

    public override void TaskEnd()
    {
        Application.Quit();
    }
}
