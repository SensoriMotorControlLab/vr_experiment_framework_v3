using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    //The lists for the experiment
    Dictionary<string, List<object>> expLists = new Dictionary<string, List<object>>();
    //Prefixes used in the JSON file
    List<string> prefixes = new List<string>() { "per_block_", "per_trial_" };
    //The total number of trials for the experiment
    int totalNumOfTrials = 0;
    //Is the experiment using VR
    bool useVR = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateBlocks(Session session)
    {
        var keys = session.settings.Keys;

        //the name of the experiment
        string experimentName = session.settings.GetString("experiment_name");
        session.settings.SetValue("experiment_name", experimentName);

        useVR = session.settings.GetBool("use_vr");
        session.settings.SetValue("use_vr", useVR);
        //list of the number of trials per block
        List<int> trialsPerBlock = session.settings.GetIntList("trials_in_block");
        session.settings.SetValue("trials_in_block", trialsPerBlock);

        //get the total number of trials by adding all trials in each block
        foreach (int trialCount in trialsPerBlock)
            totalNumOfTrials += trialCount;

        session.settings.SetValue("total_number_of_trials", totalNumOfTrials);

        //the type of task in each block
        List<string> taskPerBlock = session.settings.GetStringList("per_block_task");
        session.settings.SetValue("block_task", taskPerBlock);

        //loop through each block
        for (int i = 0; i < trialsPerBlock.Count; i++)
        {
            //the created block
            Block theBlock = session.CreateBlock(trialsPerBlock[i]);
            //define the experiment mode as the task for the block
            theBlock.settings.SetValue("experiment_mode", taskPerBlock[i]);

            foreach(string key in keys)
            {
                if (key.StartsWith("per_block_"))
                {
                    //key minus the prefix
                    string newKey = key.Substring(10, key.Length-10);
                    theBlock.settings.SetValue(newKey, session.settings.GetObjectList(key)[i]);
                    expLists[newKey] = new List<object>(session.settings.GetObjectList(key));
                }
                else if (key.StartsWith("per_trial_"))
                {
                    //key minus the prefix
                    string newKey = key.Substring(10, key.Length-10);

                    //if we already have that list move on
                    if (expLists.ContainsKey(newKey))
                        break;

                    int trialCount = session.settings.GetObjectList(key).Count;
                    
                    //if the total number of elements for the per trial list is not equal to the total number of trials
                    if(trialCount != totalNumOfTrials)
                    {
                        throw new NoSuchTrialException("Number of trials for " + key + " not equal to total number of trials\n" + 
                                                        key +" number of trials: " + trialCount + "\nTotal number of trials: " + totalNumOfTrials);
                    }

                    //pseudo randomize and then set the value with the new key
                    theBlock.settings.SetValue(newKey, PseudoRandom(key));
                }
            }
        }
    }

    public object PseudoRandom(string key)
    {
        List<object> list = Session.instance.settings.GetObjectList(key);
        //a copy of the list
        List<object> copyList = new List<object>(list);
        //a pseudo random version of the list
        List<object> randomList = new List<object>();
        //set the capacity to be the size of the original list
        randomList.Capacity = list.Count;

        if (totalNumOfTrials % list.Count != 0)
        {
            Debug.LogError("The total number of trials is not divisible by the number of elements in: " + key);
            throw new NullReferenceException();
        }

        if (key.StartsWith("per_trial_") || key.StartsWith("per_block_"))
            key = key.Substring(10, key.Length - 10);

        if (!expLists.ContainsKey(key))
            expLists[key] = new List<object>(list);


        if (list.Count == 1)
        {
            return list[0];

        }
        else if (list.Count == 0)
        {
            Debug.LogError(key + " contains no elements. Not possible to select");
            throw new NullReferenceException();
        }

        for (int i = 0; i < list.Count; i++)
        {
            //get a random value based off the size of the copyList
            int randomVal = UnityEngine.Random.Range(0, copyList.Count);
            //set the value for the copy list
            randomList.Add(copyList[randomVal]);
            //remove the random element from the copy list
            copyList.Remove(copyList[randomVal]);
        }

        return randomList;
    }
}
