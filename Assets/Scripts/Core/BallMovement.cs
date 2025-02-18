using System.Collections;
using System.Collections.Generic;
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
    float velocityMagnitude;
    float radius;
    float area;
    SphereCollider col;

    public Vector3 velocity = Vector3.zero;

    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        f = 0;
        velocity = Vector3.zero;
        col = GetComponent<SphereCollider>();
        radius = col.radius * transform.lossyScale.x;
        area = Mathf.PI * radius * radius;
    }

    void FixedUpdate()
    {
        transform.position += velocity * Time.fixedDeltaTime;
        if(velocity.magnitude > 0)
        {
            CalculateDeceleration();
            if(velocity.magnitude < 0)
            {
                velocity = Vector3.zero;
            }
            if(transform.position.y > -0.05f)
            {
                velocity.y += -9.81f * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y = 0;
                transform.position = new Vector3(transform.position.x, -0.05f, transform.position.z);
            }
            
        }
    }

    void CalculateDeceleration()
    {
        // Compute drag forces for each medium:
        velocityMagnitude = velocity.magnitude;
        float dragAir = 0.5f * CdAir * rhoAir * area * velocityMagnitude * velocityMagnitude;
        float dragWater = 0.5f * CdWater * rhoWater * area * velocityMagnitude * velocityMagnitude;
        float y = transform.position.y - radius;
        if(y < -0.05f)
        {
            Debug.Log("fraction of ball in Water: " + f);
            f = Mathf.Lerp(0, 0.5f, transform.position.y/ -0.05f);
        }
        

        // Weighted total drag:
        float totalDragForce = (1 - f) * dragAir + (f * dragWater);

        // Deceleration (a scalar value; ensure you maintain the vector direction):
        float deceleration = totalDragForce / mass;

        // Calculate the change in speed over the frame:
        float deltaSpeed = deceleration * Time.deltaTime;
        // Ensure we don't reverse the velocity if deltaSpeed is larger than the current speed:
        float newSpeed = Mathf.Max(velocityMagnitude - deltaSpeed, 0);
        velocity = velocity.normalized * newSpeed;
    }
}
