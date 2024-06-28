using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTask : BaseTask
{
    [SerializeField]
    Tool ball;
    Rigidbody ballRB;
    Vector3 previousPos;
    /// <summary>
    /// Speed to increment the step
    /// </summary>
    const float END_SPEED = 0.06f;
    const float LAUNCH_FORCE = 100.0f;
    const float LAUNCH_DIST = 0.2f;
    const float PRE_LAUNCH_DIST = 0.12f;
    const float LAUNCH_MAG = LAUNCH_DIST - PRE_LAUNCH_DIST;
    float launchTime = 0.0f;

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
            //Participant returns to home
            case 0:
                if (Vector3.Distance(cursor.transform.position,home.transform.position) <= PRE_LAUNCH_DIST && Input.GetButtonDown("Fire1"))
                {
                    Debug.Log("At home");
                    IncrementStep();
                } 

                break;
            //Track cursor(hand position) and launch when certain distance from home
            case 1:

                if (Input.GetButton("Fire1"))
                {
                    launchTime += Time.deltaTime;
                    previousPos = cursor.transform.position;
                }


                if (Vector3.Distance(cursor.transform.position, home.transform.position) >= LAUNCH_DIST || !Input.GetButton("Fire1"))
                {
                    Vector3 launchVec = cursor.transform.position - home.transform.position;
                    
                    float cursorDist = Vector3.Distance(cursor.transform.position, home.transform.position);
                    Debug.Log("Cursor dist " + cursorDist);
                    Debug.Log("Launch time " + launchTime);

                    Debug.Log("Launch vector "  + launchVec);
                    ballRB.isKinematic = false;
                    ballRB.useGravity = true;
                    ballRB.AddForce(launchVec.normalized * LAUNCH_FORCE);
                    Debug.Log("Launch force " + launchVec.normalized * LAUNCH_FORCE);
                    //Debug.Log("Launch force " + launchVec * LAUNCH_FORCE);
                    cursor.SetActive(false);
                    IncrementStep();
                }

                break;
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                Vector3 toTarget = target.transform.position - home.transform.position;
                Vector3 toBall = target.transform.position - ball.transform.position;
                float dot = Vector3.Dot(toTarget, toBall);

                //Ball the hit target
                if (target.GetComponent<Target>().TargetHit)
                {
                    Debug.Log("Target hit");
                    ballRB.isKinematic = false;
                    IncrementStep();
                }
                else if(dot <= 0.0f)
                {
                    IncrementStep();
                }
                //Ball slowed down
                else if(ballRB.velocity.magnitude <= END_SPEED)
                {
                    ballRB.isKinematic = true;
                    IncrementStep();
                }
                break;
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        if(!ball)
            ball = GameObject.Find("Ball").GetComponent<Tool>();

        ballRB = ball.GetComponent<Rigidbody>();
        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        cursor.SetActive(true);
        ballRB.isKinematic = true;
        ballRB.useGravity = false;
        ball.transform.position = home.transform.position;
        launchTime = 0.0f;

        //Setup target position
        target.GetComponent<Target>().ResetTarget();
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        
    }
}
