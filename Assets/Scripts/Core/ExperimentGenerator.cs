using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
/// <summary>
/// Generates experiment parameters based off of JSON file and task objects based off experiment parameters
/// </summary>
public class ExperimentGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    ///<summary>
    ///Read the experiment JSON file and generate the blocks
    ///</summary>
    public void GenerateBlocks(Session session)
    {
        var keys = session.settings.Keys;

        //set if using VR
        ExperimentController.Instance.UseVR = session.settings.GetBool("use_vr");        
        //list of the number of trials per block
        List<int> trialsPerBlock = session.settings.GetIntList("trials_in_block");

        //set the total number of blocks
        ExperimentController.Instance.TotalNumOfBlocks = trialsPerBlock.Count;

        //loop and create each block
        for (int i = 0; i < trialsPerBlock.Count; i++)
        {
            //the created block
            Block theBlock = session.CreateBlock(trialsPerBlock[i]);
            //set the total number of trials by adding all trials in each block
            ExperimentController.Instance.TotalNumOfTrials += trialsPerBlock[i];
            //set use VR for the block
            theBlock.settings.SetValue("use_vr", ExperimentController.Instance.UseVR);
        }

        //loop through the keys and then set them for the blocks
        //prefixes will be removed to be more streamlined
        //for each key in the JSON
        foreach (string key in keys)
        {
            //for per block parameters
            if (key.StartsWith("per_block_"))
            {
                //key minus the prefix
                string newKey = key.Substring(10, key.Length - 10);
                List<object> perBlockList = new List<object>(session.settings.GetObjectList(key));
                ExperimentController.Instance.ExperimentLists[newKey] = new List<object>(perBlockList);
                //set the value for each block
                for (int i = 0; i < session.blocks.Count; i++)
                {
                    session.blocks[i].settings.SetValue(newKey, perBlockList[i]);
                }
            }
            //for per trial parameters, they will be pseudo randomized and then set
            else if (key.StartsWith("per_trial_"))
            {
                //key minus the prefix
                string newKey = key.Substring(10, key.Length - 10);

                //if we already have that list move on
                if (ExperimentController.Instance.ExperimentLists.ContainsKey(newKey))
                    break;

                //the number of elements in the per trial list
                //should be equal or divisible to the number of trials
                int perTrialCount = session.settings.GetObjectList(key).Count;
                //the per trial list
                List<object> perTrialList = session.settings.GetObjectList(key);
                //the resulting list after being pseudo randomized
                List<object> pseudoList = new List<object>();
                //loop for each block and pseudo randomize
                //the count of per trial should be the same as the number of blocks
                for (int i = 0; i < perTrialCount; i++)
                {
                    if (perTrialList[i] != null)
                        pseudoList.Add(PseudoRandom(perTrialList[i].ToString(), session.blocks[i]));
                    else
                        pseudoList.Add(new List<object>());

                    //set the value in the block
                    session.blocks[i].settings.SetValue(newKey, pseudoList[i]);
                }
            }
            else
            {
                if (!ExperimentController.Instance.ExperimentLists.ContainsKey(key))
                {
                    ExperimentController.Instance.ExperimentLists[key] = new List<object>();
                    ExperimentController.Instance.ExperimentLists[key].Add(session.settings.GetObject(key));
                    //session.settings.SetValue(key, session.settings.GetObject(key));
                }
            }
        }
    }
    ///<summary>
    ///Create task objects based on experiment settings from JSON file. When creating a new Task object, said object needs to be added here or it will not be handled.
    ///</summary>
    public void GenerateTasks()
    {
        Session s = ExperimentController.Instance.Session;

        //for each block create a task
        //for every block there is a corresponding task object
        foreach (Block theBlock in s.blocks)
        {
            //based on task type create a Task object as a script component
            //disable the script and add it to the task list
            switch (theBlock.settings.GetString("task"))
            {
                case ("reach_to_target"):
                    ReachTask reachTask = ExperimentController.Instance.gameObject.AddComponent<ReachTask>();
                    reachTask.enabled = false;
                    ExperimentController.Instance.Tasks.Add(reachTask);
                    break;
                case ("sling_shot"):
                    SlingShotTask slingShotTask = ExperimentController.Instance.gameObject.AddComponent<SlingShotTask>();
                    slingShotTask.enabled = false;
                    ExperimentController.Instance.Tasks.Add(slingShotTask);
                    break;
                case ("instruction"):
                    InstructionTask instructionTask = ExperimentController.Instance.gameObject.AddComponent<InstructionTask>();
                    instructionTask.enabled = false;
                    ExperimentController.Instance.Tasks.Add(instructionTask);
                    break;

                default:
                    Debug.LogError("THE TASK HAS NO BEEN DEFINED IN: ExperimentGenerator.GenerateTasks()");
                    break;
            }
        }

        //add a end screen to the end
        EndSessionTask endTask = ExperimentController.Instance.gameObject.AddComponent<EndSessionTask>();
        endTask.enabled = false;
        ExperimentController.Instance.Tasks.Add(endTask);
    }
    /// <summary>
    /// Pseudo randomize a list
    /// </summary>
    public object PseudoRandom(string key, Block theBlock)
    {
        List<object> list = Session.instance.settings.GetObjectList(key);
        //a copy of the list
        List<object> copyList = new List<object>(list);
        //a pseudo random version of the list
        List<object> randomList = new List<object>();
        //set the capacity to be the size of the original list
        randomList.Capacity = list.Count;

        //if it is not divisible by the number of trials
        if (theBlock.trials.Count % list.Count != 0)
        {
            throw new NullReferenceException("The total number of trials in block "+theBlock.number+" is not divisible by the number of elements in: " + key);
        }

        //if the passed key has these prefixes
        if (key.StartsWith("per_trial_") || key.StartsWith("per_block_"))
            key = key.Substring(10, key.Length - 10);

        //If the experiment list does not contain this object list
        if (!ExperimentController.Instance.ExperimentLists.ContainsKey(key))
            ExperimentController.Instance.ExperimentLists[key] = new List<object>(list);

        if (list.Count == 1)
            return list[0];
        else if (list.Count == 0)
        {
            Debug.LogError(key + " contains no elements. Not possible to select");
            throw new NullReferenceException();
        }

        //loop through and randomly set new values
        for (int i = 0; i < list.Count; i++)
        {
            //get a random value based off the size of the copyList
            int randomVal = UnityEngine.Random.Range(0, copyList.Count);
            //set the value for the copy list
            randomList.Add(copyList[randomVal]);
            //remove the random element from the copy list
            copyList.RemoveAt(randomVal);
        }

        return randomList;
    }
}
