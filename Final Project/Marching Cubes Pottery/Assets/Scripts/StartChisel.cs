using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartChisel : Grabbable
{
    public GameObject LeftController;
    public GameObject RightController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoFunc(GameObject arg = null)
    {
        if (arg == LeftController)
        {
            LeftController.GetComponent<Chisel>().enabled = true;
        }
        else if (arg == RightController)
        {
            RightController.GetComponent<Chisel>().enabled = true;
        }
    }
}
