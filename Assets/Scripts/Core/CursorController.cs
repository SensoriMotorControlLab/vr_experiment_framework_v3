using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controller for the experiment cursor
/// </summary>
public class CursorController : MonoBehaviour
{
    private static CursorController instance = null;
    GameObject cursor;

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
            instance = this;
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

        }
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
}
