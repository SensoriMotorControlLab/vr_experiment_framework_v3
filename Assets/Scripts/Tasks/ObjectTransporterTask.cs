using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTransporterTask : BaseTask
{
    // Start is called before the first frame update
    [SerializeField] 
    List<Tool> toolList = new List<Tool>();
    
    [SerializeField] 
    List<GoalCheck> goalChecks = new List<GoalCheck>();
    Tool GrabbedObject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //reach to dock
            case 0:
                foreach (Tool t in toolList){
                    if (t.IsGrabbed){
                        GrabbedObject = t;
                        IncrementStep();
                        break;
                    }
                }
                break;
            //reach to home
            case 1:
                GrabbedObject.transform.position = cursor.transform.position;
                foreach (GoalCheck g in goalChecks){
                    if (g.colliding){
                        IncrementStep();
                        break;
                    }
                }
                break;
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;
        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);

    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start
    }
    
    public override void TaskEnd()
    {
        //clean up
        base.TaskEnd();
    }

    public override void LogParameters()
    {

    }
}
