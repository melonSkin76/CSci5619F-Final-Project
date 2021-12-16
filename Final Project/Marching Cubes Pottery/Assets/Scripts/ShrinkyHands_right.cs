using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkyHands_right : MonoBehaviour
{
    public GameObject BrushTip;
    public OVRSkeleton leftHandSkeleton;
    Vector3 originalScale;

    private bool isActivate;
    private Transform indexTip;
    private Transform thumbTip;
    private Vector3 center;
    private float radius;

    // Start is called before the first frame update
    void Start()
    {
        originalScale = BrushTip.transform.localScale;

        BrushTip.transform.localScale = Vector3.zero;
        BrushTip.GetComponent<MeshRenderer>().enabled = true;
        BrushTip.GetComponent<SphereCollider>().enabled = true;

        isActivate = false;
        BrushTip.SetActive(isActivate);
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivate == false && OVRInput.Get(OVRInput.Button.Four))
        {
            isActivate = true;
            Debug.Log("ShrinkyHands activatied");
        }
        else if (isActivate && OVRInput.Get(OVRInput.Button.Three))
        {
            isActivate = false;
            Debug.Log("ShrinkyHands deactivatied");
        }

        if (OVRPlugin.GetHandTrackingEnabled() && isActivate)
        {
            BrushTip.SetActive(true);

            bonesHandler();

            foreach (var bone in leftHandSkeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    indexTip = bone.Transform;
                }
                else if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                {
                    thumbTip = bone.Transform;
                }
            }

            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!");
            //Debug.Log(indexTip.position);
            //Debug.Log(thumbTip.position);
            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!");

            center = (indexTip.position + thumbTip.position) / 2;
            radius = Vector3.Distance(indexTip.position, thumbTip.position) / 2;

            BrushTip.transform.position = center;
            BrushTip.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
        }
        else
        {
            BrushTip.SetActive(false);
        }
    }

    void ChangeBrushSize()
    {
        if (this.enabled)
        {
            BrushTip.transform.localScale = originalScale;
        }
    }

    void ResetBrushSize()
    {
        if (this.enabled)
        {
            BrushTip.transform.localScale = originalScale;
        }
    }

    private void OnDisable()
    {
        BrushTip.GetComponent<MeshRenderer>().enabled = false;
        BrushTip.GetComponent<SphereCollider>().enabled = false;
    }

    private void OnEnable()
    {
        BrushTip.GetComponent<MeshRenderer>().enabled = true;
        BrushTip.GetComponent<SphereCollider>().enabled = true;
    }

    IEnumerator bonesHandler()
    {
        while (leftHandSkeleton.Bones.Count == 0)
        {
            yield return null;
        }
    }
}
