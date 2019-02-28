﻿using System.Collections;
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

        minPoint.x = Mathf.Min(v1.x, Mathf.Min(v2.x, v3.x));
        minPoint.y = Mathf.Min(v1.y, Mathf.Min(v2.y, v3.y));
        minPoint.z = Mathf.Min(v1.z, Mathf.Min(v2.z, v3.z));

        maxPoint.x = Mathf.Max(v1.x, Mathf.Max(v2.x, v3.x));
        maxPoint.y = Mathf.Max(v1.y, Mathf.Max(v2.y, v3.y));
        maxPoint.z = Mathf.Max(v1.z, Mathf.Max(v2.z, v3.z));

        centroid.x = (v1.x + v2.x + v3.x) / 3;
        centroid.y = (v1.y + v2.y + v3.y) / 3;
        centroid.z = (v1.z + v2.z + v3.z) / 3;
    }

    public Vector3 position, v1, v2, v3, minPoint, maxPoint, centroid;
    public int id;
    public int i1, i2, i3;    
};

public class NavMeshNode
{
    public List<NavMeshNode> adjacents;
}


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
    NavMeshNode[] navMesh;

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
        navMesh = new NavMeshNode[mesh.vertexCount];


        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            navMesh[i] = new NavMeshNode();
        }

        for (int i = 0; i < nTris; ++i) //create triangles
        {
            triangles[i] = new Triangle(i,
                                        mesh.triangles[i * 3],
                                        mesh.triangles[(i * 3) + 1],
                                        mesh.triangles[(i * 3) + 2],
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[i * 3]]),
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[(i * 3) + 1]]),
                                        transform.TransformPoint(mesh.vertices[mesh.triangles[(i * 3) + 2]]));

            navMesh[triangles[i].i1].adjacents.Add(navMesh[triangles[i].i2]);
            navMesh[triangles[i].i1].adjacents.Add(navMesh[triangles[i].i3]);

            navMesh[triangles[i].i2].adjacents.Add(navMesh[triangles[i].i1]);
            navMesh[triangles[i].i2].adjacents.Add(navMesh[triangles[i].i3]);

            navMesh[triangles[i].i3].adjacents.Add(navMesh[triangles[i].i2]);
            navMesh[triangles[i].i3].adjacents.Add(navMesh[triangles[i].i1]);
        }
       
        octree.InsertElements(triangles);

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
                if (closest == null)
                {
                    closest = tri;
                }
                else
                {
                    if (Vector3.Distance(tri.centroid, p2) < Vector3.Distance(closest.centroid, p2))
                    {
                        closest = tri;
                    }
                }
            }            
        }

        if (closest != null)
        {
            Gizmos.DrawWireSphere(closest.v1, 1);
            Gizmos.DrawWireSphere(closest.v2, 1);
            Gizmos.DrawWireSphere(closest.v3, 1);
            Gizmos.DrawWireSphere(closest.centroid, 1);
        }
        else
        {
            Debug.Log("Closest triangle not found");
        }
    }

    
    // Update is called once per frame
    void Update()
    {

    }

}