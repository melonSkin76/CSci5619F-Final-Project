using UnityEngine;
using UnityEngine.InputSystem;


public class Chisel : MonoBehaviour
{
    public LineRenderer line;
    public GameObject nonDominantHand;
    public GameObject targetPrimitive;
    public InputActionProperty chiselAction;
    private MarchingCubes potInScene;
    private Vector3Int targetCell;

    // Variables for PRISM
    public GameObject leftHand;
    public GameObject rightHand;
    
    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    private Vector3 lastPrimaryPos;
    private Vector3 lastSecondaryPos;

    // Start is called before the first frame update
    void Start()
    {
        targetCell = new Vector3Int(-1, -1, -1);
        chiselAction.action.canceled += ChiselPot;
        targetPrimitive.GetComponent<MeshRenderer>().enabled = false;
        
        lastLeftPos = leftHand.transform.position;
        lastRightPos = rightHand.transform.position;

        lastPrimaryPos = this.transform.position;
        lastSecondaryPos = nonDominantHand.transform.position;
    }

    private void OnDestroy()
    {
        chiselAction.action.canceled -= ChiselPot;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 curLeftPos = leftHand.transform.position;
        Vector3 curRightPos = rightHand.transform.position;

        Vector3 leftTravelDir = curLeftPos - lastLeftPos;
        float leftTravelDist = leftTravelDir.magnitude;
        float leftScaleFactor = 1.0f;
        if(leftTravelDist < .02f)
        {
            leftScaleFactor = 0;
        }
        else if(leftTravelDist < .03f)
        {
            leftScaleFactor = .25f;
        }
        else if(leftTravelDist < .05f)
        {
            leftScaleFactor = 1.0f;
        }
        else
        {
            leftScaleFactor = 1.25f;
        }
        leftScaleFactor = 1.0f;
        Vector3 PRISMPrimaryPos = lastPrimaryPos + leftScaleFactor * leftTravelDir;
        lastPrimaryPos = PRISMPrimaryPos;
        this.transform.position = PRISMPrimaryPos;

        lastLeftPos = curLeftPos;
        lastRightPos = curRightPos;

        // get direction from this hand to the non-dominant hand
        Vector3 toOther = nonDominantHand.transform.position - this.transform.position;
        line.SetPosition(1, leftHand.transform.InverseTransformPoint(nonDominantHand.transform.position));
        // we need to find both the entry and exit point into the clay for our find nearest cell that contains clay method
        // In Unity, we must perform 2 raycasts in order to get the entry and exit points
        // Start by raycasting forward
        RaycastHit start;
        bool foundHit = Physics.Raycast(nonDominantHand.transform.position, toOther, out start);
        if(!foundHit || start.collider.GetComponent<MarchingCubes>() == null)
        {
            // nothing to do; we didn't hit the pot first
            potInScene = null;
            targetCell = new Vector3Int(-1, -1, -1);
            targetPrimitive.GetComponent<MeshRenderer>().enabled = false;
            return;
        }
        potInScene = start.collider.GetComponent<MarchingCubes>();
        // now that we found the entry point, we need the exit point. To find the exit, we are going to raycast
        // from the opposite direction. To be safe, we are going to to RaycastAll to check for every object
        // that collides with our ray, then scan to find the pot. This way, if there is an object immediately behind
        // the pot (say, the floor), we can still hit the pot to find the exit point.
        // Remember to shoot backwards from the goal to find our hit
        // 100 meters should be safe for our purposes; the scene isn't that large, and it isn't too large that we have
        // to worry about overflowing
        RaycastHit[] hits = Physics.RaycastAll(nonDominantHand.transform.position + toOther * 100, -toOther);
        RaycastHit end = new RaycastHit();
        
        if(hits.Length == 0)
        {
            // then there is no point in continuing, because we didn't hit the pot somehow
            potInScene = null;
            targetCell = new Vector3Int(-1, -1, -1);
            targetPrimitive.GetComponent<MeshRenderer>().enabled = false;
            return;
        }
        // since we entered the cube region, we are guarenteed an exit point, so find that as well
        foreach(RaycastHit objectIntersection in hits)
        {
            if(objectIntersection.collider.GetComponent<MarchingCubes>())
            {
                end = objectIntersection;
                break;
            }
        }
        // Find the grid cell point with clay in it that we hit
        targetCell = potInScene.FindClosestFilledCell(start, end);
        Vector3 endPos = potInScene.CubeCellToWorldSpace(targetCell);
        line.SetPosition(1, leftHand.transform.InverseTransformPoint(endPos));
        targetPrimitive.GetComponent<MeshRenderer>().enabled = true;
        targetPrimitive.transform.position = endPos;
    }
    
    void ChiselPot(InputAction.CallbackContext context)
    {
        Debug.Log("Released!");
        if(targetCell.x != -1 && targetCell.y != -1 && targetCell.z != -1)
        {
            float curVal = potInScene.GetCell(targetCell);
            float newVal = Mathf.Max(0, curVal - 0.1f);
            potInScene.ChangeCell(targetCell, newVal);
            potInScene = null;
            targetCell = new Vector3Int(-1, -1, -1);
            targetPrimitive.GetComponent<MeshRenderer>().enabled = false;
        }
    }

}
