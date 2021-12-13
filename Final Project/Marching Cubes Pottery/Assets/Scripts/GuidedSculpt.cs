using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GuidedSculpt : MonoBehaviour
{
    public InputActionProperty graspAction;
    public InputActionProperty changeLayerAction;
    public InputActionProperty startAction;
    public MarchingCubes pot;
    public GameObject sculptor;
    public LineRenderer sculptPath;

    private Vector3 originalSculptPos;

    private float insetAmount;
    private int lineIdx;
    // Start is called before the first frame update
    void Start()
    {
        graspAction.action.performed += ChangeInset;
        graspAction.action.canceled += ResetInset;

        changeLayerAction.action.performed += ChangeLevel;

        sculptor.GetComponent<MeshRenderer>().enabled = true;
        sculptor.GetComponent<SphereCollider>().enabled = true;
        
        originalSculptPos = sculptor.transform.position;
        insetAmount = originalSculptPos.x;

        sculptPath.positionCount = 21;
        Vector3[] positions = new Vector3[21];
        for(int i = 0; i < 21; ++i)
        {
            positions[i] = new Vector3();
            positions[i].x = -0.5f;
            positions[i].y = 1.0f - i * 0.0125f;
            positions[i].z = 0.5f;
        }
        sculptPath.SetPositions(positions);

        lineIdx = 0;
    }

    private void OnDestroy()
    {
        graspAction.action.performed -= ChangeInset;
        graspAction.action.canceled -= ResetInset;

        changeLayerAction.action.performed -= ChangeLevel;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 handPos = this.transform.position;
        Vector3 sculptPos = new Vector3();
        sculptPos.x = insetAmount;
        sculptPos.y = handPos.y;
        sculptPos.z = originalSculptPos.z;
        

        sculptor.transform.position = sculptPos;
    }

    void ChangeLevel(InputAction.CallbackContext context)
    {
        Vector2 joystickDir = context.ReadValue<Vector2>();
        Debug.Log("Here");
        if(joystickDir.y > 0)
        {
            lineIdx = Mathf.Max(0, lineIdx - 1);
        }
        else if(joystickDir.y < 0)
        {
            lineIdx = Mathf.Min(sculptPath.positionCount, lineIdx + 1);
        }
    }

    void ChangeInset(InputAction.CallbackContext context)
    {
        float amt = context.ReadValue<float>();
        Vector3 toCenter = pot.transform.position - originalSculptPos;
        insetAmount = originalSculptPos.x + amt * toCenter.x;
        Vector3 curPos = sculptPath.GetPosition(lineIdx);
        curPos.x = insetAmount;
        sculptPath.SetPosition(lineIdx, curPos);
    }

    void ResetInset(InputAction.CallbackContext context)
    {
        sculptor.transform.position = originalSculptPos;
        insetAmount = originalSculptPos.x;
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
