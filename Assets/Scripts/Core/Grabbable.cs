using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    public GameObject parent;
    public GameObject prongs;
    public GameObject ball;
    Vector3 clampedCursor;
    Vector3 homePos;
    float GRAB_DISTANCE = 0.1f;
    const float PULL_DISTANCE = 2.5f;
    float speed = 0.25f;

    bool isGrabbed = false;
    bool isGrabbing = false;
    bool awayFromHome = false;
    [SerializeField]
    bool isLoaded = true;

    // Start is called before the first frame update
    void Start()
    {
        GRAB_DISTANCE = GetComponent<SphereCollider>().radius;
        ball.GetComponent<Rigidbody>().useGravity = false;
        homePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.Instance.UseVR == true)
        {
            bool primaryButton;
            bool secondaryButton;

            if (CursorController.Instance.GetDominantHand() == "LeftHand")
            {
                primaryButton = Input.GetButton("XRI_Left_PrimaryButton");
                secondaryButton = Input.GetButton("XRI_Left_SecondaryButton");

            }
            else
            {
                primaryButton = Input.GetButton("XRI_Right_PrimaryButton");
                secondaryButton = Input.GetButton("XRI_Right_SecondaryButton");
            }

            isGrabbing = primaryButton || secondaryButton;
        }
        else
        {
            bool leftMouse = Input.GetButton("Fire1");
            bool rightMouse = Input.GetButton("Fire2");


            isGrabbing = leftMouse || rightMouse;
        }

        clampedCursor = new Vector3(CursorController.Instance.Cursor.transform.position.x, transform.position.y, CursorController.Instance.Cursor.transform.position.z);

        float distance = Vector3.Distance(transform.position, clampedCursor);

        isGrabbed = (distance <= GRAB_DISTANCE && isGrabbing) || awayFromHome && isGrabbing;

        float distanceFromHome = Vector3.Distance(transform.position, homePos);

        awayFromHome = distanceFromHome > 0.1f;

        if (isGrabbed)
        {
            Vector3 dir = Vector3.Normalize(homePos - clampedCursor);

            parent.transform.rotation = Quaternion.LookRotation(dir, transform.up);
        }
    }

    public void RelooadSlingshot()
    {
        isLoaded = true;
        ball.SetActive(true);
    }

    private void LateUpdate()
    {
        float distance = Vector3.Distance(clampedCursor, homePos);

        if (isGrabbed)
        {
            if (distance <= PULL_DISTANCE)
                //transform.position = new Vector3(transform.position.x, transform.position.y, clampedCursor.z);
                transform.position = clampedCursor;

            //TODO hide the cursor
        }
        else if(!isGrabbed && awayFromHome)
        {
            transform.position = Vector3.Lerp(homePos, transform.position, speed * Time.deltaTime);
            distance = Vector3.Distance(transform.position, homePos);
            Debug.Log(distance);

            if(isLoaded && distance <= 0.1f)
            {
                isLoaded = false;
                ball.SetActive(false);
                GameObject shot = Instantiate(ball,ball.transform.position,ball.transform.rotation);
                shot.transform.localScale = ball.transform.lossyScale;
                shot.SetActive(true);
                shot.GetComponent<Rigidbody>().useGravity = true;
                shot.GetComponent<Rigidbody>().AddForce((transform.forward) * 1000.0f);
            }
        }
    }
}
