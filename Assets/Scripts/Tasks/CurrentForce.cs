using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentForce : MonoBehaviour
{
    public float forwardForce;
    public float sideForce;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tool"){
            other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward*forwardForce*1000);
            other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left*sideForce*1000);

        }
    }
}
