using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleTarget : MonoBehaviour
{
    
    public List<GameObject> tools = new List<GameObject>();
    public List<GameObject> targets = new List<GameObject>();

    GameObject collidingTool;
    GameObject collidingTarget;

    bool isColliding = false;
    bool isTriggered = false;

    bool isToolColliding = false;
    bool isTargetColliding = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if(g == collision.gameObject)
            {
                collidingTool = g;
                isToolColliding = true;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == collision.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        /*
        isColliding = true;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if (g == collision.gameObject)
            {
                collidingTool = g;
                isToolColliding = true;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == collision.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = true;
            }
        }
        */
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if (g == collision.gameObject)
            {
                collidingTool = g;
                isToolColliding = false;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == collision.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if (g == other.gameObject)
            {
                collidingTool = g;
                isToolColliding = true;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == other.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        /*
        isTriggered = true;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if (g == other.gameObject)
            {
                collidingTool = g;
                isToolColliding = true;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == other.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = true;
            }
        }
        */
    }


    private void OnTriggerExit(Collider other)
    {
        isTriggered = false;

        //Check if object is tool
        foreach (GameObject g in tools)
        {
            if (g == other.gameObject)
            {
                collidingTool = g;
                isToolColliding = false;
            }
        }

        //Check if object is tool
        foreach (GameObject g in targets)
        {
            if (g == other.gameObject)
            {
                collidingTarget = g;
                isTargetColliding = false;
            }
        }
    }

    public bool IsColliding
    {
        get
        {
            return isColliding;
        }
    }

    public bool IsTriggered
    {
        get
        {
            return isTriggered;
        }
    }

    public GameObject CollidingTool
    {
        get
        {
            return collidingTool;
        }
    }

    public bool IsToolCollding
    {
        get
        {
            return isToolColliding;
        }
    }

    public bool IsTargetCollding
    {
        get
        {
            return isTargetColliding;
        }
    }

    public GameObject CollidingTarget
    {
        get
        {
            return collidingTarget;
        }
    }

    public void ResetState()
    {
        isColliding = false;
        isTriggered = false;

        isTargetColliding = false;
        isToolColliding = false;
    }

    public void ClearLists()
    {
        tools.Clear();
        targets.Clear();
    }
}
