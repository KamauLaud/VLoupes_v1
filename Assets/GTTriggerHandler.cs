using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarjoExample;

public class GTTriggerHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject gt_me;
    private VarjoGazeTarget gt_script;
    void Start()
    {
        // get the associated parent GazeTarget
        gt_me = transform.parent.gameObject;
        gt_script = gt_me.GetComponent<VarjoGazeTarget>(); //use this to call all the methods 

    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
