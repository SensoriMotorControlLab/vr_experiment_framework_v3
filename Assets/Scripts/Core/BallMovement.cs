using System.Collections;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.Items.LockRules;
using TMPro;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    float f = 0;  // Fraction of the ball in water
    float CdAir = 0.47f;   // Drag coefficient for air (approximation)
    float CdWater = 0.47f; // Could be similar or adjusted based on shape and roughness in water
    float rhoAir = 1.225f;   // Air density (kg/m^3)
    float rhoWater = 1000f;  // Water density (kg/m^3)
    float mass = 1.0f;  // Mass of the ball (kg)
    float velocityMagnitude; // Magnitude of the velocity vector
    float radius;  // Radius of the ball
    float area; // Cross-sectional area of the ball
    SphereCollider col; // Sphere collider component
    public float waterSpeed;    // Speed of the water current

    public Vector3 velocity = Vector3.zero; // Velocity of the ball
    public bool canSimulate = false;  // Flag to enable/disable simulation
    float startingHeight = 0;   // Starting height of the ball
    float distToReachWater = 0.1f;  // Distance to reach the water surface

    void Start()
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
        area = Mathf.PI * radius * radius;
        startingHeight = transform.position.y;
    }

    void FixedUpdate()
    {
        // Check if simulation is enabled
        if(!canSimulate)
        {
            return;
        }
        // f = FractionInWater();
        ApplyWaterCurrentForce();
        transform.position += velocity * Time.fixedDeltaTime;

        if(velocity.magnitude > 0)
        {
            float y = transform.position.y;
            CalculateDeceleration();
            if(velocity.magnitude < 0)
            {
                velocity = Vector3.zero;
            }
            // if(transform.position.y > -0.05f)
            // {
            //     // velocity.y += -9.81f * Time.fixedDeltaTime;
            //     y = Mathf.Lerp(startingHeight, -0.05f, Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(0, 0)) / distToReachWater );
            // }
            // else
            // {
            //     // velocity.y = 0;
            //     y = -0.05f;
            // }

            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            
        }
    }
    // Calculate the deceleration of the ball based on the drag forces in air and water
    void CalculateDeceleration()
    {
        // Compute drag forces for each medium:
        velocityMagnitude = velocity.magnitude;
        float dragAir = ComputeForce(velocityMagnitude, area, CdAir, rhoAir);
        float dragWater = ComputeForce(velocityMagnitude, area, CdWater, rhoWater);
        
        // Weighted total drag:
        float totalDragForce = (0.5f) * dragAir + (0.5f * dragWater);

        // Deceleration (a scalar value; ensure you maintain the vector direction):
        float deceleration = totalDragForce / mass;

        // Calculate the change in speed over the frame:
        float deltaSpeed = deceleration * Time.fixedDeltaTime;
        // Ensure we don't reverse the velocity if deltaSpeed is larger than the current speed:
        float newSpeed = Mathf.Max(velocityMagnitude - deltaSpeed, 0);
        velocity = velocity.normalized * newSpeed;
    }
    // Apply the force of the water current on the ball
    private void ApplyWaterCurrentForce()
    {
        // Compute the drag force in water and apply it to the ball
        float waterForce = ComputeForce(waterSpeed, area, CdWater, rhoWater);
        // Compute the total force based on the fraction of the ball in water
        float totalForce = 0.5f * waterForce;
        float acc = totalForce / mass;
        float dealtaSpeed = acc * Time.fixedDeltaTime;
        // Apply the force in the direction of the water current
        velocity.x += dealtaSpeed * -1;
    }
    // Compute the drag force based on the speed, object area, drag coefficient, and medium density
    private float ComputeForce(float speed, float objectArea, float dragCoefficient, float mediumDensity)
    {
        // TODO: CHECK IF AREA IS DIVIDED BY 2
        return 0.5f * dragCoefficient * mediumDensity * (objectArea/2) * (speed * speed);
    }
    // Compute the fraction of the ball that is submerged in water
    private float FractionInWater()
    {
        float height = transform.position.y - radius;
        float fraction = 0;
        if(height < -0.05f)
        {
            fraction = Mathf.Lerp(0, 0.5f, transform.position.y/ -0.05f);
        }

        return fraction;
    }
}
