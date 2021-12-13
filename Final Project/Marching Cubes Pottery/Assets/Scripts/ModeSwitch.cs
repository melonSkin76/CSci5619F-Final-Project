using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSwitch : MonoBehaviour
{
    public InputActionProperty switchModeAction;

    public GameObject leftController;
    public GameObject rightController;

    private int idx;
    private MonoBehaviour[] leftModes;
    private MonoBehaviour[] rightModes;

    // Start is called before the first frame update
    void Start()
    {
        idx = 0;

        leftModes = leftController.GetComponents<MonoBehaviour>();
        rightModes = rightController.GetComponents<MonoBehaviour>();

        leftModes[idx].enabled = true;
        for(int i = 1; i < leftModes.Length; ++i)
        {
            leftModes[i].enabled = false;
        }

        switchModeAction.action.performed += SwitchMode;
    }

    private void OnDestroy()
    {
        switchModeAction.action.performed -= SwitchMode;
    }

    void SwitchMode(InputAction.CallbackContext context)
    {
        Debug.Log(idx);
        leftModes[idx].enabled  = false;
        //rightModes[idx].enabled = false;
        
        ++idx;
        idx %= leftModes.Length;
        leftModes[idx].enabled  = true;
        //rightModes[idx].enabled = true;
    }
}
