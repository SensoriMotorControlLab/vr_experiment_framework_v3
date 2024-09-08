using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject item;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == item) {
            Debug.Log(item.name + " has scored a point");
        }
    }
}
