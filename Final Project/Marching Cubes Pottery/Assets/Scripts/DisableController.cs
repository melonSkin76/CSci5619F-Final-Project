using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableController : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRPlugin.GetHandTrackingEnabled())
        {
            leftController.SetActive(false);
            rightController.SetActive(false);
        }
    }
}
