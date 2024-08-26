using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class ProjectileTask : BaseTask
{
    /// <summary>
    /// Position of the hand during flick
    /// </summary>
    List<Vector4> handPos = new List<Vector4>();
    /// <summary>
    /// Visible ball travel position
    /// </summary>
    List<Vector3> ballPos = new List<Vector3>();
    /// <summary>
    /// True ball/tool object
    /// </summary>
    [SerializeField]
    GameObject ball;
    /// <summary>
    /// Rigidboy of the actual ball
    /// </summary>
    Rigidbody ballRB;
    /// <summary>
    /// Collider to check if the pariticpant hit into the wrong area
    /// </summary>
    [SerializeField]
    List<Target> outOfBoundsCollider;
    Plane ballPlane;
    /// <summary>
    /// Feedback text
    /// </summary>
    [SerializeField]
    Text displayText;
    /// <summary>
    /// Visible ball travel line
    /// </summary>
    LineRenderer visBallTravelPath;
    /// <summary>
    /// Color of the line renderer
    /// </summary>
    Color lineColor = Color.white;
    /// <summary>
    /// Hand start pos
    /// </summary>
    Vector3 startPos;
    /// <summary>
    /// Hand end pos
    /// </summary>
    Vector3 endPos;
    /// <summary>
    /// The normalized vector the ball will launch
    /// </summary>
    Vector3 launchVec;
    /// <summary>
    /// The vector the ball will launch
    /// </summary>
    Vector3 launchForce;
    /// <summary>
    /// Which button to look for to check for button held
    /// </summary>
    string buttonCheck = "";
    /// <summary>
    /// Speed to determine the ball came to a stop
    /// </summary>
    const float END_SPEED = 0.06f;
    /// <summary>
    /// Force to launch the ball
    /// </summary>
    const float LAUNCH_FORCE = 8.0f;
    /// <summary>
    /// Magnitude to cap the launch force
    /// </summary>
    const float LAUNCH_MAG = 0.5f;
    /// <summary>
    /// Minimum magnitude to be considered a launch
    /// </summary>
    const float MIN_MAG = 0.3f;
    /// <summary>
    /// Distance to determine the participant is flicking the ball
    /// </summary>
    const float FLICK_DIST = 0.1f;
    const float TARGET_DIST = 1f;
    /// <summary>
    /// Time in seconds to display a prompt
    /// </summary>
    const float DISPLAY_TIME = 1.5f;
    /// <summary>
    /// Width of the line rendered visible ball path complete
    /// </summary>
    const float LINE_SIZE = 0.025f;
    const float BALL_MAX_ANGULAR_VEL = 240.0f;
    /// <summary>
    /// Time the button is pressed
    /// </summary>
    float launchStartTime = 0.0f;
    /// <summary>
    /// Time the button was released or distance was greater than a certain amount
    /// </summary>
    float launchEndTime = 0.0f;

    float currentAngle = 0.0f;
    string currentType = "";

    float currentWaterForce = 0.0f;

    int totalScore = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    void FixedUpdate()
    {
        if(currentStep == 1)
        {
            if (Input.GetButton(buttonCheck))
            {
                Vector3 pos = GetMousePos();

                handPos.Add(new Vector4(pos.x, pos.y, pos.z, Time.time));
            }

            if (Vector3.Distance(GetMousePos(), startPos) > FLICK_DIST || !Input.GetButton(buttonCheck))
            {
                    endPos = GetMousePos();
                    launchEndTime = Time.time;

                    float totalTime = launchEndTime - launchStartTime;
                    launchVec = endPos - startPos;
                    launchVec.Normalize();
                    //launchVec = ExperimentController.Instance.UseVR ? launchVec : Quaternion.Euler(90, 0, 0) * launchVec;

                    Debug.Log("Launch time " + totalTime);
                    Debug.Log("Launch vector " + launchVec);

                    ballRB.isKinematic = false;
                    ballRB.useGravity = true;

                    Vector3 force = launchVec;
                    force.y = 0.0f;
                    force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);
                    force *= ((targetAngles[currentTrial] / 5) * 0.5f) + (LAUNCH_FORCE);

                    launchForce = force;

                    if (launchForce.magnitude < MIN_MAG)
                    {
                        Debug.Log("The launch force was too small, applying a new force");
                        Debug.Log("New force " + force * 2.0f);
                        Debug.Log("New force mag " + (force * 2.0f).magnitude);

                        launchForce = force * 2.0f;
                    }

                    ballRB.velocity = launchForce;
                    Debug.Log("Launch force " + force);
                    Debug.Log("Launch mag " + force.magnitude);
                    cursor.SetActive(false);

                    IncrementStep();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //Participant returns to home
            case 0:
                if (/*Vector3.Distance(cursor.transform.position,home.transform.position) <= PRE_LAUNCH_DIST && */Input.GetButtonDown(buttonCheck))
                {
                    Debug.Log("Button held");
                    //If we are using VR use the VR hand position else get the
                    //converted mouse position
                    startPos = GetMousePos();

                    launchStartTime = Time.time;
                    IncrementStep();
                } 

                break;
            #region Launch ball
            //Track cursor(hand position) and launch when certain distance from home
            // case 1:
            //     {
            //         //If button is pressed
            //         if (Input.GetButton(buttonCheck))
            //         {
            //             Vector3 pos = GetMousePos();

            //             handPos.Add(new Vector4(pos.x, pos.y, pos.z, Time.time));
            //         }

            //         if (Vector3.Distance(GetMousePos(), startPos) > FLICK_DIST || !Input.GetButton(buttonCheck))
            //         {
            //             endPos = GetMousePos();
            //             launchEndTime = Time.time;

            //             float totalTime = launchEndTime - launchStartTime;
            //             launchVec = endPos - startPos;
            //             launchVec.Normalize();
            //             launchVec = ExperimentController.Instance.UseVR ? launchVec : Quaternion.Euler(90, 0, 0) * launchVec;
            //             //launchVec = Quaternion.Euler(90, 0, 0) * launchVec;

            //             Debug.Log("Launch time " + totalTime);
            //             Debug.Log("Launch vector " + launchVec);

            //             ballRB.isKinematic = false;
            //             ballRB.useGravity = true;

            //             Vector3 force = launchVec;
            //             force.y = 0.0f;
            //             force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);
            //             force *= ((targetAngles[currentTrial] / 5) * 0.5f) + (LAUNCH_FORCE);

            //             Debug.Log("fire force: " + (((targetAngles[currentTrial] / 5) * 0.5f) + (LAUNCH_FORCE)));
            //             Debug.Log("target angle: " + targetAngles[currentTrial]);



            //             Vector3 launchForce = force;

            //             if (launchForce.magnitude < MIN_MAG)
            //             {
            //                 Debug.Log("The launch force was too small, applying a new force");
            //                 Debug.Log("New force " + force * 2.0f);
            //                 Debug.Log("New force mag " + (force * 2.0f).magnitude);

            //                 launchForce = force * 2.0f;
            //             }

            //             ballRB.velocity = launchForce;
            //             Debug.Log("Launch force " + force);
            //             Debug.Log("Launch mag " + force.magnitude);
            //             cursor.SetActive(false);

            //             IncrementStep();
            //         }
            //     }
            //     break;
            #endregion
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                {
                    DebugDrawLaunchVec();

                    /*
                    Vector3 toTarget = target.transform.position - home.transform.position;
                    Vector3 toBall = target.transform.position - ball.transform.position;
                    float dot = Vector3.Dot(toTarget, toBall);
                    */
                    ballPos.Add(ball.transform.position);

                    //Ball hit the target
                    if (target.GetComponent<Target>().TargetHit)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.green;
                        int points = CalculatePoints(true);
                        StartCoroutine(DisplayMessage("Target hit\n" + points + " points"));
                        IncrementStep();
                    }
                    // else if (dot <= 0.0f)
                    // {
                    //     ballRB.isKinematic = true;

                    //     lineColor = Color.white;
                    //     StartCoroutine(DisplayMessage("Missed target"));
                    //     IncrementStep();
                    // }
                    //Ball slowed down
                    else if (ballRB.velocity.magnitude <= END_SPEED)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.yellow;
                        int points = CalculatePoints(false);
                        StartCoroutine(DisplayMessage("Ball came to a stop\n" + points + " points"));
                        totalScore += points;
                        IncrementStep();
                    }
                    else
                    {
                        foreach (Target t in outOfBoundsCollider)
                        {
                            if (t.TargetHit)
                            {
                                ballRB.isKinematic = true;

                                lineColor = Color.red;
                                StartCoroutine(DisplayMessage("Ball out of bounds\n0 points"));
                                IncrementStep();
                                break;
                            }
                        }
                    }
                }
                break;
            //Displaying feedback
            case 3:
                {
                    DebugDrawLaunchVec();
                }
                break;
        }
    }

    IEnumerator DisplayMessage(string displayMessage = "")
    {
        float delayTime = 0.0f;

        visBallTravelPath.positionCount = ballPos.Count;
        visBallTravelPath.SetPositions(ballPos.ToArray());
        visBallTravelPath.startColor = visBallTravelPath.endColor = lineColor;

        //Display feedback text here
        if (displayMessage.Length > 0)
        {
            Debug.Log(displayMessage);
            displayText.text = displayMessage;
        }

        while (delayTime <= DISPLAY_TIME)
        {
            delayTime += Time.deltaTime;
            yield return null;
        }

        IncrementStep();
        yield return new WaitForEndOfFrame();
    }

    private void DebugDrawLaunchVec()
    {
        float dist = Vector3.Distance(startPos, endPos);
        Vector3 dir = endPos - startPos;
        Debug.DrawRay(home.transform.position, dir.normalized * dist, Color.red);
    }

    private int CalculatePoints(bool hitTarget)
    {
        float distanceFromTarget = Vector3.Distance(target.transform.position, ball.transform.position);
        Debug.Log("The distance from target is " + distanceFromTarget + " units");
        int points = 0;

        if (hitTarget)
        {
            points = 5;
        }
        else
        {
            float targetWidth = target.GetComponent<MeshRenderer>().bounds.size.x;
            Debug.Log("Target width " + targetWidth);

            if (distanceFromTarget > targetWidth)
                points = 0;
            else if (distanceFromTarget <= targetWidth)
                points = 1;
        }

        Debug.Log("Scored " + points + " points");
        return points;
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;
        totalScore = 0;

        if (!ball)
            ball = GameObject.Find("Ball");

        if(outOfBoundsCollider.Count == 0)
            Debug.LogWarning("No out of bounds colliders set");

        ballRB = ball.GetComponent<Rigidbody>();
        ballRB.maxAngularVelocity = BALL_MAX_ANGULAR_VEL;
        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
        visBallTravelPath = GetComponent<LineRenderer>();

        //Set the renderer for pinpall path
        visBallTravelPath.startWidth = visBallTravelPath.endWidth = LINE_SIZE;

        if (targetAngles.Count == 0)
        {
            targetAngles = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }

        if(ExperimentController.Instance.UseVR == true)
        {
            if(InputHandler.Instance.GetDominantHandString() == "RightHand")
                buttonCheck = "XRI_Right_TriggerButton";
            else
                buttonCheck = "XRI_Left_TriggerButton";
        }
        else
        {
            buttonCheck = "Fire1";
        }

        GameObject water = GameObject.Find("Water");
        CurrentForce currentForce = water.GetComponent<CurrentForce>();
        currentWaterForce = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_force")[ExperimentController.Instance.Session.currentBlockNum - 1];
        currentForce.sideForce = currentWaterForce;

        water.GetComponent<Renderer>().material.SetFloat("_Speed", (float)(-0.2*(currentWaterForce/50)));

        
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        launchStartTime = 0.0f;
        launchEndTime = 0.0f;

        startPos = Vector3.zero;
        endPos = Vector3.zero;

        //cursor.SetActive(true);

        ballRB.velocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;
        ballRB.isKinematic = true;
        ballRB.useGravity = false;
        
        ball.transform.position = home.transform.position;
        ball.transform.rotation = Quaternion.identity;

        handPos.Clear();
        ballPos.Clear();
        lineColor = Color.white;
        visBallTravelPath.positionCount = 0;
        visBallTravelPath.SetPositions(ballPos.ToArray());

        //Setup target position
        target.GetComponent<Target>().ResetTarget();

        foreach (Target t in outOfBoundsCollider)
            t.ResetTarget();

        displayText.text = "";

        ballPlane = new Plane(ball.transform.up,ball.transform.position.y);

        //Debug.Log("target angle: " + targetAngles[currentTrial]);
        // target.transform.position = Vector3.zero;
        // target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        float z = target.transform.localPosition.z;
        // calculate x position based on angle
        float x = Mathf.Tan(targetAngles[currentTrial] * Mathf.Deg2Rad) * z;    
        target.transform.localPosition = new Vector3(x, target.transform.localPosition.y, z);
        currentAngle = targetAngles[currentTrial];
        currentType = ExperimentController.Instance.Session.CurrentTrial.settings.GetStringList("per_block_task")[ExperimentController.Instance.Session.currentBlockNum - 1];
    }

    private Vector3 GetMousePos()
    {

        return ExperimentController.Instance.UseVR ? InputHandler.Instance.GetHandPosition() : GetMouseWorldPos();
    }

    private Vector3 GetCursorScreenPercentage()
    {
        return  new Vector3(InputHandler.Instance.GetPosition().x / Screen.width, InputHandler.Instance.GetPosition().y / Screen.height, 0);
    }

    private Vector3 GetMouseWorldPos()
    {
        //Vector3 mousePos = Input.mousePosition;
        //mousePos.z = Camera.main.nearClipPlane;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        Plane pPlane = new Plane(plane.transform.up, ball.transform.position.y);

        if(pPlane.Raycast(ray, out distance)){
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private Vector3 GetHandOnBallPlane()
    {
        return Vector3.ProjectOnPlane(InputHandler.Instance.GetHandPosition(), ballPlane.normal) + Vector3.Dot(InputHandler.Instance.GetHandPosition(),ballPlane.normal) * ballPlane.normal;
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;

        session.CurrentTrial.result["hand"] = "r";
        session.CurrentTrial.result["target_hit"] = target.GetComponent<Target>().TargetHit;
        session.CurrentTrial.result["type"] = currentType;
        session.CurrentTrial.result["target_position"] = target.transform.position;
        session.CurrentTrial.result["target_angle"] = currentAngle;
        session.CurrentTrial.result["launch_direction"] = launchVec;
        session.CurrentTrial.result["launch_angle"] = Vector3.Angle(Vector3.forward, launchVec);
        session.CurrentTrial.result["launch_angle_error"] = Vector3.Angle(Vector3.forward, launchVec) - Mathf.Abs(currentAngle);
        session.CurrentTrial.result["distance_from_target"] = Vector3.Distance(target.transform.position, ball.transform.position);
        session.CurrentTrial.result["total_score"] = totalScore;
    }
}
