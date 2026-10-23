using System.Collections;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.Items.LockRules;
using TMPro;
using UnityEngine;

public class BallMovement2026 : MonoBehaviour
{
    float f = 0;  // Fraction of the ball in water
    float CdAir = 0.47f;   // Drag coefficient for air (approximation)
    float CdWater = 0.47f; // Could be similar or adjusted based on shape and roughness in water
    float rhoAir = 1.225f;   // Air density (kg/m^3)
    float rhoWater = 100f;  // Water density (kg/m^3)
    float CdRho; // initialize
    float mass = 1.0f;  // Mass of the ball (kg)
    float velocityMagnitude; // Magnitude of the ball velocity vector
    float radius;  // Radius of the ball
    float area; // Cross-sectional area of the ball
    SphereCollider col; // Sphere collider component
    public float waterSpeed;    // Speed of the water current

    public Vector3 velocity = Vector3.zero; // Velocity of the ball
    public Vector3 waterVelocity; // velocity of water current
    public bool canSimulate = false;  // Flag to enable/disable simulation
    float startingHeight = 0;   // Starting height of the ball
    float distToReachWater = 0.1f;  // Distance to reach the water surface


    void Start() // every trial?
    {
        Reset();
        startingHeight = transform.position.y;

    }

    public void Reset()
    {
        f = 0;
        velocity = Vector3.zero;
        canSimulate = false;
        col = GetComponent<SphereCollider>();
        radius = col.radius * transform.lossyScale.x;
        area = 0.5f * (Mathf.PI * radius * radius);
        startingHeight = transform.position.y;
        CdRho = CdWater * rhoWater;

    }


    void FixedUpdate()
    {

        // Check if simulation is enabled
        if(!canSimulate)
        {
            return;
        }


        waterVelocity = Vector3.right * waterSpeed;

        velocity = updateVelocity();

        velocityMagnitude = velocity.magnitude;
        transform.position += velocity * Time.fixedDeltaTime;

    }


    Vector3 updateVelocity()
    {

        Vector3 relativeVelocity = velocity - waterVelocity; // newtonian accuarte: including the drag of the water, can inlcude NO drag when both velocities; absolute of v_rel
        Debug.Log($"relative velocity: {relativeVelocity}");
        
        Vector3 F = (CdRho * area * relativeVelocity.magnitude * relativeVelocity) / mass;

        return velocity - (F * Time.fixedDeltaTime);

    }

}





