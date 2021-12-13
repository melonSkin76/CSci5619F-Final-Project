using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnWheel : MonoBehaviour
{

    private Vector3 angles;

    // Start is called before the first frame update
    void Start()
    {
        angles = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        angles.y += Time.deltaTime * 400.0f;
        if (angles.y > 360.0f)
        {
            angles.y -= 360;
        }
        this.transform.rotation = Quaternion.Euler(new Vector3(0, angles.y, 0));
    }
}
