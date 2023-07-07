using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentController : MonoBehaviour
{
    private static ExperimentController instance = null;
    private ExperimentGenerator expGenerator = null;
    private Session session;


    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
            instance = this;

        expGenerator = new ExperimentGenerator();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public ExperimentController Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("ExperimentController is unitialized");
            
            return instance; 
        }
    }

    public ExperimentGenerator ExperimentGenerator
    {
        get { return expGenerator; }
    }

    public void SessionBegin(Session session)
    {
        this.session = session;

        expGenerator.GenerateBlocks(session);
    }
}
