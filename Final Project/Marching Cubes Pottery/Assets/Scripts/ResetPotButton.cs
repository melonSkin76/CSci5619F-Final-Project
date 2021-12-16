using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPotButton : Grabbable
{
    public MarchingCubes pot;

    private Vector3 upPos;
    private Vector3 downPos;
    private bool buttonPressed;

    // Start is called before the first frame update
    void Start()
    {
        upPos = this.gameObject.transform.position;
        downPos = this.gameObject.transform.position + this.gameObject.transform.TransformDirection(Vector3.down * .00625f);
        buttonPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoFunc(GameObject arg = null)
    {
        pot.Initialize();
    }
}
