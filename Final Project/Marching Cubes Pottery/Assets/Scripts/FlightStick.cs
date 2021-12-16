using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightStick : MonoBehaviour
{
    public MarchingCubes Pot;
    public LineRenderer PreviewAxis;
    
    public GameObject controller { get; set; }

    public GameObject stickHandle;

    public Vector3 stickBase;
    public Vector3 stickHead;
    // Start is called before the first frame update
    void Start()
    {
        controller = null;
        stickHead = stickBase + .15f * (this.transform.position - stickBase).normalized;
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller != null)
        {
            Vector3 stickDir = (controller.transform.position - stickBase).normalized;
            if (Vector3.Dot(stickDir, Vector3.up) < 0)
            {
                return;
            }
            stickHead = stickBase + .15f * stickDir;
            this.transform.position = stickHead;
            Pot.axisOfRotation = (stickHead - stickBase).normalized;
            Pot.UpdateOrientation();
            Vector3 previewTip = PreviewAxis.GetPosition(0) + .35f * Pot.axisOfRotation;
            PreviewAxis.SetPosition(1, previewTip);
            // Rotate the stick model
            stickHandle.transform.rotation = Quaternion.FromToRotation(Vector3.up, stickDir);
        }
    }

    private void OnEnable()
    {
        PreviewAxis.enabled = true;
    }

    private void OnDisable()
    {
        PreviewAxis.enabled = false;
    }
}
