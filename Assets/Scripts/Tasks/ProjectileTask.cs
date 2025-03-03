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
    List<float> invisBallTime = new List<float>();
    List<float> stepTime = new List<float>();
    List<Vector3> otherBallPos = new List<Vector3>();
    /// <summary>
    /// True ball/tool object
    /// </summary>
    [SerializeField]
    GameObject ball;
    [SerializeField]
    GameObject otherBall;
    [SerializeField]
    GameObject water;
    [SerializeField]
    GameObject otherWater;
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
    Vector3 launchVel;
    /// <summary>
    /// Which button to look for to check for button held
    /// </summary>
    string buttonCheck = "";
    /// <summary>
    /// Speed to determine the ball came to a stop
    /// </summary>
    const float END_SPEED = 0.1f;
    /// <summary>
    /// Force to launch the ball
    /// </summary>
    const float LAUNCH_FORCE = 4.0f;
    /// <summary>
    /// Minimum magnitude to be considered a launch
    /// </summary>
    const float MIN_MAG = 0.3f;
    /// <summary>
    /// Distance to determine the participant is flicking the ball
    /// </summary>
    const float FLICK_DIST = 0.3f;
    /// <summary>
    /// Time in seconds to display a prompt
    /// </summary>
    const float DISPLAY_TIME = 0.5f;
    /// <summary>
    /// Width of the line rendered visible ball path complete
    /// </summary>
    const float LINE_SIZE = 0.025f;
    const float BALL_MAX_ANGULAR_VEL = 240.0f;
    bool hitTarget = false;

    float currentAngle = 0.0f;
    string currentType = "";

    float waterSpeedJson = 0.0f;
    float waterInertia = 0.0f;
    float otherWaterSpeed = 0.0f;
    float otherWaterInertia = 0.0f;
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
    Vector3 throwVel;
    Vector2 absTurning;
    string finalBallState;
    float targetScale;
    float waterSpeed;
    GameObject poleOne;
    GameObject poleTwo;
    float targetWidth;

    private List<Vector3> handPositions = new List<Vector3>();
    private int pointsToConsider = 4;
    private float timeToWait = 0.0f;
    private Coroutine coroutine;
    private bool isFisrtInvisible = true;
    private GameObject invisibleTrailCard;
    bool isMainBallComplete = false;
    bool isInviBallComplete = false;
    Target targetScript;
    BallMovement ballMovement;
    BallMovement invisibleBallMovement;

    // Start is called before the first frame update
    void Start()
    {
        trialsRemaining = ExperimentController.Instance.GetTotalTrials();
        targetScript = target.transform.GetChild(0).GetComponent<Target>();
    }

    Vector3 CalculateThrowDirectionSimplified()
    {
        if (handPositions.Count < 2)
            return Vector3.zero;

        Vector3 directionSum = Vector3.zero;
        int validPoints = Mathf.Min(pointsToConsider, handPositions.Count - 1);

        // Average deltas of the last few points
        for (int i = handPositions.Count - validPoints; i < handPositions.Count - 1; i++)
        {
            directionSum += handPositions[i + 1] - handPositions[i];
        }

        Vector3 direction = directionSum / validPoints;
        return direction.normalized;
    }

    void FixedUpdate()
    {
        switch (currentStep)
        {
             case 1:
                {
                    //If the button is pressed again
                    if(Input.GetButtonDown(buttonCheck))
                    {
                        startPos = GetMousePos();
                        cursorPos = startPos;
                        handPos.Clear();
                        handPositions.Clear();
                        handPositions.Add(cursorPos);
                        handPos.Add(new Vector4(cursorPos.x, cursorPos.y, cursorPos.z, Time.time));
                    }
                    //If button released early
                    else if (Input.GetButton(buttonCheck))
                    {
                        cursorPos = GetMousePos();
                        handPositions.Add(cursorPos);
                        handPos.Add(new Vector4(cursorPos.x, cursorPos.y, cursorPos.z, Time.time));
                    }
                    //Debug.Log("Distance from start: " +Vector3.Distance(cursorPos, startPos));

                    launchVec = CalculateThrowDirectionSimplified();
                    if (Vector3.Distance(cursorPos, startPos) > FLICK_DIST && launchVec.z > 0)
                    {
                        // ballRB.isKinematic = false;
                        // ballRB.useGravity = true;

                        //If using VR get the hand velocity for launch if not use the LAUNCH_FORCE constant
                        throwVel = ExperimentController.Instance.UseVR ? launchVec.magnitude * launchVec * LAUNCH_FORCE: launchVec * LAUNCH_FORCE * launchVec.magnitude;
                        throwVel.y = 0.0f;

                        launchVel = throwVel;

                        if (launchVel.magnitude < MIN_MAG)
                        {
                            Debug.Log("The launch force was too small, applying a new force");
                            Debug.Log("New force " + throwVel * 2.0f);
                            Debug.Log("New force mag " + (throwVel * 2.0f).magnitude);

                            launchVel = throwVel * 2.0f;
                        }
                        ballMovement.velocity = launchVel;
                        invisibleBallMovement.velocity = launchVel;
                        cursor.SetActive(false);

                        IncrementStep();

                        stepTime.Add(Time.time);
                    }
                }
                break;
        }
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(timeToWait); 

        if(invisibleTrailCard.activeInHierarchy)
        {
            invisibleTrailCard.SetActive(false);
        }

        //If we are using VR use the VR hand position else get the
        //converted mouse position
        startPos = GetMousePos();
        cursorPos = startPos;

        IncrementStep();

        //increment step time
        stepTime.Add(Time.time);
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //Participant returns to home
            case 0:
                if(coroutine == null)
                {
                    coroutine = StartCoroutine(DelayStart());
                }

                break;
            #region Launch ball
            //Track cursor(hand position) and launch when certain distance from home
            case 1:
                {
                    
                }
                break;
            #endregion
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                {
                    
                    if(waterSpeedJson >= 0 && absTurning.x < ball.transform.position.x)
                    {
                        absTurning = new Vector2 (ball.transform.position.x, ball.transform.position.z);
                    }
                    else if(waterSpeedJson < 0 && absTurning.x > ball.transform.position.x)
                    {
                        absTurning = new Vector2 (ball.transform.position.x, ball.transform.position.z);
                    }
                    DebugDrawLaunchVec();
                    ClosestPointToTarget(ball.transform.position);
                    /*
                    Vector3 toTarget = target.transform.position - home.transform.position;
                    Vector3 toBall = target.transform.position - ball.transform.position;
                    float dot = Vector3.Dot(toTarget, toBall);
                    */
                    Vector3 skewedPos = new Vector3(ball.transform.position.x, home.transform.position.y - ball.GetComponent<SphereCollider>().bounds.size.y * 3/4, ball.transform.position.z);
                    Vector3 otherSkewedPos = new Vector3(otherBall.transform.position.x, home.transform.position.y - otherBall.GetComponent<SphereCollider>().bounds.size.y * 3/4, otherBall.transform.position.z);
                    globalBallPos.Add(new Vector3(ball.transform.position.x, home.transform.position.y - ball.GetComponent<SphereCollider>().bounds.size.y * 3/4, ball.transform.position.z));
                    
                    string displayMsg = "";
                    int points = 0;

                    //Ball hit the target
                    if (targetScript.TargetHit)
                    {
                        isMainBallComplete = true;
                        // ballRB.isKinematic = true;

                        finalBallState = "Hit";

                        lineColor = Color.green;
                        hitTarget = true;
                        
                        points = CalculatePoints();
                        totalScore += points;

                        displayMsg = "Target hit\n" + points + " points";

                        prefabAudio.clip = correctAudioClip;

                        closestDistance = 0.0f;
                    }
                    //Ball slowed down
                    else if (ballMovement.velocity.magnitude <= END_SPEED)
                    {
                        isMainBallComplete = true;
                        // ballRB.isKinematic = true;

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
                    }
                    //Ball went out of bounds
                    else
                    {
                        foreach (Target t in outOfBoundsCollider)
                        {
                            if (t.TargetHit)
                            {
                                isMainBallComplete = true;
                                // ballRB.isKinematic = true;

                                finalBallState = "Missed";

                                lineColor = Color.red;
                                hitTarget = false;

                                points = 0;
                                displayMsg = "Ball out of bounds\n0 points";

                                prefabAudio.clip = incorrectAudioClip;

                                break;
                            }
                        }
                    }

                    if(targetScript.OtherTargetHit)
                    {
                        isInviBallComplete = true;
                    }

                    else if(invisibleBallMovement.velocity.magnitude <= END_SPEED)
                    {
                        isInviBallComplete = true;
                    }

                    else
                    {
                        foreach (Target t in outOfBoundsCollider)
                        {
                            if(t.OtherTargetHit)
                            {
                                isInviBallComplete = true;
                            }
                        }
                    }

                    if(!isMainBallComplete)
                    {
                        ballPos.Add(skewedPos);
                        ballTime.Add(Time.time);
                    }
                    if(!isInviBallComplete)
                    {
                        otherBallPos.Add(otherSkewedPos);
                        invisBallTime.Add(Time.time);
                    }

                    if(isMainBallComplete && isInviBallComplete)
                    {
                        ballAudio.Stop();
                        ShowFeedback(points, displayMsg);
                        IncrementStep();

                        stepTime.Add(Time.time);

                        isMainBallComplete = false;
                        isInviBallComplete = false;
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
        int points = 0;

        if (hitTarget)
        {
            points = 5;
        }
        else
        {
            float targetWidth = target.GetComponent<MeshRenderer>().bounds.size.x;

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
        Collider targetCollider = target.transform.GetChild(0).GetComponent<Collider>();
        Vector3 closestPoint = targetCollider.ClosestPoint(location);
        float distance = Vector3.Distance(location, closestPoint);
        
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestBallPosToTarget = new Vector2(ball.transform.position.x, ball.transform.position.z);
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;
        int currBlock = ExperimentController.Instance.Session.currentBlockNum - 1;

        if (!ball || !otherBall)
        {
            ball = GameObject.Find("Ball");
            otherBall = GameObject.Find("OtherBall");
        }
        ballMovement = ball.GetComponent<BallMovement>();  
        ballMovement.Reset();
        invisibleBallMovement = otherBall.GetComponent<BallMovement>();
        invisibleBallMovement.Reset();  

        if(outOfBoundsCollider.Count == 0)
            Debug.LogWarning("No out of bounds colliders set");


        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
        visBallTravelPath = GetComponent<LineRenderer>();

        otherBall.transform.position = ball.transform.position;

        //Set the renderer for pinpall path
        visBallTravelPath.startWidth = visBallTravelPath.endWidth = LINE_SIZE;

        targetScript = target.transform.GetChild(0).GetComponent<Target>();

        if (targetAngles.Count == 0)
        {
            targetAngles = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }
        targetScale = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("per_block_target_width")[currBlock];
        target.transform.localScale = new Vector3(targetScale * target.transform.localScale.x, targetScale * target.transform.localScale.y, target.transform.localScale.z);

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

        if(!water || !otherWater)
        {
            water = GameObject.Find("Water");
            otherWater = GameObject.Find("OtherWater");
        }
            

        CurrentForce currentForce = water.GetComponent<CurrentForce>();
        CurrentForce otherCurrentForce = otherWater.GetComponent<CurrentForce>();
        

        waterSpeedJson = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_speed")[currBlock];
        ballMovement.waterSpeed = waterSpeedJson;
        currentForce.sideForce = waterSpeedJson;


        otherWaterSpeed = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_otherBall_water_speed")[currBlock];
        invisibleBallMovement.waterSpeed = otherWaterSpeed;
        otherCurrentForce.sideForce = otherWaterSpeed;

        debrisSpawner = GameObject.Find("DebrisSpawner").GetComponent<DebrisSpawner>();
        debrisSpawner.speed = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_speed")[currBlock];
        debrisSpawnRate = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("per_block_debris_spawn_rate")[currBlock];
        debrisCount = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_debris_count")[currBlock];
        debrisSpawner.spawnRate = debrisSpawnRate;
        debrisSpawner.debrisCount = debrisCount;

        if(waterSpeedJson >= 0)
        {
            absTurning.x = float.MinValue;
        }
        else
        {
            absTurning.x = float.MaxValue;
        }

        waterSpeed = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("per_block_water_speed")[currBlock];
        GameObject waterSurface = GameObject.Find("WaterSurface");
        waterSurface.GetComponent<Renderer>().material.SetFloat("_Speed", -waterSpeed/20);

        //Adjusted water audio based on current water force
        if (waterSpeedJson > 0.0f || waterSpeedJson < 0.0f)
        {
            waterAudio.volume = 0.5f;

            if (waterSpeedJson > 30.0f)
            {
                float forceDiff = Math.Abs(waterSpeedJson) - 30.0f;
                float pitchAdjust = forceDiff / 50.0f;

                waterAudio.pitch = 1.0f + pitchAdjust;
            }
            else if (waterSpeedJson < 30.0f)
            {
                
                float forceDiff = 30.0f - Math.Abs(waterSpeedJson);
                float pitchAdjust = forceDiff / 50.0f;

                waterAudio.pitch = 1.0f - pitchAdjust;
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
        invisibleTrailCard = GameObject.Find("InvisibleTrialCard");
    }

    float ComputeWaterForce(float waterSpeed, float objectArea, float dragCoefficient = 0.47f, float waterDensity = 1000f)
    {
        return 0.5f * dragCoefficient * waterDensity * objectArea * (waterSpeed * waterSpeed);
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        ball.GetComponent<MeshRenderer>().enabled = true;
        closestDistance = float.MaxValue;

        if(taskType == "invisible")
        {
            if(isFisrtInvisible)
            {
                invisibleTrailCard.SetActive(true);
                timeToWait = 3.0f;
                isFisrtInvisible = false;
            }
            else
            {
                invisibleTrailCard.SetActive(false);
                timeToWait = 0;
            }
        }
        else
        {
            invisibleTrailCard.SetActive(false);
            isFisrtInvisible = true;
            timeToWait = 0;
        }

        startPos = Vector3.zero;
        endPos = Vector3.zero;

        hitTarget = false;
        
        ball.transform.position = home.transform.position;
        ball.transform.rotation = Quaternion.identity;

        otherBall.transform.position = home.transform.position;
        otherBall.transform.rotation = Quaternion.identity;

        coroutine = null;
        ballMovement.Reset(); 
        invisibleBallMovement.Reset();
        handPos.Clear();
        otherBallPos.Clear();
        handPositions.Clear();
        ballPos.Clear();
        globalBallPos.Clear();
        ballTime.Clear();
        invisBallTime.Clear();
        stepTime.Clear();
        lineColor = Color.white;
        visBallTravelPath.positionCount = 0;
        visBallTravelPath.SetPositions(ballPos.ToArray());

        targetScript = target.transform.GetChild(0).GetComponent<Target>();

        //Setup target position
        targetScript.ResetTarget();

        foreach (Target t in outOfBoundsCollider)
            t.ResetTarget();

        displayText.text = "";
        ballDisplayText.text = "";;

        // target.transform.position = Vector3.zero;
        // target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        float z = target.transform.position.z;
        // calculate x position based on angle
        float x = Mathf.Tan(targetAngles[currentTrial] * Mathf.Deg2Rad) * z;    
        target.transform.position = new Vector3(x, target.transform.position.y, z);
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

        if(currentStep == 1 && taskType == "invisible")
        {
            ball.GetComponent<MeshRenderer>().enabled = false;
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
        session.CurrentTrial.result["target_hit"] = targetScript.TargetHit;
        session.CurrentTrial.result["invisibleBall_target_hit"] = targetScript.OtherTargetHit;
        session.CurrentTrial.result["final_ball_state"] = finalBallState;
        session.CurrentTrial.result["type"] = currentType;
        //session.CurrentTrial.result["target_position"] = target.transform.position;
        session.CurrentTrial.result["target_position_x"] = target.transform.position.x;
        session.CurrentTrial.result["target_position_y"] = target.transform.position.y;
        session.CurrentTrial.result["target_position_z"] = target.transform.position.z;
        session.CurrentTrial.result["target_angle"] = currentAngle;
        session.CurrentTrial.result["target_width"] = targetWidth;
        session.CurrentTrial.result["launch_direction"] = launchVec;

        session.CurrentTrial.result["current_force"] = waterSpeedJson;
        session.CurrentTrial.result["water_inertia"] = waterInertia;
        session.CurrentTrial.result["water_speed_m/s"] = waterSpeed;

        session.CurrentTrial.result["invisibleBall_current_force"] = otherWaterSpeed;
        session.CurrentTrial.result["invisibleBall_water_inertia"] = otherWaterInertia;

        session.CurrentTrial.result["launch_angle"] = Vector3.Angle(Vector3.right, launchVec);
        session.CurrentTrial.result["launch_Speed"] = throwVel.magnitude;

        session.CurrentTrial.result["ball_pos_x"] = string.Join(",", ballPos.Select(i => string.Format($"{i.x:F6}")));
        session.CurrentTrial.result["ball_pos_z"] = string.Join(",", ballPos.Select(i => string.Format($"{i.z:F6}")));
        session.CurrentTrial.result["ball_time"] = string.Join(",", ballTime.Select(i => string.Format($"{i:F6}")));
        session.CurrentTrial.result["final_ball_pos_x"] = ballPos[ballPos.Count - 1].x;
        session.CurrentTrial.result["final_ball_pos_z"] = ballPos[ballPos.Count - 1].z;
        session.CurrentTrial.result["turning_absolute_x"] = absTurning.x;
        session.CurrentTrial.result["turning_absolute_y"] = absTurning.y;

        session.CurrentTrial.result["invisibleBall_pos_x"] = string.Join(",", otherBallPos.Select(i => string.Format($"{i.x:F6}")));
        session.CurrentTrial.result["invisibleBall_pos_z"] = string.Join(",", otherBallPos.Select(i => string.Format($"{i.z:F6}")));
        session.CurrentTrial.result["invisibleBall_time"] = string.Join(",", invisBallTime.Select(i => string.Format($"{i:F6}")));
        session.CurrentTrial.result["final_invisibleBall_pos_x"] = otherBallPos[otherBallPos.Count - 1].x;
        session.CurrentTrial.result["final_invisibleBall_pos_z"] = otherBallPos[otherBallPos.Count - 1].z;


        session.CurrentTrial.result["distance_from_target"] = closestDistance;
        session.CurrentTrial.result["min_pos_from_target_x"] = closestBallPosToTarget.x;
        session.CurrentTrial.result["min_pos_from_target_z"] = closestBallPosToTarget.y;

        session.CurrentTrial.result["total_score"] = totalScore;

        for(int i = 0; i < stepTime.Count; i++)
        {
            session.CurrentTrial.result["step_" + i + "_time"] = stepTime[i]; 
        }
    }
}
