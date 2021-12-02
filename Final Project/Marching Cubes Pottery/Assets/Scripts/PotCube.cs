using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotCube : MonoBehaviour
{
    public float ismValue;
    public Vector3 idx;
    // Start is called before the first frame update
    void Start()
    {
        ismValue = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit:" + idx);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
