using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using UXF;
using TMPro;
using UnityEditor;

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
    List<float> ballTime = new List<float>();
    List<float> stepTime = new List<float>();
    /// <summary>
    /// True ball/tool object
    /// </summary>
    [SerializeField]
    GameObject ball;
    [SerializeField]
    GameObject water;
    /// <summary>
    /// Rigidboy of the actual ball
    /// </summary>
    Rigidbody ballRB;
    /// <summary>
    /// Collider to check if the pariticpant hit into the wrong area
    /// </summary>
    [SerializeField]
    List<Target> outOfBoundsCollider;
    /// <summary>
    /// Feedback text
    /// </summary>
    [SerializeField]
    Text displayText;
    [SerializeField]
    TextMeshProUGUI ballDisplayText;
    [SerializeField]
    Canvas ballCanvas;
    [SerializeField]
    AudioSource ballAudio;
    [SerializeField]
    AudioSource prefabAudio;
    [SerializeField]
    AudioClip correctAudioClip;
    [SerializeField]
    AudioClip incorrectAudioClip;
    [SerializeField]
    AudioSource waterAudio;
    /// <summary>
    /// Visible ball travel line
    /// </summary>
    LineRenderer visBallTravelPath;
    List<Vector3> globalBallPos = new List<Vector3>();
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
    const float LAUNCH_FORCE = 1.0f;
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
    const float DISPLAY_TIME = 1.0f;
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
    bool hitTarget = false;

    float currentAngle = 0.0f;
    string currentType = "";

    float currentWaterForce = 0.0f;
    float currentWaterForceForward = 0.0f;

    static int totalScore = 0;
    public TextMeshProUGUI scoreText;

    private int trialsRemaining;
    public TextMeshProUGUI trialsRemainingText;

    float closestDistance = float.MaxValue;
    Vector2 closestBallPosToTarget;
    float debrisSpawnRate;
    int debrisCount;
    DebrisSpawner debrisSpawner;
    Vector3 cursorPos;
    Vector3 throwSpeed;
    Vector2 absTurning;
    string finalBallState;
    float targetScale;
    float waterSpeed;
    GameObject poleOne;
    GameObject poleTwo;
    float targetWidth;
    Vector3 finalBallPosWorld;

    // Start is called before the first frame update
    void Start()
    {
        trialsRemaining = ExperimentController.Instance.GetTotalTrials();
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
                    // Debug.Log("Button held");
                    //If we are using VR use the VR hand position else get the
                    //converted mouse position
                    startPos = GetMousePos();
                    cursorPos = startPos;

                    launchStartTime = Time.time;
                    IncrementStep();

                    //increment step time
                    stepTime.Add(Time.time);
                } 

                break;
            #region Launch ball
            //Track cursor(hand position) and launch when certain distance from home
            case 1:
                {
                    //If the button is pressed again
                    if(Input.GetButtonDown(buttonCheck))
                    {
                        startPos = GetMousePos();
                        Debug.Log("start position: " + startPos);
                        cursorPos = startPos;
                        handPos.Clear();
                    }
                    //If button released early
                    else if (Input.GetButton(buttonCheck))
                    {
                        cursorPos = GetMousePos();
                        handPos.Add(new Vector4(cursorPos.x, cursorPos.y, cursorPos.z, Time.time));
                    }
                    //Debug.Log("Distance from start: " +Vector3.Distance(cursorPos, startPos));

                    if (Vector3.Distance(cursorPos, startPos) > FLICK_DIST)
                    {
                        //log step time
                        endPos = GetMousePos();
                        launchEndTime = Time.time;

                        float totalTime = launchEndTime - launchStartTime;
                        launchVec = endPos - startPos;
                        launchVec.Normalize();
                        //launchVec = ExperimentController.Instance.UseVR ? launchVec : Quaternion.Euler(90, 0, 0) * launchVec;

                        ballRB.isKinematic = false;
                        ballRB.useGravity = true;

                        //If using VR get the hand velocity for launch if not use the LAUNCH_FORCE constant
                        throwSpeed = ExperimentController.Instance.UseVR ? InputHandler.Instance.GetHandVelocity("RightHand").magnitude * launchVec : launchVec * LAUNCH_FORCE;
                        throwSpeed.y = 0.0f;

                        launchForce = throwSpeed;
                        Debug.Log("Launch vec " + launchVec);

                        if (launchForce.magnitude < MIN_MAG)
                        {
                            Debug.Log("The launch force was too small, applying a new force");
                            Debug.Log("New force " + throwSpeed * 2.0f);
                            Debug.Log("New force mag " + (throwSpeed * 2.0f).magnitude);

                            launchForce = throwSpeed * 2.0f;
                        }

                        ballRB.velocity = launchForce;
                        // Debug.Log("Launch force " + force);
                        // Debug.Log("Launch mag " + force.magnitude);
                        cursor.SetActive(false);

                        IncrementStep();

                        stepTime.Add(Time.time);
                    }
                }
                break;
            #endregion
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                {
                    if(currentWaterForce >= 0 && absTurning.x < ball.transform.localPosition.x)
                    {
                        absTurning = new Vector2 (ball.transform.localPosition.x, ball.transform.localPosition.z);
                    }
                    else if(currentWaterForce < 0 && absTurning.x > ball.transform.localPosition.x)
                    {
                        absTurning = new Vector2 (ball.transform.localPosition.x, ball.transform.localPosition.z);
                    }
                    DebugDrawLaunchVec();
                    ClosestPointToTarget(ball.transform.position);
                    /*
                    Vector3 toTarget = target.transform.position - home.transform.position;
                    Vector3 toBall = target.transform.position - ball.transform.position;
                    float dot = Vector3.Dot(toTarget, toBall);
                    */
                    Vector3 skewedPos = new Vector3(ball.transform.localPosition.x, home.transform.position.y - ball.GetComponent<SphereCollider>().bounds.size.y * 3/4, ball.transform.localPosition.z);
                    globalBallPos.Add(new Vector3(ball.transform.position.x, home.transform.position.y - ball.GetComponent<SphereCollider>().bounds.size.y * 3/4, ball.transform.position.z));
                    ballPos.Add(skewedPos);
                    ballTime.Add(Time.time);
                    string displayMsg = "";
                    int points = 0;

                    //Ball hit the target
                    if (target.GetComponent<Target>().TargetHit)
                    {
                        ballRB.isKinematic = true;

                        finalBallState = "Hit";

                        lineColor = Color.green;
                        hitTarget = true;
                        
                        points = CalculatePoints();
                        totalScore += points;

                        displayMsg = "Target hit\n" + points + " points";

                        prefabAudio.clip = correctAudioClip;
                        //prefabAudio.Play();
                        ballAudio.Stop();

                        closestDistance = 0.0f;
                        ShowFeedback(points, displayMsg);
                        IncrementStep();

                        stepTime.Add(Time.time);
                        finalBallPosWorld = ball.transform.position;
                    }
                    #region Dot product check
                    // else if (dot <= 0.0f)
                    // {
                    //     ballRB.isKinematic = true;

                    //     lineColor = Color.white;
                    //     StartCoroutine(DisplayMessage("Missed target"));
                    //     IncrementStep();
                    // }
                    #endregion
                    //Ball slowed down
                    else if (ballRB.velocity.magnitude <= END_SPEED)
                    {
                        ballRB.isKinematic = true;

                        if(ball.transform.position.x > target.transform.position.x)
                        {
                            finalBallState = "To the right";
                        }
                        else
                        {
                            finalBallState = "To the left";
                        }

                        lineColor = Color.yellow;
                        hitTarget = false;

                        points = CalculatePoints();
                        totalScore += points;

                        displayMsg = "Ball came to a stop\n" + points + " points";

                        prefabAudio.clip = incorrectAudioClip;
                        //prefabAudio.Play();
                        ballAudio.Stop();

                        ShowFeedback(points, displayMsg);
                        IncrementStep();

                        stepTime.Add(Time.time);
                    }
                    //Ball went out of bounds
                    else
                    {
                        foreach (Target t in outOfBoundsCollider)
                        {
                            if (t.TargetHit)
                            {
                                ballRB.isKinematic = true;

                                finalBallState = "Missed";

                                lineColor = Color.red;
                                hitTarget = false;

                                points = 0;
                                displayMsg = "Ball out of bounds\n0 points";

                                prefabAudio.clip = incorrectAudioClip;
                                //prefabAudio.Play();
                                ballAudio.Stop();

                                ShowFeedback(points, displayMsg);
                                IncrementStep();

                                stepTime.Add(Time.time);

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
        UpdateScoreboardUI();
    }

    private void ShowFeedback(int points, string feedbackMsg)
    {
        if (taskType != "invisible")
        {
            StartCoroutine(DisplayMessage(feedbackMsg));
            ballCanvas.transform.position = ballPos[ballPos.Count - 1];
            ballDisplayText.text = "+" + points;

            prefabAudio.Play();
        }
        else
        {
            ballCanvas.transform.position = new Vector3(0.0f, -100.0f, 0.0f);
            ballDisplayText.text = "";
            IncrementStep();
        }
    }


    IEnumerator DisplayMessage(string displayMessage = "")
    {
        float delayTime = 0.0f;

        visBallTravelPath.positionCount = globalBallPos.Count;
        visBallTravelPath.SetPositions(globalBallPos.ToArray());
        visBallTravelPath.startColor = visBallTravelPath.endColor = lineColor;

        //Display feedback text here
        if (displayMessage.Length > 0)
        {
            // Debug.Log(displayMessage);
            displayText.text = displayMessage;
        }

        while (delayTime <= DISPLAY_TIME)
        {
            delayTime += Time.fixedDeltaTime;
            yield return null;
        }

        IncrementStep();
        yield return new WaitForEndOfFrame();
    }

    private void UpdateScoreboardUI()
    {
        trialsRemainingText.text = "Trials Remaining: " + trialsRemaining.ToString();

        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore.ToString();
        }
    }

    private void DebugDrawLaunchVec()
    {
        float dist = Vector3.Distance(startPos, endPos);
        Vector3 dir = endPos - startPos;
        Debug.DrawRay(home.transform.position, dir.normalized * dist, Color.red);
    }

    private int CalculatePoints()
    {
        float distanceFromTarget = Vector3.Distance(target.transform.position, ballPos[ballPos.Count - 1]);
        // Debug.Log("The distance from target is " + distanceFromTarget + " units");
        int points = 0;

        if (hitTarget)
        {
            points = 5;
        }
        else
        {
            float targetWidth = target.GetComponent<MeshRenderer>().bounds.size.x;
            // Debug.Log("Target width " + targetWidth);

            if (distanceFromTarget > targetWidth)
                points = 0;
            else if (distanceFromTarget <= targetWidth)
                points = 1;
        }

        Debug.Log("Scored " + points + " points");
        return points;
    }

    private void ClosestPointToTarget(Vector3 location)
    {
        Collider targetCollider = target.GetComponent<Collider>();
        Vector3 closestPoint = targetCollider.ClosestPoint(location);
        float distance = Vector3.Distance(location, closestPoint);
        
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestBallPosToTarget = new Vector2(ball.transform.localPosition.x, ball.transform.localPosition.z);
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;
        int currBlock = ExperimentController.Instance.Session.currentBlockNum - 1;

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
        targetScale = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("per_block_target_width")[currBlock];
        target.transform.localScale = new Vector3(target.transform.localScale.x, targetScale * target.transform.localScale.y, target.transform.localScale.z);

        poleOne = GameObject.Find("PoleOne");
        poleTwo = GameObject.Find("PoleTwo");

        Vector3[] vertices = target.GetComponent<MeshFilter>().mesh.vertices;
        poleOne.transform.position = target.transform.TransformPoint(vertices[0]);
        poleTwo.transform.position = target.transform.TransformPoint(vertices[vertices.Length - 1]);

        Vector2 poleOnePos = new Vector2(poleOne.transform.position.x, poleOne.transform.position.z);
        Vector2 poleTwoPos = new Vector2(poleTwo.transform.position.x, poleTwo.transform.position.z);

        targetWidth = Vector2.Distance(poleOnePos, poleTwoPos);

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

        if(!water)
            water = GameObject.Find("Water");

        CurrentForce currentForce = water.GetComponent<CurrentForce>();
        
        currentWaterForce = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_force")[currBlock];
        currentForce.sideForce = currentWaterForce;

        currentWaterForceForward = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_force_forward")[currBlock];
        currentForce.forwardForce = currentWaterForceForward;

        debrisSpawner = GameObject.Find("DebrisSpawner").GetComponent<DebrisSpawner>();
        debrisSpawner.speed = currentWaterForce/38.48f;
        debrisSpawnRate = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("per_block_debris_spawn_rate")[currBlock];
        debrisCount = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_debris_count")[currBlock];
        debrisSpawner.spawnRate = debrisSpawnRate;
        debrisSpawner.debrisCount = debrisCount;

        if(currentWaterForce >= 0)
        {
            absTurning.x = float.MinValue;
        }
        else
        {
            absTurning.x = float.MaxValue;
        }

        waterSpeed = currentWaterForce / -384.4f;
        water.GetComponent<Renderer>().material.SetFloat("_Speed", waterSpeed);

        //Adjusted water audio based on current water force
        if (currentWaterForce > 0.0f || currentWaterForce < 0.0f)
        {
            waterAudio.volume = 0.5f;

            if (currentWaterForce > 30.0f)
            {
                float forceDiff = Math.Abs(currentWaterForce) - 30.0f;
                float pitchAdjust = forceDiff / 50.0f;

                waterAudio.pitch = 1.0f + pitchAdjust;
            }
            else if (currentWaterForce < 30.0f)
            {
                
                float forceDiff = 30.0f - Math.Abs(currentWaterForce);
                float pitchAdjust = forceDiff / 50.0f;

                //Debug.Log("Force diff " + forceDiff);
                //Debug.Log("Pitch adjustment " + pitchAdjust);

                waterAudio.pitch = 1.0f - pitchAdjust;

                //Debug.Log("Total diff " + (1.0f - pitchAdjust));
            }
            else
            {
                waterAudio.pitch = 1.0f;
            }
        }
        else
        {
            waterAudio.volume = 0.0f;
        }

        taskType = ExperimentController.Instance.Session.CurrentBlock.settings.GetStringList("per_block_task")[currBlock];
        if(taskType == "invisible")
        {
            GameObject plane = GameObject.Find("Plane");
            plane.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y - 0.075f, plane.transform.position.z);
        }
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        closestDistance = float.MaxValue;

        launchStartTime = 0.0f;
        launchEndTime = 0.0f;

        startPos = Vector3.zero;
        endPos = Vector3.zero;

        hitTarget = false;
        
        ballRB.isKinematic = true;
        ballRB.useGravity = false;
        
        ball.transform.position = home.transform.position;
        ball.transform.rotation = Quaternion.identity;

        handPos.Clear();
        ballPos.Clear();
        globalBallPos.Clear();
        ballTime.Clear();
        stepTime.Clear();
        lineColor = Color.white;
        visBallTravelPath.positionCount = 0;
        visBallTravelPath.SetPositions(ballPos.ToArray());

        //Setup target position
        target.GetComponent<Target>().ResetTarget();

        foreach (Target t in outOfBoundsCollider)
            t.ResetTarget();

        displayText.text = "";
        ballDisplayText.text = "";;

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
    public override bool IncrementStep()
    {
        // check if current trial is the last in the current block
        if (currentTrial == totalTrials - 1 && currentStep == maxSteps - 1)
        {
            debrisSpawner.DestroyDebris();
        }

        if(currentStep + 1 == maxSteps)
        {
            // Decrement trialsRemaining here
            trialsRemaining--;
        }


        return base.IncrementStep();
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;

        session.CurrentTrial.result["hand"] = "r";
        // session.CurrentTrial.result["Head Position"]
        session.CurrentTrial.result["target_hit"] = target.GetComponent<Target>().TargetHit;
        session.CurrentTrial.result["final_ball_state"] = finalBallState;
        session.CurrentTrial.result["type"] = currentType;
        //session.CurrentTrial.result["target_position"] = target.transform.position;
        session.CurrentTrial.result["target_position_x"] = target.transform.localPosition.x;
        session.CurrentTrial.result["target_position_y"] = target.transform.localPosition.y;
        session.CurrentTrial.result["target_position_z"] = target.transform.localPosition.z;
        session.CurrentTrial.result["target_angle"] = currentAngle;
        session.CurrentTrial.result["target_width"] = targetWidth;
        session.CurrentTrial.result["launch_direction"] = launchVec;

        session.CurrentTrial.result["side_water_force"] = currentWaterForce;
        session.CurrentTrial.result["forward_water_force"] = currentWaterForceForward;
        session.CurrentTrial.result["water_speed_m/s"] = waterSpeed * 38.48f;

        session.CurrentTrial.result["launch_angle"] = Vector3.Angle(Vector3.right, launchVec);
        session.CurrentTrial.result["launch_angle_error"] = Vector3.Angle(Vector3.right, launchVec) - Mathf.Abs(currentAngle);
        session.CurrentTrial.result["launch_Speed"] = throwSpeed.magnitude;

        session.CurrentTrial.result["ball_pos_x"] = string.Join(",", ballPos.Select(i => string.Format($"{i.x:F6}")));
        session.CurrentTrial.result["ball_pos_z"] = string.Join(",", ballPos.Select(i => string.Format($"{i.z:F6}")));
        session.CurrentTrial.result["final_ball_pos_x"] = ballPos[ballPos.Count - 1].x;
        session.CurrentTrial.result["final_ball_pos_z"] = ballPos[ballPos.Count - 1].z;
        session.CurrentTrial.result["ball_time"] = string.Join(",", ballTime.Select(i => string.Format($"{i:F6}")));
        session.CurrentTrial.result["turning_absolute_x"] = absTurning.x;
        session.CurrentTrial.result["turning_absolute_y"] = absTurning.y;


        session.CurrentTrial.result["distance_from_target"] = closestDistance;
        poleOne.transform.parent = ball.transform.parent;
        poleTwo.transform.parent = ball.transform.parent;
        session.CurrentTrial.result["rightPole_position_x"] = poleOne.transform.localPosition.x;
        session.CurrentTrial.result["rightPole_position_z"] = poleOne.transform.localPosition.z;
        session.CurrentTrial.result["rightPole_distance_from_ball"] = Vector2.Distance(new Vector2(poleOne.transform.localPosition.x, poleOne.transform.localPosition.z), new Vector2(ballPos[ballPos.Count - 1].x, ballPos[ballPos.Count - 1].z));
        session.CurrentTrial.result["leftPole_position_x"] = poleTwo.transform.localPosition.x;
        session.CurrentTrial.result["leftPole_position_z"] = poleTwo.transform.localPosition.z;
        session.CurrentTrial.result["leftPole_distance_from_ball"] = Vector2.Distance(new Vector2(poleTwo.transform.localPosition.x, poleTwo.transform.localPosition.z), new Vector2(ballPos[ballPos.Count - 1].x, ballPos[ballPos.Count - 1].z));
        session.CurrentTrial.result["min_pos_from_target_x"] = closestBallPosToTarget.x;
        session.CurrentTrial.result["min_pos_from_target_z"] = closestBallPosToTarget.y;

        session.CurrentTrial.result["total_score"] = totalScore;

        for(int i = 0; i < stepTime.Count; i++)
        {
            session.CurrentTrial.result["step_" + i + "_time"] = stepTime[i]; 
        }
    }
}
