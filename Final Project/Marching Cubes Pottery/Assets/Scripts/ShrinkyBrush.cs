using UnityEngine;
using UnityEngine.InputSystem;

public class ShrinkyBrush : MonoBehaviour
{
    public InputActionProperty graspAction;
    public GameObject BrushTip;
    Vector3 originalScale;
    // Start is called before the first frame update
    void Start()
    {
        originalScale = BrushTip.transform.localScale;
        
        BrushTip.transform.localScale = Vector3.zero;
        BrushTip.GetComponent<MeshRenderer>().enabled = true;
        BrushTip.GetComponent<SphereCollider>().enabled = true;


        graspAction.action.performed += ChangeBrushSize;
        graspAction.action.canceled += ResetBrushSize;
    }

    private void OnDestroy()
    {
        graspAction.action.performed -= ChangeBrushSize;
        graspAction.action.canceled -= ResetBrushSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeBrushSize(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            float amt = context.ReadValue<float>();
            BrushTip.transform.localScale = amt * originalScale;
        }
    }

    void ResetBrushSize(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            BrushTip.transform.localScale = originalScale;
        }
    }

    private void OnDisable()
    {
        BrushTip.GetComponent<MeshRenderer>().enabled   = false;
        BrushTip.GetComponent<SphereCollider>().enabled = false;
    }

    private void OnEnable()
    {
        BrushTip.GetComponent<MeshRenderer>().enabled   = true;
        BrushTip.GetComponent<SphereCollider>().enabled = true;
    }
}
