using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private static InputHandler instance;
    private Dictionary<string, GameObject> vrHands = new Dictionary<string, GameObject>();
    //private Dictionary<string, InputDeviceProperties> inputDevices = new Dictionary<string, InputDeviceProperties>();
    /// <summary>
    /// SortedDictionary of all inputDevices, sorted by lowest priority to highest
    /// </summary>
    private SortedDictionary<int, InputDeviceProperties> inputDevices = new SortedDictionary<int, InputDeviceProperties>();

    /// <summary>
    /// Highest priority for the InputHandler (usually VR touch controllers)
    /// </summary>
    private const int HIGHEST_PRIORITY = 100;
    /// <summary>
    /// Name Unity InputDevice associated with the headset device when using OpenXR
    /// </summary>
    public const string HEADSET_DEVICE_NAME = "HeadTrackingOpenXR";
    /// <summary>
    /// Name Unity InputDevice associated with the mouse
    /// </summary>
    public const string MOUSE_NAME = "Mouse";
    /// <summary>
    /// Name Unity InputDevice associated with the VR controllers when using OpenXR
    /// </summary>
    public const string TOUCH_CONTROLLER_NAME = "OculusTouchControllerOpenXR";

    /// <summary>
    /// Dominant hand to track
    /// </summary>
    private string domHand = "RightHand";

    public delegate Vector3 InputPositionUpdate();
    public delegate Quaternion InputRotationUpdate();

    public enum InputType
    {
        NONE,
        SPATIAL,
        RAY
    }

    public struct InputDeviceProperties
    {
        public Vector3 position;
        public Quaternion rotation;
        public InputDevice inputDevice;
        public InputType inputType;

        InputPositionUpdate inputPosUp;
        InputRotationUpdate inputRotUp;

        public InputDeviceProperties(Vector3 position, Quaternion rotation, InputType inputType, InputDevice inputDevice, InputPositionUpdate inputPosUp, InputRotationUpdate inputRotUp)
        {
            this.position = position;
            this.rotation = rotation;
            this.inputType = inputType;
            this.inputDevice = inputDevice;
            this.inputPosUp = inputPosUp;
            this.inputRotUp = inputRotUp;
        }

        public void Update()
        {
            UpdatePosition();
            UpdateRotation();

            //Debug.Log(inputDevice.name + " pos= " + position);
        }

        public void UpdatePosition()
        {
            if (inputPosUp != null)
            {
                position = inputPosUp();
            }
        }

        public void UpdateRotation()
        {
            if (inputRotUp != null)
            {
                rotation = inputRotUp();
            }
        }

        /*
        public override bool Equals(object obj)
        {
            InputDeviceProperties inputObj = (InputDeviceProperties)obj;

            return Equals(inputObj);
        }

        public bool Equals(InputDeviceProperties other)
        {
            return inputDevice == other.inputDevice && inputType == other.inputType;
        }

        public static bool operator ==(InputDeviceProperties lhs, InputDeviceProperties rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(InputDeviceProperties lhs, InputDeviceProperties rhs)
        {
            return !lhs.Equals(rhs);
        }
        */
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
        else
        {
            Destroy(gameObject);
        }

        //FindAllInputDevices();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var e in inputDevices.ToList())
        {
            InputDeviceProperties r = inputDevices[e.Key];
            r.Update();
            inputDevices[e.Key] = r;
            //Debug.Log(inputDevices[k].position);
        }
    }

    public SortedDictionary<int, InputDeviceProperties> GetInputDevices()
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

    public void FindAllInputDevices()
    {
        //TODO: remove this and do a better check of adding new devices and keeping already found ones
        inputDevices.Clear();

        int counter = 0;

        foreach (InputDevice d in InputSystem.devices)
        {
            string deviceName = d.name;

            switch (deviceName)
            {
                case (HEADSET_DEVICE_NAME):
                    inputDevices[HIGHEST_PRIORITY - 1] = new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.RAY, d, null, null);
                    break;
                case (TOUCH_CONTROLLER_NAME):
                    inputDevices[HIGHEST_PRIORITY] = new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.SPATIAL, d, GetHandPosition, GetHandRotation);
                    break;
                case (MOUSE_NAME):
                    inputDevices[counter] = new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.RAY, d, delegate { return Input.mousePosition; }, null);
                    counter++;
                    break;
                default:
                    inputDevices[counter] = new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.NONE, d, null, null);
                    counter++;
                    break;
            }

        }
    }

    public Vector3 GetHandPosition(string handName)
    {
        if (vrHands[handName])
        {
            return vrHands[handName].transform.position;
        }

        return Vector3.zero;
    }

    public Quaternion GetHandRotation(string handName)
    {
        if (vrHands[handName])
        {
            return vrHands[handName].transform.rotation;
        }

        return Quaternion.identity;
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

    /// <summary>
    /// Get dominant hand positions
    /// </summary>
    /// <returns></returns>
    public Vector3 GetHandPosition()
    {
        if (vrHands[domHand])
        {
            return vrHands[domHand].transform.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get dominant hand rotation
    /// </summary>
    /// <returns></returns>
    public Quaternion GetHandRotation()
    {
        if (vrHands[domHand])
        {
            return vrHands[domHand].transform.rotation;
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get the highest priority device connected and returns said devices position
    /// </summary>
    /// <returns>If proper device connected returns a Vector3</returns>
    public Vector3 GetPosition()
    {
        int highestPriority = inputDevices.Keys.Max(key => key);
        return inputDevices[highestPriority].position;
    }

    /// <summary>
    /// Get position of device related to passed priority
    /// </summary>
    /// <param name="priority">Priority of device</param>
    /// <returns>If device with given priority return said devices position else return zero vector</returns>
    public Vector3 GetPosition(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            return inputDevices[priority].position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get rotation of device related to passed priority
    /// </summary>
    /// <param name="priority">Priority of device</param>
    /// <returns>If device with given priority return said devices rotation else return identity quaternion</returns>
    public Quaternion GetRotation(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            return inputDevices[priority].rotation;
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get the highest priority device connected and returns said devices rotation
    /// </summary>
    /// <returns>If proper device connected returns a Quaternion</returns>
    public Quaternion GetRotation()
    {
        int highestPriority = inputDevices.Keys.Max(key => key);
        return inputDevices[highestPriority].rotation;
    }


    /// <summary>
    /// Get the highest priority device of RAY inputType and returns said devices position
    /// </summary>
    /// <returns>Ray origin position</returns>
    public Vector3 GetRayPosition()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.RAY)
            {
                return e.Value.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Is the ray casting onto the plane
    /// </summary>
    /// <param name="plane">Plane to check collision for</param>
    /// <returns>True if collision, false if not</returns>
    public bool IsRaycastColliding(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(GetRayPosition());
        float enter;

        if (plane.Raycast(ray, out enter))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get the hit position of a ray onto a plane
    /// </summary>
    /// <param name="plane">Plane to cast the ray onto</param>
    /// <returns>If ray hits plan return the hit position else return ray origin position</returns>
    public Vector3 GetRaycastPosition(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(GetRayPosition());
        float enter;

        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            return hitPoint;
        }

        return ray.origin;
    }

    /// <summary>
    /// Get the highest priority device of SPATIAL inputType and returns said devices position
    /// </summary>
    /// <returns>If spatial device return position of said device else zero vector</returns>
    public Vector3 GetSpatialPosition()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.SPATIAL)
            {
                return e.Value.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get the highest priority device of SPATIAL inputType and returns said devices rotation
    /// </summary>
    /// <returns>If spatial device return rotation of said device else identity quaternion</returns>
    public Quaternion GetSpatialRotation()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.SPATIAL)
            {
                return e.Value.rotation;
            }
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get the dictionary of devices
    /// </summary>
    /// <returns>Dictionary of devices</returns>
    public SortedDictionary<int, InputDeviceProperties> GetDevices()
    {
        return inputDevices;
    }

    public void SetDevicePriority(string deviceName, int newPriority)
    {
        foreach (var e in inputDevices.ToList())
        {
            InputDeviceProperties r = inputDevices[e.Key];

            if (r.inputDevice.name == deviceName)
            {
                int oldKey = e.Key;
                inputDevices[newPriority] = r;
                inputDevices.Remove(oldKey);
                break;
            }
        }
    }

    public void SetDevicePriority(InputDevice inputDevice, int newPriority)
    {
        foreach (var e in inputDevices.ToList())
        {
            InputDeviceProperties r = inputDevices[e.Key];

            if (r.inputDevice == inputDevice)
            {
                int oldKey = e.Key;
                inputDevices[newPriority] = r;
                inputDevices.Remove(oldKey);
                break;
            }
        }
    }

    public void ChangePriority(int oldPriority, int newPriority)
    {
        if (inputDevices.ContainsKey(oldPriority))
        {
            InputDeviceProperties theDevice = inputDevices[oldPriority];
            inputDevices[newPriority] = theDevice;
            inputDevices.Remove(oldPriority);
        }
    }

    public void AddInputDevice(int priority, Vector3 pos, Quaternion rot, InputType inputType, InputDevice inputDevice, InputPositionUpdate posUpdate, InputRotationUpdate rotUpdate)
    {
        InputDeviceProperties newDeviceProperty = new InputDeviceProperties(pos, rot, inputType, inputDevice, posUpdate, rotUpdate);

        if(!inputDevices.ContainsValue(newDeviceProperty))
        {
            inputDevices[priority] = newDeviceProperty;

            if (inputDevices.ContainsKey(priority))
            {
                Debug.Log("InputDevices replaced " + inputDevices[priority].inputDevice.name + " with " + newDeviceProperty.inputDevice.name + " with priority " + priority + " because of same priority");
            }
        }
    }

    public void AddInputDevice(int priority, InputDeviceProperties inputDeviceProperties)
    {
        if (!inputDevices.ContainsValue(inputDeviceProperties))
        {
            inputDevices[priority] = inputDeviceProperties;

            if (inputDevices.ContainsKey(priority))
            {
                Debug.Log("InputDevices replaced " + inputDevices[priority].inputDevice.name + " with " + inputDeviceProperties.inputDevice.name + " with priority " + priority + " because of same priority");
            }
        }
    }

    public void RemoveInputDevice(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            inputDevices.Remove(priority);
        }
    }
}
