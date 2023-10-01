using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controller for the experiment cursor
/// </summary>
public class CursorController : MonoBehaviour
{
    private static CursorController instance = null;
    private Dictionary<string,GameObject> vrHands = new Dictionary<string, GameObject>();
    private string domHand = "LeftHand";
    GameObject cursor;
    MovementType moveType;

    /// <summary>
    /// The behaviour of the cursor
    /// </summary>
    public enum MovementType
    {
        aligned,
        rotated,
        clamped
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
            instance = this;

        vrHands["LeftHand"] = GameObject.Find("LeftHandAnchor");
        vrHands["RightHand"] = GameObject.Find("RightHandAnchor");

        if (!vrHands["LeftHand"])
            Debug.LogError("No GameObject for left hand found");

        vrHands["RightHand"] = GameObject.Find("RightHandAnchor");

        if (!vrHands["RightHand"])
            Debug.LogError("No GameObject for right hand found");

        moveType = MovementType.aligned;
    }

    // Update is called once per frame
    void Update()
    {
        //if not use vr and both a camera and a cursor object has been set
        if (ExperimentController.Instance.UseVR == false && Camera.main && cursor)
        {
            //update the cursor posiiton 
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            pos.y = 0.0f;
            cursor.transform.position = pos;
        }
        else if(ExperimentController.Instance.UseVR == true)
        {
            cursor.transform.position = vrHands[domHand].transform.position;
        }

        if(cursor)
            ConvertCursorPosition();

    }

    private void LateUpdate()
    {
    }

    public static CursorController Instance
    {
        get 
        {
            if (!instance)
                Debug.LogWarning("ExperimentController is unitialized"); 
            return instance; 
        }
    }

    public GameObject Cursor
    {
        get { return cursor; }
        set { cursor = value; }
    }

    public Vector3 ConvertCursorPosition()
    {
        ExperimentController expCtrl = ExperimentController.Instance;
        GameObject t = GameObject.Find("Target");
        GameObject h = GameObject.Find("Home");
        Vector3 target = t != null ? t.transform.position : Vector3.zero;
        Vector3 home = h != null ? h.transform.position : Vector3.zero;

        switch (moveType)
        {
            case MovementType.aligned:
                return cursor.transform.position;
            case MovementType.rotated:
                float rotation = expCtrl.Session.CurrentBlock.settings.GetFloat("rotation");
                cursor.transform.position = Quaternion.Euler(0, -rotation, 0) * (cursor.transform.position - home) + home;
                return cursor.transform.position;
            case MovementType.clamped:
                Vector3 normal = target - home;

                // Rotate vector by 90 degrees to get plane parallel to the vector
                normal = Quaternion.Euler(0f, -90f, 0f) * normal;

                /*          ^  normal
                  target O  |
                          \ |
                           \|
                            \
                             \
                         home X
                 */

                cursor.transform.position = Vector3.ProjectOnPlane(cursor.transform.position - home, normal.normalized) + home;
                return cursor.transform.position;
            default:
                throw new System.ArgumentOutOfRangeException("No moveType defined for CursorController");
        }
    }
}
