using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    private GameObject pickedObject;
    public Camera playerCamera;  // Reference to the camera used for the raycast
    public float pickupRange = 5f;  // Max distance to pick up object
    public float moveSpeed = 10f;   // Speed to move object towards the mouse position
    private float initialYPosition;  // To store the object's initial Y position

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button
        {
            // Try to pick up an object
            TryPickUpObject();
        }

        if (Input.GetMouseButtonUp(0))  // Release the mouse button
        {
            // Drop the object if one is picked
            DropObject();
        }

        if (pickedObject != null)
        {
            // Move the picked object with the mouse but keep Y position constant
            MoveObjectWithMouse();
        }
    }

    void TryPickUpObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if we hit an object in range
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                // Check if the object has the "Goal" tag, and don't pick it up if it does
                if (hit.collider.gameObject.CompareTag("Goal"))
                {
                    Debug.Log("Cannot pick up object with 'Goal' tag.");
                    return; // Do nothing if the object is tagged "Goal"
                }

                pickedObject = hit.collider.gameObject; // Store the picked object
                pickedObject.GetComponent<Rigidbody>().isKinematic = true; // Disable physics
                initialYPosition = pickedObject.transform.position.y; // Store the initial Y position
            }
        }
    }

    void DropObject()
    {
        if (pickedObject != null)
        {
            pickedObject.GetComponent<Rigidbody>().isKinematic = false; // Re-enable physics
            pickedObject = null; // Clear the reference
        }
    }

    void MoveObjectWithMouse()
    {
        // Get the position of the mouse in the world
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Move object towards the mouse position in the world, but lock the Y-axis
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            Vector3 targetPosition = hit.point;
            targetPosition.y = initialYPosition; // Keep the Y position constant
            pickedObject.transform.position = Vector3.Lerp(pickedObject.transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }
}
