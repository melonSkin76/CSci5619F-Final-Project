using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusPointGrabber_right : Grabber
{
    public LineRenderer laserPointer;
    public Material grabbablePointerMaterial;
    public float triggerThreshold = 0.8f;

    protected Material lineRendererMaterial;
    protected Transform grabPoint;
    protected Grabbable grabbedObject;
    protected Transform initialParent;

    private bool performed = false;

    // Start is called before the first frame update
    void Start()
    {
        laserPointer.enabled = false;
        lineRendererMaterial = laserPointer.material;

        grabPoint = new GameObject().transform;
        grabPoint.name = "Grab Point";
        grabPoint.parent = this.transform;
        grabbedObject = null;
        initialParent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (laserPointer.enabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                laserPointer.SetPosition(1, new Vector3(0, 0, hit.distance));

                if (hit.collider.GetComponent<Grabbable>())
                {
                    laserPointer.material = grabbablePointerMaterial;
                }
                else
                {
                    laserPointer.material = lineRendererMaterial;
                }
            }
            else
            {
                laserPointer.SetPosition(1, new Vector3(0, 0, 100));
                laserPointer.material = lineRendererMaterial;
            }
        }

        if (!performed && OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > triggerThreshold)
        {
            Grab();
            performed = true;
            Debug.Log("trigger pressed!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        else if (performed && OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) < triggerThreshold)
        {
            Release();
            performed = false;
            Debug.Log("trigger released!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        if (OVRInput.Get(OVRInput.Touch.SecondaryIndexTrigger))
        {
            TouchDown();
        }
        else
        {
            TouchUp();
        }
    }

    public void Grab()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (hit.collider.GetComponent<Grabbable>())
            {
                hit.collider.GetComponent<Grabbable>().DoFunc();
            }
        }
    }

    public void Release()
    {
        if (grabbedObject)
        {
            if (grabbedObject.GetComponent<Rigidbody>())
            {
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
            }

            grabbedObject.transform.parent = initialParent;
            grabbedObject = null;
        }
    }

    void TouchDown()
    {
        laserPointer.enabled = true;
    }

    void TouchUp()
    {
        laserPointer.enabled = false;
    }
}
