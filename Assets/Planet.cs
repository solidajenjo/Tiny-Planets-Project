using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Triangle
{
    public Triangle(int id, int i1, int i2, int i3, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.id = id;
        this.i1 = i1;
        this.i2 = i2;
        this.i3 = i3;
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        adjacents = new List<Triangle>();

        minPoint.x = Mathf.Min(v1.x, Mathf.Min(v2.x, v3.x));
        minPoint.y = Mathf.Min(v1.y, Mathf.Min(v2.y, v3.y));
        minPoint.z = Mathf.Min(v1.z, Mathf.Min(v2.z, v3.z));

        maxPoint.x = Mathf.Max(v1.x, Mathf.Max(v2.x, v3.x));
        maxPoint.y = Mathf.Max(v1.y, Mathf.Max(v2.y, v3.y));
        maxPoint.z = Mathf.Max(v1.z, Mathf.Max(v2.z, v3.z));
    }

    public Vector3 position, v1, v2, v3, minPoint, maxPoint;
    public int id;
    public int i1, i2, i3;
    public List<Triangle> adjacents;
};

[ExecuteInEditMode]
public class Planet : MonoBehaviour
{
    public float size;
    public int maxDepth, bucketSize;
    public bool showOctree = false;
    public Vector3 p1, p2;
    public GameObject beaconStart;
    public GameObject beaconEnd;
    public bool setStart = true;



    [SerializeField]
    public OctreeComponent octree;

    Triangle[] triangles;

    void Start()
    {
        Debug.Log("START");
        Reset();        
    }

    public void Reset()
    {
        Debug.Log("Reset");
        octree = new OctreeComponent(transform.position, size, maxDepth, bucketSize);
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int nTris = mesh.triangles.Length / 3;
        triangles = new Triangle[nTris];

        for (int i = 0; i < nTris; ++i) //create triangles
        {
            triangles[i] = new Triangle(i,
                                        mesh.triangles[i * 3],
                                        mesh.triangles[(i * 3) + 1],
                                        mesh.triangles[(i * 3) + 2],
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[i * 3]]),
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[(i * 3) + 1]]),
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[(i * 3) + 2]]));            
        }

       

        octree.InsertElements(triangles);
        /*for (int i = 0; i < nTris; ++i) //Link triangles step
        {            
            for (int j = i + 1; j < nTris; ++j)
            {
                if (triangles[i].i1 == triangles[j].i1 || triangles[i].i1 == triangles[j].i2 || triangles[i].i1 == triangles[j].i3 ||
                    triangles[i].i2 == triangles[j].i1 || triangles[i].i2 == triangles[j].i2 || triangles[i].i2 == triangles[j].i3 ||
                    triangles[i].i3 == triangles[j].i1 || triangles[i].i3 == triangles[j].i2 || triangles[i].i3 == triangles[j].i3) //Adjacent triangles?
                {
                    triangles[i].adjacents.Add(triangles[j]); //Link triangles
                    triangles[j].adjacents.Add(triangles[i]);  //Link triangles
                }
            }
        }*/
    }

    private void OnDrawGizmos()
    {
        if (showOctree)
            this.octree.DrawGizmos();
        
        List<Triangle> tris = octree.getIntesections(p2);

        if (setStart)
            beaconStart.transform.position = p2;
        else
            beaconEnd.transform.position = p2;

        Triangle closest = null;
        

        foreach (Triangle tri in tris)
        {
            if (p2.x > tri.minPoint.x && p2.y > tri.minPoint.y && p2.z > tri.minPoint.z &&
                p2.x < tri.maxPoint.x && p2.y < tri.maxPoint.y && p2.z < tri.maxPoint.z)
            {
                closest = tri;
            }
        }
        if (closest != null)
        {
            Gizmos.DrawWireSphere(closest.v1, 10);
            Gizmos.DrawWireSphere(closest.v2, 10);
            Gizmos.DrawWireSphere(closest.v3, 10);
        }
    }

    
    // Update is called once per frame
    void Update()
    {

    }

}