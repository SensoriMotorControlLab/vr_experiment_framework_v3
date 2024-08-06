using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    /// Collider to check if the pariticpant hit the ball backwards
    /// </summary>
    [SerializeField]
    Target wrongWayCollider;
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
    string buttonCheck = "";
    /// <summary>
    /// Speed to determine the ball came to a stop
    /// </summary>
    const float END_SPEED = 0.06f;
    /// <summary>
    /// Force to launch the ball
    /// </summary>
    const float LAUNCH_FORCE = 25.0f;
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
    bool aimingBall = false;

    // Start is called before the first frame update
    void Start()
    {
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
            //Track cursor(hand position) and launch when certain distance from home
            case 1:
                {
                    //If button is pressed
                    if (Input.GetButton(buttonCheck))
                    {
                        Vector3 pos = GetMousePos();

                        handPos.Add(new Vector4(pos.x, pos.y, pos.z, Time.time));

                        aimingBall = true;
                    }

                    if (Vector3.Distance(GetMousePos(), startPos) > FLICK_DIST || !Input.GetButton(buttonCheck))
                    {
                        endPos = GetMousePos();
                        launchEndTime = Time.time;

                        Vector3 handVelocity = Vector3.zero;

                        if (ExperimentController.Instance.UseVR)
                        {
                            handVelocity = InputHandler.Instance.GetHandVelocity();
                        }

                        float totalTime = launchEndTime - launchStartTime;
                        Vector3 launchVec = endPos - startPos;
                        launchVec.Normalize();
                        launchVec = Quaternion.Euler(90, 0, 0) * launchVec;

                        aimingBall = false;

                        Debug.Log("Launch time " + totalTime);
                        Debug.Log("Launch vector " + launchVec);

                        ballRB.isKinematic = false;
                        ballRB.useGravity = true;

                        Vector3 force = launchVec;
                        force.y = 0.0f;
                        force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);
                        force *= LAUNCH_FORCE;

                        Vector3 launchForce = force;

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
                break;
            //Ball is launched, tracking for colliding with target, missing target, or slowing down
            case 2:
                {
                    float dist = Vector3.Distance(startPos, endPos);
                    Vector3 dir = endPos - startPos;
                    Debug.DrawRay(startPos, dir.normalized * dist, Color.red);

                    Vector3 toTarget = target.transform.position - home.transform.position;
                    Vector3 toBall = target.transform.position - ball.transform.position;
                    float dot = Vector3.Dot(toTarget, toBall);
                    ballPos.Add(ball.transform.position);

                    //Ball the hit target
                    if (target.GetComponent<Target>().TargetHit)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.green;
                        StartCoroutine(DisplayMessage("Target hit"));
                        IncrementStep();
                    }
                    else if (wrongWayCollider.TargetHit)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.red;
                        StartCoroutine(DisplayMessage("Wrong way"));
                        IncrementStep();
                    }
                    else if (dot <= 0.0f)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.white;
                        StartCoroutine(DisplayMessage("Missed target"));
                        IncrementStep();
                    }
                    //Ball slowed down
                    else if (ballRB.velocity.magnitude <= END_SPEED)
                    {
                        ballRB.isKinematic = true;

                        lineColor = Color.yellow;
                        StartCoroutine(DisplayMessage("Ball came to a stop"));
                        IncrementStep();
                    }
                }
                break;
            //Displaying feedback
            case 3:
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

        while(delayTime <= DISPLAY_TIME)
        {
            delayTime += Time.deltaTime;
            yield return null;
        }

        IncrementStep();
        yield return new WaitForEndOfFrame();
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;

        if (!ball)
            ball = GameObject.Find("Ball");

        if (!wrongWayCollider)
            wrongWayCollider = GameObject.Find("WrongWayCollider").GetComponent<Target>();

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
            buttonCheck = "XRI_Right_TriggerButton";
        }
        else
        {
            buttonCheck = "Fire1";
        }
        
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
        wrongWayCollider.ResetTarget();

        displayText.text = "";

        ballPlane = new Plane(ball.transform.up,ball.transform.position.y);

        //Debug.Log("target angle: " + targetAngles[currentTrial]);
        // target.transform.position = Vector3.zero;
        // target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        float z = target.transform.localPosition.z;
        // calculate x position based on angle
        float x = Mathf.Tan(targetAngles[currentTrial] * Mathf.Deg2Rad) * z;    
        target.transform.localPosition = new Vector3(x, target.transform.localPosition.y, z);
    }

    private Vector3 GetMousePos()
    {
        return ExperimentController.Instance.UseVR ? GetHandOnBallPlane() : GetCursorScreenPercentage();
    }

    private Vector3 GetCursorScreenPercentage()
    {
        return  new Vector3(InputHandler.Instance.GetPosition().x / Screen.width, InputHandler.Instance.GetPosition().y / Screen.height, 0);
    }

    private Vector3 GetHandOnBallPlane()
    {
        return Vector3.ProjectOnPlane(InputHandler.Instance.GetHandPosition(), ballPlane.normal) + Vector3.Dot(InputHandler.Instance.GetHandPosition(),ballPlane.normal) * ballPlane.normal;
    }

    private Vector3 GetHandVector()
    {
        return new Vector3(InputHandler.Instance.GetHandPosition().x, 0.0f, InputHandler.Instance.GetHandPosition().z);
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        
    }
}
