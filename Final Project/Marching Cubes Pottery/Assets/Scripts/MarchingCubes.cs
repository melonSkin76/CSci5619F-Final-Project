using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridCell
{
    public Vector3[] bounds;
    public GridCell()
    {
        bounds = new Vector3[8];
    }
};

public class MarchingCubes : MonoBehaviour
{
    private int num_x_steps;
    private int num_y_steps;
    private int num_z_steps;
    
    private float iso_value;
    private int num_indices;

    private Vector3 min_pt;
    private Vector3 max_pt;

    private Vector3 angles;

    private float[] ism;

    public MeshFilter output;
    public InputActionProperty saveAction;
    
    private bool polygonize;

    public float speed { get; set; }
    public bool rotate { get; set; }
    public Vector3 axisOfRotation { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        num_x_steps = 20;
        num_y_steps = 20;
        num_z_steps = 20;

        min_pt = new Vector3(-num_x_steps / 2, -num_y_steps / 2, -num_z_steps / 2);
        max_pt = new Vector3(num_x_steps / 2, num_y_steps / 2, num_z_steps / 2);
        angles = new Vector3(0, 0, 0);

        Vector3 scale = transform.localScale;
        
        iso_value = .5f;
        //saveAction.action.performed += SaveAsset;
        num_indices = 0;

        // 10 by 10 by 10 grid
        ism = new float[num_x_steps * num_y_steps * num_z_steps];

        Initialize();
        speed = 2000.0f;
        rotate = false;
        polygonize = false;
        Polygonize();

        axisOfRotation = new Vector3(1, 1, 1);
    }

    private void OnDestroy()
    {
        //saveAction.action.performed -= SaveAsset;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            angles.y += Time.deltaTime * speed;

            if (angles.y > 360.0f)
            {
                angles.y -= 360;
            }
            //this.transform.localRotation = Quaternion.Euler(new Vector3(0, angles.y, 0));
            this.transform.localRotation = Quaternion.AngleAxis(angles.y, axisOfRotation);
        }
        if(polygonize)
        {
            Polygonize();
            polygonize = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other is SphereCollider)
        {
            IntersectSphere(other as SphereCollider);
        }
        else if (other is CapsuleCollider)
        {
            IntersectCylinder(other as CapsuleCollider);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other is SphereCollider)
        {
            IntersectSphere(other as SphereCollider);
        }
        else if (other is CapsuleCollider)
        {
            IntersectCylinder(other as CapsuleCollider);
        }
    }

    public void Initialize()
    {
        for (int i = 0; i < num_x_steps * num_y_steps * num_z_steps; ++i)
        {
            ism[i] = 1;
        }

        for (int z = 0; z < num_z_steps; ++z)
        {
            for (int y = 0; y < num_y_steps; ++y)
            {
                for (int x = 0; x < num_x_steps; ++x)
                {
                    if (z == 0 || z == num_z_steps - 1 || y == 0 || y == num_y_steps - 1 || x == 0 || x == num_x_steps - 1)
                    {
                        ism[x + y * num_x_steps + z * num_x_steps * num_y_steps] = 0;
                    }
                }
            }
        }
        polygonize = true;
    }

    private void IntersectSphere(SphereCollider sphere)
    {
        Vector3 cen = sphere.transform.position;
        // take the collider from world space into the pot's space
        Vector3 cell_dim = ToCubeCellSpace(cen);
        float rad_x = sphere.radius * sphere.transform.localScale.x / this.transform.localScale.x;
        float rad_y = sphere.radius * sphere.transform.localScale.y / this.transform.localScale.y;
        float rad_z = sphere.radius * sphere.transform.localScale.z / this.transform.localScale.z;
        
        float radius = Mathf.Min(rad_x, rad_y, rad_z);
        float radiusSqr = radius * radius;

        int x_start = Mathf.Clamp(Mathf.CeilToInt(cell_dim.x - rad_x), 0, num_x_steps - 1);
        int y_start = Mathf.Clamp(Mathf.CeilToInt(cell_dim.y - rad_y), 0, num_y_steps - 1);
        int z_start = Mathf.Clamp(Mathf.CeilToInt(cell_dim.z - rad_z), 0, num_z_steps - 1);

        int x_end = Mathf.Clamp(Mathf.FloorToInt(cell_dim.x + rad_x), 0, num_x_steps - 1);
        int y_end = Mathf.Clamp(Mathf.FloorToInt(cell_dim.y + rad_y), 0, num_y_steps - 1);
        int z_end = Mathf.Clamp(Mathf.FloorToInt(cell_dim.z + rad_z), 0, num_z_steps - 1);
        
        // Iterate through all cube cells which intersect the cube's bounding box
        for (int z = z_start; z <= z_end; z += 1)
        {
            for (int y = y_start; y <= y_end; y += 1)
            {
                for (int x = x_start; x <= x_end; x += 1)
                {
                    //if ((cen - CubeCellToWorldSpace(new Vector3Int(x, y, z))).sqrMagnitude <= radiusSqr)
                    {
                        
                        polygonize = polygonize || ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] != 0;
                        float smaller = Mathf.Max(ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] - .01f, 0);
                        ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] = smaller;
                    }
                }
            }
        }
    }

    private void IntersectCylinder(CapsuleCollider cylinder)
    {
        // First, make a cylinder around the capsule
        Vector3 fromBotToTop = new Vector3();
        float radius = 0;
        float height = 0;
        switch(cylinder.direction)
        {
            case (0):
            {
                // X axis
                fromBotToTop = new Vector3(1, 0, 0);
                radius = cylinder.radius * cylinder.transform.localScale.x;
                height = cylinder.height * cylinder.transform.localScale.x;
                break;
            }
            case (1):
            {
                // Y axis
                fromBotToTop = new Vector3(0, 1, 0);
                radius = cylinder.radius * cylinder.transform.localScale.y;
                height = cylinder.height * cylinder.transform.localScale.y;
                break;
            }
            case (2):
            {
                // Z axis
                fromBotToTop = new Vector3(0, 0, 1);
                radius = cylinder.radius * cylinder.transform.localScale.z;
                height = cylinder.height * cylinder.transform.localScale.z;
                break;
            }
        }
        float bnd = Mathf.Max(radius, height);
        float radiusSqr = radius * radius;

        Vector3 bottomPt = cylinder.center - fromBotToTop * cylinder.height * .5f;
        Vector3 topPt = cylinder.center + fromBotToTop * cylinder.height * .5f;

        // Now, transform the cylinder into world space
        bottomPt = cylinder.transform.TransformPoint(bottomPt);
        topPt = cylinder.transform.TransformPoint(topPt);
        Vector3 cylinderAxis = (cylinder.transform.TransformVector(fromBotToTop)).normalized;

        // Make a cube encapsulating the entire cylinder; these are the cells we will check are inside
        // or outside the cylinder
        Vector3 minBnds = new Vector3();
        minBnds.x = Mathf.Min(bottomPt.x - bnd, bottomPt.x + bnd, topPt.x - bnd, topPt.x + bnd);
        minBnds.y = Mathf.Min(bottomPt.y - bnd, bottomPt.y + bnd, topPt.y - bnd, topPt.y + bnd);
        minBnds.z = Mathf.Min(bottomPt.z - bnd, bottomPt.z + bnd, topPt.z - bnd, topPt.z + bnd);
                                           
        Vector3 maxBnds = new Vector3();   
        maxBnds.x = Mathf.Max(bottomPt.x - bnd, bottomPt.x + bnd, topPt.x - bnd, topPt.x + bnd);
        maxBnds.y = Mathf.Max(bottomPt.y - bnd, bottomPt.y + bnd, topPt.y - bnd, topPt.y + bnd);
        maxBnds.z = Mathf.Max(bottomPt.z - bnd, bottomPt.z + bnd, topPt.z - bnd, topPt.z + bnd);

        // Now that the cylinder is in world space, we can transform it into the pot space
        Vector3 startDim = ToCubeCellSpace(minBnds);
        Vector3 endDim = ToCubeCellSpace(maxBnds);
        // bug fix; since orientation matters, we can't just assume start < end after transforming
        // to cube cell space (exercise: ask yourself why?), so we need to find the min and max again
        Vector3 minVals = new Vector3(Mathf.Min(startDim.x, endDim.x), Mathf.Min(startDim.y, endDim.y), Mathf.Min(startDim.z, endDim.z));
        Vector3 maxVals = new Vector3(Mathf.Max(startDim.x, endDim.x), Mathf.Max(startDim.y, endDim.y), Mathf.Max(startDim.z, endDim.z));

        startDim = minVals;
        endDim = maxVals;

        int start_x = Mathf.Clamp(Mathf.FloorToInt(startDim.x), 0, num_x_steps - 1);
        int start_y = Mathf.Clamp(Mathf.FloorToInt(startDim.y), 0, num_y_steps - 1);
        int start_z = Mathf.Clamp(Mathf.FloorToInt(startDim.z), 0, num_z_steps - 1);

        int end_x = Mathf.Clamp(Mathf.CeilToInt(endDim.x), 0, num_x_steps - 1);
        int end_y = Mathf.Clamp(Mathf.CeilToInt(endDim.y), 0, num_y_steps - 1);
        int end_z = Mathf.Clamp(Mathf.CeilToInt(endDim.z), 0, num_z_steps - 1);

        Vector3Int cellPos = new Vector3Int();
        // now loop through and check if each point is in the cylinder
        for (int z = start_z; z <= end_z; ++z)
        {
            for(int y = start_y; y <= end_y; ++y)
            {
                for(int x = start_x; x <= end_x; ++x)
                {
                    // Get the cell pos in world space
                    cellPos.x = x;
                    cellPos.y = y;
                    cellPos.z = z;
                    Vector3 worldPos = CubeCellToWorldSpace(cellPos);
                    Vector3 toWPos = (worldPos - bottomPt);
                    // Calculate the vector rejection to find the shorted distance from this point to the cylinder axis
                    Vector3 rej = (toWPos - Vector3.Dot(toWPos, cylinderAxis) * cylinderAxis);
                    float shortestDistToCenterSqr = rej.sqrMagnitude;
                    if(shortestDistToCenterSqr <= radiusSqr && Vector3.Dot(toWPos, worldPos - topPt) <= 0)
                    {
                        polygonize = polygonize || ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] != 0;
                        float smaller = Mathf.Max(ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] - .01f, 0);
                        ism[x + num_x_steps * y + z * num_x_steps * num_y_steps] = smaller;
                    }
                }
            }
        }
    }

    public Vector3Int FindClosestFilledCell(RaycastHit enterHit, RaycastHit exitHit)
    {
        // Use the DDA algorithm to march through the marching cubes cells to find the first
        // nonzero cell along the path
        // Transform the start and end points into the cube cell space
        Vector3 start = ToCubeCellSpace(enterHit.point);
        Vector3 end = ToCubeCellSpace(exitHit.point);

        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float dz = end.z - start.z;

        float[] ds = new float[3];
        ds[0] = Mathf.Abs(dx);
        ds[1] = Mathf.Abs(dy);
        ds[2] = Mathf.Abs(dz);
        float step = Mathf.Max(ds);

        dx /= step;
        dy /= step;
        dz /= step;

        float x = start.x;
        float y = start.y;
        float z = start.z;
        int i = 1;

        while(i <= step)
        {
            // Check the current cell
            int nearestX = Mathf.RoundToInt(x);
            int nearestY = Mathf.RoundToInt(y);
            int nearestZ = Mathf.RoundToInt(z);
            // Is this point in the cube? Is this point nonzero?
            if(Mathf.Clamp(nearestX, 0, num_x_steps - 1) == nearestX &&
               Mathf.Clamp(nearestY, 0, num_y_steps - 1) == nearestY &&
               Mathf.Clamp(nearestZ, 0, num_z_steps - 1) == nearestZ &&
               ism[nearestX + nearestY * num_x_steps + nearestZ * num_x_steps * num_y_steps] > iso_value)
            {
                return new Vector3Int(nearestX, nearestY, nearestZ);
            }

            x += dx;
            y += dy;
            z += dz;
            ++i;
        }
        return new Vector3Int(-1, -1, -1);
    }

    public void ChangeCell(Vector3Int pos, float val)
    {
        bool polygonize = ism[pos.x + num_x_steps * pos.y + num_x_steps * num_y_steps * pos.z] != val;
        if(polygonize)
        {
            ism[pos.x + num_x_steps * pos.y + num_x_steps * num_y_steps * pos.z] = val;
            Polygonize();
        }
    }

    public float GetCell(Vector3Int pos)
    {
        return ism[pos.x + num_x_steps * pos.y + num_x_steps * num_y_steps * pos.z];
    }

    private Vector3 ToCubeCellSpace(Vector3 pos)
    {
        Vector3 cen = pos;
        // take the collider from world space into the pot's space
        cen -= this.transform.position;
        cen = new Vector3(cen.x / this.transform.localScale.x,
                          cen.y / this.transform.localScale.y,
                          cen.z / this.transform.localScale.z);
        // get the rotation, this takes more work than the scaling & translating
        Matrix4x4 rotMatrix = Matrix4x4.Rotate(Quaternion.Inverse(this.transform.rotation));
        cen = rotMatrix.MultiplyPoint(cen);
        // remember to make sure we start from 0 indexing, so subtract the minimum point
        return cen - min_pt;
    }

    // Take a cube cell index (x, y, z) and transform it into
    // a 3D Point in world space
    public Vector3 CubeCellToWorldSpace(Vector3Int cell)
    {
        // First, map the cube cell from the range [0, numcells]
        // to [-numcells/2, numcells/2]
        Vector3 pos = cell + min_pt;
        // get the rotation, this takes more work than the scaling & translating
        Matrix4x4 rotMatrix = Matrix4x4.Rotate(this.transform.rotation);
        pos = rotMatrix.MultiplyPoint(pos);
        pos = new Vector3(pos.x * this.transform.localScale.x,
                          pos.y * this.transform.localScale.y,
                          pos.z * this.transform.localScale.z);
        pos += this.transform.position;
        return pos;
    }

    void Polygonize()
    {
        List<Vector3> pts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        Mesh Pot = new Mesh();
        output.mesh.Clear();
        num_indices = 0;
        Stopwatch timer = new Stopwatch();
        timer.Start();
        for (float z = min_pt.z; z < max_pt.z - 1; z += 1.0f)
        {
            // Marching through the Y direction
            for (float y = min_pt.y; y < max_pt.y - 1; y += 1.0f)
            {
                // Marching through the X direction
                for (float x = min_pt.x; x < max_pt.x - 1; x += 1.0f)
                {
                    // Create the Cube - set the location of each corner so we can polygonize the cell
                    GridCell cube = new GridCell();
                    cube.bounds[0] = new Vector3(x, y, z);
                    cube.bounds[1] = new Vector3(x + 1, y, z);
                    cube.bounds[2] = new Vector3(x + 1, y + 1, z);
                    cube.bounds[3] = new Vector3(x, y + 1, z);

                    cube.bounds[4] = new Vector3(x, y, z + 1);
                    cube.bounds[5] = new Vector3(x + 1, y, z + 1);
                    cube.bounds[6] = new Vector3(x + 1, y + 1, z + 1);
                    cube.bounds[7] = new Vector3(x, y + 1, z + 1);
                    // Now find the triangles for this grid cell
                    PolygonizeCell(cube, pts, normals, triangles);
                }
            }
        }
        timer.Stop();
        //UnityEngine.Debug.Log("Time Elapsed: " + timer.ElapsedMilliseconds);
        Pot.vertices = pts.ToArray();
        Pot.normals = normals.ToArray();
        Pot.triangles = triangles.ToArray();
        output.mesh = Pot;
    }

    void PolygonizeCell(GridCell cube, List<Vector3> pts, List<Vector3> normals, List<int> triangles)
    {
        float[] values = new float[8];
        int idx = 0;
        foreach (Vector3 bound in cube.bounds)
        {
            values[idx++] = value(bound);
        }
        
        // Now check whether each cube corner is inside or outside the scalar field
        int edge_table_lookup = 0;
        for (int i = 0; i < 8; ++i)
        {
            if (values[i] < iso_value)
            {
                // If the value at this region is less than the iso value, we flag it as inside
                edge_table_lookup |= 0x01 << i;
            }
        }

        // There are 256 possible triangulations of the gridcell, but we know there are 2 cases
        // where we don't have any triangles:
        // 1) All the grid points are in the surface
        // 2) All the grid points are outside the surface
        // In these cases, there is no point in continuing, so we can return early
        if (edge_table[edge_table_lookup] == 0)
        {
            // this cube is completely inside or outside the surface
            return;
        }

        // This table defines the edges of the gridcell - so {0,1} connects
        // grid points 0 and 1, {1, 2} connects 1 and 2, etc.
        int[,] edges = 
        {
            { 0, 1}, { 1, 2}, { 2, 3}, { 3, 0},
            { 4, 5}, { 5, 6}, { 6, 7}, { 7, 4},
            { 0, 4}, { 1, 5}, { 2, 6}, { 3, 7}
        };
        Vector3[] vertex_edges = new Vector3[12];
        // Right now, we only know that some gridcells are inside the surface
        // and some are outside, but we don't know WHERE the surface actually
        // is. So next we are going to find exact location of the surface along
        // each edge of the gridcell by using linear interpolation
        // check for intersections along the iso surface and the edge
        for (int i = 0; i < 12; ++i)
        {
            if ((edge_table[edge_table_lookup] & (0x01 << i)) != 0)
            {
                // linearly interpolate to find the vertex point along the edge
                int index_1 = edges[i, 0];
                int index_2 = edges[i, 1];
                float delta = values[index_2] - values[index_1];
                float lerp = (iso_value - values[index_1]) / delta;
                vertex_edges[i].x = cube.bounds[index_1].x + lerp * (cube.bounds[index_2].x - cube.bounds[index_1].x);
                vertex_edges[i].y = cube.bounds[index_1].y + lerp * (cube.bounds[index_2].y - cube.bounds[index_1].y);
                vertex_edges[i].z = cube.bounds[index_1].z + lerp * (cube.bounds[index_2].z - cube.bounds[index_1].z);
            }
        }

        // Finally, we can use the lookup table to construct the triangles
        // All we have to do is look up how to create triangles using the points
        // in vertex_edges
        // construct the triangles for this cell
        // A cell may have at most 5 triangles
        for (int i = 0; i < 5; ++i)
        {
            // Each triangle is defined by 3 indices; these indices specify an offset into vertex_edges
            int index_1 = tri_table[edge_table_lookup, 3 * i];
            int index_2 = tri_table[edge_table_lookup, 3 * i + 1];
            int index_3 = tri_table[edge_table_lookup, 3 * i + 2];
            // If one of the indices is invalid, then we have no more triangles to polygonize,
            // so end early
            if (index_1 == -1 || index_2 == -1 || index_3 == -1)
            {
                break;
            }
            Vector3 vert_1 = new Vector3(vertex_edges[index_1].x, vertex_edges[index_1].y, vertex_edges[index_1].z);
            Vector3 vert_2 = new Vector3(vertex_edges[index_2].x, vertex_edges[index_2].y, vertex_edges[index_2].z);
            Vector3 vert_3 = new Vector3(vertex_edges[index_3].x, vertex_edges[index_3].y, vertex_edges[index_3].z);
            // In order to light our model, we are going to want some normal vectors. Fortunately, the 3 triangles points
            // allow us to create a normal. All we have to do is create 2 vectors (1 from point 1 to point 2 and another from point 1 to point 3)
            // then cross them to find a normal.
            // find plane normal
            Vector3 to_p2 = vert_2 - vert_1;
            Vector3 to_p3 = vert_3 - vert_1;
            // Make sure it has a length of 1, otherwise the lighting will look weird
            Vector3 normal = Vector3.Cross(to_p2, to_p3).normalized;

            pts.Add(vert_1);
            pts.Add(vert_2);
            pts.Add(vert_3);

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            triangles.Add(num_indices++);
            triangles.Add(num_indices++);
            triangles.Add(num_indices++);
        }
    }

    float value(Vector3 pos)
    {
        pos = pos + new Vector3(num_x_steps / 2, num_y_steps / 2, num_z_steps / 2);
        return ism[(int)(pos.x + pos.y * num_x_steps + pos.z * num_x_steps * num_y_steps)];
    }

    public float GetRotation()
    {
        return angles.y;
    }

    public void SetRotation(float newAngle)
    {
        angles.y = newAngle;
        //this.transform.localRotation = Quaternion.Euler(new Vector3(0, angles.y, 0));
        this.transform.localRotation = Quaternion.AngleAxis(angles.y, axisOfRotation);
    }

    void SaveAsset(InputAction.CallbackContext context)
    {
        var mf = this.gameObject.GetComponent<MeshFilter>();
        if (mf)
        {
            //var savePath = "Assets/" + "pot" + ".asset";
            //UnityEngine.Debug.Log("Saved Mesh to:" + savePath);
            //AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }

    // Edge table for marching cubes
    int[] edge_table = 
    {
        0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
        0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
        0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
        0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
        0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
        0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
        0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
        0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
        0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
        0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
        0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
        0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
        0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
        0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
        0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
        0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
        0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
        0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
        0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
        0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
        0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
        0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
        0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
        0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
        0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
        0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
        0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
        0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
        0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
        0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
        0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
        0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
    };

    // Triangle table for marching cubes
    int[,] tri_table =
    { 
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
        {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
        {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
        {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
        {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
        {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
        {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
        {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
        {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
        {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
        {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
        {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
        {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
        {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
        {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
        {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
        {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
        {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
        {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
        {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
        {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
        {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
        {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
        {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
        {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
        {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
        {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
        {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
        {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
        {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
        {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
        {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
        {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
        {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
        {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
        {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
        {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
        {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
        {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
        {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
        {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
        {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
        {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
        {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
        {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
        {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
        {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
        {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
        {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
        {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
        {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
        {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
        {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
        {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
        {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
        {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
        {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
        {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
        {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
        {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
        {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
        {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
        {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
        {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
        {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
        {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
        {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
        {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
        {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
        {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
        {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
        {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
        {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
        {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
        {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
        {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
        {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
        {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
        {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
        {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
        {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
        {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
        {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
        {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
        {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
        {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
        {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
        {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
        {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
        {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
        {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
        {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
        {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
        {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
        {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
        {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
        {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
    };    
}
