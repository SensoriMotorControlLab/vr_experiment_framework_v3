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
    List<Vector3> visBallPos = new List<Vector3>();
    /// <summary>
    /// Ball/Tool object
    /// </summary>
    [SerializeField]
    GameObject ball;
    /// <summary>
    /// Rigidboy of the ball
    /// </summary>
    Rigidbody ballRB;
    /// <summary>
    /// Collider to check if the pariticpant hit the ball backwards
    /// </summary>
    [SerializeField]
    Target wrongWayCollider;
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
    /// Speed to determine the ball came to a stop
    /// </summary>
    const float END_SPEED = 0.06f;
    /// <summary>
    /// Force to launch the ball
    /// </summary>
    const float LAUNCH_FORCE = 15.0f;
    /// <summary>
    /// Magnitude to cap the launch force
    /// </summary>
    const float LAUNCH_MAG = 0.25f;
    /// <summary>
    /// Distance to determine the participant is flicking the ball
    /// </summary>
    const float FLICK_DIST = 0.1f;
    /// <summary>
    /// Time in seconds to display a prompt
    /// </summary>
    const float DISPLAY_TIME = 1.5f;
    /// <summary>
    /// Width of the line rendered ball path complete
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
                if (/*Vector3.Distance(cursor.transform.position,home.transform.position) <= PRE_LAUNCH_DIST && */Input.GetButtonDown("Fire1"))
                {
                    Debug.Log("Button held");
                    Vector3 mouse = GetCursorScreenPercentage();
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
                    Vector3 mouse = GetCursorScreenPercentage();

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
                            endPos = InputHandler.Instance.GetHandPosition();
                            launchEndTime = Time.time;
                            float totalTime = launchEndTime - launchStartTime;

                            Vector3 launchVec = endPos - startPos;
                            launchVec.y = home.transform.position.y;
                            launchVec.Normalize();
                            launchVec = Quaternion.Euler(90, 0, 0) * launchVec;

                            Debug.Log("Launch time " + totalTime);
                            Debug.Log("Launch vector " + launchVec);

                            ballRB.isKinematic = false;
                            ballRB.useGravity = true;

                            Vector3 force = launchVec;
                            force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);
                            force *= LAUNCH_FORCE;
                            ballRB.velocity = force;
                            Debug.Log("Launch force " + force);
                            Debug.Log("Launch mag " + force.magnitude);
                            cursor.SetActive(false);

                            IncrementStep();
                        }
                    }
                    //If not using VR
                    else if (ExperimentController.Instance.UseVR == false)
                    {
                        //Launch the ball (2D)
                        if (Vector3.Distance(mouse, startPos) > FLICK_DIST || !Input.GetButton("Fire1"))
                        {
                            endPos = mouse;
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

                            Vector3 force = launchVec;
                            force = Vector3.ClampMagnitude(force / (totalTime * 50.0f), LAUNCH_MAG);
                            force *= LAUNCH_FORCE;

                            ballRB.velocity = force;
                            Debug.Log("Launch force " + force);
                            Debug.Log("Launch mag " + force.magnitude);
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
                    visBallPos.Add(ball.transform.position);

                    //Ball the hit target
                    if (target.GetComponent<Target>().TargetHit)
                    {
                        ballRB.isKinematic = true;
                        StartCoroutine(DisplayMessage("Target hit"));
                        lineColor = Color.green;
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
                        StartCoroutine(DisplayMessage("Missed target"));
                        IncrementStep();
                    }
                    //Ball slowed down
                    else if (ballRB.velocity.magnitude <= END_SPEED)
                    {
                        ballRB.isKinematic = true;
                        lineColor = Color.yellow;
                        StartCoroutine(DisplayMessage("Ball too slow"));
                        IncrementStep();
                    }
                }
                break;
            //Displaying feedback
            case 3:
                visBallTravelPath.positionCount = visBallPos.Count;
                visBallTravelPath.SetPositions(visBallPos.ToArray());
                visBallTravelPath.startColor = visBallTravelPath.endColor = lineColor;
                break;
        }
    }

    IEnumerator DisplayMessage(string displayMessage = "")
    {
        float delayTime = 0.0f;
        //Display feedback text here
        if(displayMessage.Length > 0)
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

        if(!ball)
            ball = GameObject.Find("Ball");
        if (!wrongWayCollider)
            wrongWayCollider = GameObject.Find("WrongWayCollider").GetComponent<Target>();

        ballRB = ball.GetComponent<Rigidbody>();
        ballRB.maxAngularVelocity = BALL_MAX_ANGULAR_VEL;
        CursorController.Instance.planeOffset = new Vector3(0.0f, -ball.transform.position.y, 0.0f);
        visBallTravelPath = GetComponent<LineRenderer>();

        //Set the renderer for pinpall path
        visBallTravelPath.startWidth = visBallTravelPath.endWidth = LINE_SIZE;
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

        handPos.Clear();
        visBallPos.Clear();
        lineColor = Color.white;
        visBallTravelPath.positionCount = 0;
        visBallTravelPath.SetPositions(visBallPos.ToArray());

        //Setup target position
        target.GetComponent<Target>().ResetTarget();
        wrongWayCollider.ResetTarget();

        displayText.text = "";
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
