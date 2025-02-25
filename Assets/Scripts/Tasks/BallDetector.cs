using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDetector : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        BallMovement bm = other.GetComponent<BallMovement>();
        if(bm)
        {
            bm.velocity = Vector3.zero;
            bm.canSimulate = false;
            return;
        }

        if (other.gameObject.tag == "Tool")
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        BallMovement bm = collision.gameObject.GetComponent<BallMovement>();
        if(bm)
        {
            bm.velocity = Vector3.zero;
            bm.canSimulate = false;
            return;
        }

        if (collision.gameObject.tag == "Tool")
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
    }
}
