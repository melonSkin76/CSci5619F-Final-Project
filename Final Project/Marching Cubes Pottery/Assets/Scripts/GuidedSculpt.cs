using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GuidedSculpt : MonoBehaviour
{
    public InputActionProperty graspAction;
    public InputActionProperty startAction;
    public InputActionProperty endAction;

    public MarchingCubes pot;
    public GameObject sculptor;
    
    private Vector3 originalSculptPos;

    private float insetAmount;
    private bool sculpting;
    private int numDegreesTraveled;
    // Start is called before the first frame update
    void Start()
    {
        graspAction.action.performed += ChangeInset;
        graspAction.action.canceled += ResetInset;

        startAction.action.performed += StartSculpt;
        endAction.action.performed += EndSculpt;

        sculptor.GetComponent<MeshRenderer>().enabled = true;
        sculptor.GetComponent<SphereCollider>().enabled = true;
        
        originalSculptPos = sculptor.transform.position;
        insetAmount = originalSculptPos.x;
        sculpting = false;
        numDegreesTraveled = 0;
    }

    private void OnDestroy()
    {
        graspAction.action.performed -= ChangeInset;
        graspAction.action.canceled -= ResetInset;

        startAction.action.performed -= StartSculpt;
        endAction.action.performed -= EndSculpt;
    }
    // Update is called once per frame
    void Update()
    {
        if (sculpting || numDegreesTraveled > 0)
        {
            float angle = pot.GetRotation();
            angle += 1.0f;
            pot.SetRotation(angle);
            ++numDegreesTraveled;
            if(numDegreesTraveled > 359)
            {
                // make a full revolution
                numDegreesTraveled = 0;
                if (!sculpting)
                {
                    sculptor.GetComponent<Collider>().isTrigger = false;
                }
            }
        }
        else
        {
            Vector3 handPos = this.transform.position;
            Vector3 sculptPos = new Vector3();
            sculptPos.x = insetAmount;
            sculptPos.y = handPos.y;
            sculptPos.z = originalSculptPos.z;
            sculptor.transform.position = sculptPos;
        }
    }

    void ChangeInset(InputAction.CallbackContext context)
    {
        float amt = context.ReadValue<float>();
        Vector3 toCenter = pot.transform.position - originalSculptPos;
        insetAmount = originalSculptPos.x + amt * toCenter.x;
    }

    void ResetInset(InputAction.CallbackContext context)
    {
        sculptor.transform.position = originalSculptPos;
        insetAmount = originalSculptPos.x;
    }
    
    void StartSculpt(InputAction.CallbackContext context)
    {
        pot.rotate = false;
        if (numDegreesTraveled > 0)
        {
            return;
        }
        sculpting = true;
        sculptor.GetComponent<Collider>().isTrigger = true;
    }
    
    void EndSculpt(InputAction.CallbackContext context)
    {
        sculpting = false;
    }

    private void OnDisable()
    {
        sculptor.GetComponent<MeshRenderer>().enabled   = false;
        sculptor.GetComponent<SphereCollider>().enabled = false;
    }

    private void OnEnable()
    {
        sculptor.GetComponent<MeshRenderer>().enabled   = true;
        sculptor.GetComponent<SphereCollider>().enabled = true;
    }
}
