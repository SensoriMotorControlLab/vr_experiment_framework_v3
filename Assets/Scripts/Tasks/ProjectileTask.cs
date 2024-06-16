using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTask : BaseTask
{
    [SerializeField]
    Target line;
    [SerializeField]
    Tool ball;
    Rigidbody ballRB;
    Vector3 collisionPos;
    /// <summary>
    /// Speed to increment the step
    /// </summary>
    const float END_SPEED = 0.1f;
    const float LAUNCH_FORCE = 500.0f;
    bool hitLine = false;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //SetUp();
            TaskBegin();
        }

        switch (currentStep)
        {
            //Moving the ball, ball is kinematic
            case 0:

                if (line.TargetHit && !hitLine)
                {
                    collisionPos = ball.transform.position;
                    hitLine = true;
                }

                if (!line.Colliding && hitLine)
                {
                    Vector3 launchVec = Cursor.transform.position - collisionPos;
                    launchVec.Normalize();

                    Debug.Log("Launch vector "  + launchVec);
                    ballRB.isKinematic = false;
                    ballRB.useGravity = true;
                    ballRB.AddForce(launchVec * LAUNCH_FORCE);
                    Debug.Log("Launch force " + launchVec * LAUNCH_FORCE);
                    cursor.SetActive(false);
                    IncrementStep();
                }
                else
                {
                    ball.transform.position = cursor.transform.position;
                }

                break;
            //Ball hit the line, ball is no longer kinematic, apply force
            case 1:
                //Ball the hit target
                if (target.GetComponent<Target>().TargetHit)
                {

                    ballRB.isKinematic = false;
                    IncrementStep();
                }
                //Ball missed the target
                /*
                else if(ballRB.velocity.magnitude <= END_SPEED)
                {
                    ballRB.isKinematic = true;
                    IncrementStep();
                }
                */
                break;
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 2;

        if(!line)
            line = GameObject.Find("Line").GetComponent<Target>();
        if(!ball)
            ball = GameObject.Find("Ball").GetComponent<Tool>();

        ballRB = ball.GetComponent<Rigidbody>();
        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        line.ResetTarget();
        cursor.SetActive(true);
        ballRB.isKinematic = true;
        ballRB.useGravity = false;
        ball.transform.position = home.transform.position;
        hitLine = false;

        //Setup target position
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        
    }
}
