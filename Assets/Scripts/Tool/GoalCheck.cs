using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    // Start is called before the first frame update
    public bool colliding = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Tool"){
            colliding = true;
            
        }
        Debug.Log(other.tag);
        
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Tool"){
            colliding = false;
        }
    }
}
