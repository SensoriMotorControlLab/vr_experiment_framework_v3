using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private static InputHandler instance;
    private Dictionary<string, GameObject> vrHands = new Dictionary<string, GameObject>();
    private Dictionary<string, RawInput> inputDevices = new Dictionary<string, RawInput>();
    private string domHand = "RightHand";

    public struct RawInput
    {
        public Vector3 position;
        public Quaternion rotation;
        public InputDevice inputDevice;

        public RawInput(Vector3 position, Quaternion rotation, InputDevice inputDevice = null)
        {
            this.position = position;
            this.rotation = rotation;
            this.inputDevice = inputDevice;
        }
    }

    public static InputHandler Instance
    {
        get { return instance; }
    }

    public Dictionary<string, GameObject> VRHands
    {
        get { return vrHands; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
        {
            instance = this;
        }

        foreach (InputDevice d in InputSystem.devices)
        {
            Debug.Log(d.name);
            inputDevices[d.name] = new RawInput(Vector3.zero, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Dictionary<string, RawInput> GetInputDevices()
    {
        return inputDevices;
    }

    public Dictionary<string, GameObject> GetVRHands()
    {
        return vrHands;
    }

    public void FindHandAnchors()
    {
        vrHands["LeftHand"] = GameObject.Find("LeftHand Controller");
        vrHands["RightHand"] = GameObject.Find("RightHand Controller");

        if (!vrHands["LeftHand"])
        {
            Debug.LogWarning("No GameObject found for left hand found");
        }
        if (!vrHands["RightHand"])
        {
            Debug.LogWarning("No GameObject found for right hand found");
        }

        //Debug.Log(vrHands["LeftHand"]);
        //Debug.Log(vrHands["RightHand"]);
    }

    public Vector3 GetHandPosition(string handName)
    {
        if (vrHands[handName])
        {
            return vrHands[handName].transform.position;
        }

        return Vector3.zero;
    }

    public void SetDominantHand(string newDomHand)
    {
        domHand = newDomHand;
    }

    public GameObject GetDominantHand()
    {
        return vrHands[domHand];
    }

    public string GetDominantHandString()
    {
        return domHand;
    }

    public Vector3 GetHandPosition()
    {
        if (vrHands[domHand])
        {
            return vrHands[domHand].transform.position;
        }

        return Vector3.zero;
    }
}
