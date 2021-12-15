using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableController : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;

    private SkinnedMeshRenderer leftRenderer;
    private SkinnedMeshRenderer rightRenderer;

    // Start is called before the first frame update
    void Start()
    {
        leftRenderer = leftController.GetComponent<SkinnedMeshRenderer>();
        rightRenderer = rightController.GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRPlugin.GetHandTrackingEnabled())
        {
            leftRenderer.enabled = false;
            rightRenderer.enabled = false;
        } else
        {
            leftRenderer.enabled = true;
            rightRenderer.enabled = true;
        }
    }
}
