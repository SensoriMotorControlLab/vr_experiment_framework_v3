using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTask : BaseTask
{
    //w would be time
    List<Vector4> handPos = new List<Vector4>();
    [SerializeField]
    Tool ball;
    [SerializeField]
    Target wrongWayCollider;
    Rigidbody ballRB;
    Vector3 startPos;
    Vector3 endPos;
    /// <summary>
    /// Speed to increment the step
    /// </summary>
    const float END_SPEED = 0.06f;
    const float LAUNCH_FORCE = 15.0f;
    const float LAUNCH_MAG = 0.25f;

    float launchStartTime = 0.0f;
    float launchEndTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse = GetCursorScreenPercentage();

        if (Input.GetKeyDown(KeyCode.R))
        {
            //SetUp();
            TaskBegin();
        }

        switch (currentStep)
        {
            //Participant returns to home
            case 0:
                if (/*Vector3.Distance(cursor.transform.position,home.transform.position) <= PRE_LAUNCH_DIST && */Input.GetButtonDown("Fire1"))
                {
                    Debug.Log("Button held");
                    //If we are using VR use the VR hand position else get the
                    //converted mouse position
                    startPos = ExperimentController.Instance.UseVR == true ?
                        InputHandler.Instance.GetHandPosition() : mouse;

                    launchStartTime = Time.time;
                    IncrementStep();
                } 

                break;
            //Track cursor(hand position) and launch when certain distance from home
            case 1:
                {
                    //If button is pressed
                    if (Input.GetButton("Fire1"))
                    {
                        Vector3 pos = ExperimentController.Instance.UseVR == true ?
                            InputHandler.Instance.GetHandPosition() : mouse;

                        handPos.Add(new Vector4(pos.x, pos.y, pos.z, Time.time));
                    }

                    //If using VR
                    if (ExperimentController.Instance.UseVR == true)
                    {
                        //Get velocity of hand
                        Vector3 handVelocity = InputHandler.Instance.GetHandVelocity();

                        if (!Input.GetButton("Fire1"))
                        {
                            Vector3 endPos = InputHandler.Instance.GetHandPosition();
                            launchEndTime = Time.time;
                            float totalTime = launchEndTime - launchStartTime;

                            Vector3 launchVec = endPos - startPos;
                            launchVec.Normalize();
                            //This is similar to GetCursorScreenPercentage(), converts to 2D? needs testing
                            //launchVec = new Vector3(launchVec.x / Screen.width, launchVec.y / Screen.height, 0);
                            launchVec = Quaternion.Euler(90, 0, 0) * launchVec;

                            Debug.Log("Launch time " + totalTime);
                            Debug.Log("Launch vector " + launchVec);

                            ballRB.isKinematic = false;
                            ballRB.useGravity = true;

                            Vector3 force = launchVec * handVelocity.magnitude;
                            force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);

                            ballRB.velocity = force;
                            Debug.Log("Launch force " + force);
                            cursor.SetActive(false);

                            IncrementStep();
                        }
                    }
                    //If not using VR
                    else if (ExperimentController.Instance.UseVR == false)
                    {
                        //Launch the ball (2D)
                        if (Vector3.Distance(mouse, startPos) > .1f)
                        {
                            Vector3 endPos = mouse;
                            launchEndTime = Time.time;
                            float totalTime = launchEndTime - launchStartTime;

                            Vector3 launchVec = endPos - startPos;
                            //Vector3 toTarget = target.transform.position - home.transform.position;
                            launchVec.Normalize();
                            launchVec = Quaternion.Euler(90, 0, 0) * launchVec;

                            //Alternative way for checking wrong direction using vectors
                            //float dot = Vector3.Dot(toTarget, home.transform.position + launchVec);

                            //If the launch vector was towards the target
                            //if (dot >= 0.0f)
                            //{
                            //Debug.Log("Proper launch vector " + dot);

                            Debug.Log("Launch time " + totalTime);
                            Debug.Log("Launch vector " + launchVec);

                            ballRB.isKinematic = false;
                            ballRB.useGravity = true;

                            Vector3 force = launchVec * LAUNCH_FORCE;
                            force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);

                            ballRB.velocity = force;
                            Debug.Log("Launch force " + force);
                            cursor.SetActive(false);

                            IncrementStep();
                            //}
                            //If the launch vector was away from the target, launched backwards etc.
                            /*
                            else if(dot < 0.0f)
                            {
                                Debug.Log("Improper launch vector, try flicking again " + dot);
                                currentStep--;
                            }
                            */
                        }
                    }
                }
                break;
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                {
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
                    else if (wrongWayCollider.TargetHit)
                    {
                        Debug.Log("Ball went the wrong way");
                        IncrementStep();
                    }
                    else if (dot <= 0.0f)
                    {
                        Debug.Log("Missed target");
                        IncrementStep();
                    }
                    //Ball slowed down
                    else if (ballRB.velocity.magnitude <= END_SPEED)
                    {
                        Debug.Log("Ball slowed down");
                        ballRB.isKinematic = true;
                        IncrementStep();
                    }
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
        if (!wrongWayCollider)
            wrongWayCollider = GameObject.Find("WrongWayCollider").GetComponent<Target>();

        ballRB = ball.GetComponent<Rigidbody>();
        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        launchStartTime = 0.0f;
        launchEndTime = 0.0f;

        startPos = Vector3.zero;
        endPos = Vector3.zero;

        cursor.SetActive(true);

        ballRB.velocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;
        ballRB.isKinematic = true;
        ballRB.useGravity = false;

        ball.transform.position = home.transform.position;

        //Setup target position
        target.GetComponent<Target>().ResetTarget();
        wrongWayCollider.ResetTarget();
    }

    private Vector3 GetCursorScreenPercentage()
    {
        return  new Vector3(InputHandler.Instance.GetPosition().x / Screen.width, InputHandler.Instance.GetPosition().y / Screen.height, 0);
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        
    }
}
