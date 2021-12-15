using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartFlightStick : Grabbable
{
    public GameObject LeftController;
    public GameObject RightController;
    public GameObject StickHead;
    bool flightStickEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoFunc(GameObject argument = null)
    {
        flightStickEnabled = !flightStickEnabled;
        StickHead.GetComponent<FlightStick>().enabled = flightStickEnabled;
        //StickHead.GetComponent<FlightStick>().controller = argument;
        LeftController.GetComponent<GraspGrabber>().enabled = flightStickEnabled;
        RightController.GetComponent<GraspGrabber>().enabled = flightStickEnabled;
    }
}
