using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    /// <summary>
    /// The projectile meant to hit the target
    /// </summary>
    GameObject projectile;
    /// <summary>
    /// The collider for the target used to check for collision with the target
    /// </summary>
    Collider targetCollider;
    bool targetHit = false;

    // Start is called before the first frame update
    void Start()
    {
        targetCollider = GetComponent<Collider>();

        if (!targetCollider)
            Debug.LogWarning("TARGET COULD NOT FIND COLLIDER");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetProjectile(GameObject p)
    {
        projectile = p;
    }

    public void ResetTarget()
    {
        transform.position = Vector3.zero;
        targetHit = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Projectile")
            targetHit = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile")
            targetHit = true;
    }
}
