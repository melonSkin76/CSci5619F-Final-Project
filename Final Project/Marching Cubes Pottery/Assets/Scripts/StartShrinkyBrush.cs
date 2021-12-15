using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartShrinkyBrush : Grabbable
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
        LeftController.GetComponent<ShrinkyBrush>().enabled = true;
        RightController.GetComponent<ShrinkyBrush>().enabled = true;
    }
}
