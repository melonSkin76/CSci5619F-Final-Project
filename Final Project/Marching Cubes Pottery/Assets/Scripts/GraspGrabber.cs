using UnityEngine;
using UnityEngine.InputSystem;

public class GraspGrabber : Grabber
{
    public InputActionProperty grabAction;

    protected FlightStick joystick;
    
    // Start is called before the first frame update
    void Start()
    {
        joystick = null;

        grabAction.action.performed += Grab;
        grabAction.action.canceled += Release;
    }

    private void OnDestroy()
    {
        grabAction.action.performed -= Grab;
        grabAction.action.canceled -= Release;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void Grab(InputAction.CallbackContext context)
    {
        if (joystick != null)
        {
            if (joystick.controller != null && joystick.controller.GetInstanceID() != this.gameObject.GetInstanceID())
            {
                joystick.controller.GetComponent<GraspGrabber>().Release(new InputAction.CallbackContext());
            }
            joystick.controller = this.gameObject;
        }
    }

    public override void Release(InputAction.CallbackContext context)
    {
        if (joystick != null)
        {
            joystick.controller = null;
            joystick = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (joystick == null && other.GetComponent<FlightStick>())
        {
            joystick = other.gameObject.GetComponent<FlightStick>();
        }
    }

    void OnTriggerExit(Collider other)
    {
    }

    private void OnEnable()
    {
        this.GetComponent<SphereCollider>().isTrigger = true; 
    }

    private void OnDisable()
    {
        this.GetComponent<SphereCollider>().isTrigger = false;
    }
}
