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
    public int id;
    public Vector3 position;
    public List<NavMeshNode> adjacents;
    public NavMeshNode previous;
    public Triangle triangle;
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

    public Triangle begin, end;
    public NavMeshNode pathEnd;

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

        int nVertices = mesh.vertexCount;

        int nTris = mesh.triangles.Length / 3;
        triangles = new Triangle[nTris];

        navMesh = new NavMeshNode[nVertices];

        for (int i = 0; i < nVertices; ++i)
        {
            navMesh[i] = new NavMeshNode();
            navMesh[i].adjacents = new List<NavMeshNode>();
            navMesh[i].id = i;
            navMesh[i].position = transform.TransformPoint(mesh.vertices[i]);
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
            navMesh[triangles[i].i1].triangle = triangles[i];

            navMesh[triangles[i].i2].adjacents.Add(navMesh[triangles[i].i1]);
            navMesh[triangles[i].i2].adjacents.Add(navMesh[triangles[i].i3]);
            navMesh[triangles[i].i2].triangle = triangles[i];

            navMesh[triangles[i].i3].adjacents.Add(navMesh[triangles[i].i2]);
            navMesh[triangles[i].i3].adjacents.Add(navMesh[triangles[i].i1]);
            navMesh[triangles[i].i3].triangle = triangles[i];
        }

        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            for (int j = i; j < mesh.vertexCount; ++j)
            {
                if (mesh.vertices[i] == mesh.vertices[j])
                {
                    navMesh[i].adjacents.Add(navMesh[j]);
                    navMesh[j].adjacents.Add(navMesh[i]);
                }
            }
        }

        octree.InsertElements(triangles);

    }

    public NavMeshNode[] GetPath(Triangle begin, Triangle end)
    {
        pathEnd = null;
        bool[] nodesOpened = new bool[navMesh.Length];
        Queue<NavMeshNode> Q = new Queue<NavMeshNode>();
        Q.Enqueue(navMesh[begin.i1]);

        while (Q.Count > 0 && pathEnd == null)
        {
            NavMeshNode node = Q.Dequeue();
            Debug.Log("Node id " + node.id + " end i1 " + end.i1 + " end i2 " + end.i2 + " end i3 " + end.i3);
            if (node.id == end.i1 || node.id == end.i2 || node.id == end.i3) // We're on the destination triangle
            {
                Debug.Log("Path found");
                pathEnd = node;
            }
            else
            {
                foreach (NavMeshNode nextNode in node.adjacents)
                {
                    if (!nodesOpened[nextNode.id])
                    {
                        Q.Enqueue(nextNode);
                        nodesOpened[nextNode.id] = true;
                        nextNode.previous = node;
                    }
                }
            }
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (showOctree)
            this.octree.DrawGizmos();
        
        List<Triangle> tris = octree.getIntesections(p2);
      
        Triangle closest = null;

        Vector3 k1 = new Vector3(32.3f, 45.6f, -89.23f);
        Vector3 k2 = new Vector3(32.3f, 45.6f, -89.23f);

        foreach (Triangle tri in tris)
        {
            if (p2.x >= tri.minPoint.x && p2.y >= tri.minPoint.y && p2.z >= tri.minPoint.z &&
                p2.x <= tri.maxPoint.x && p2.y <= tri.maxPoint.y && p2.z <= tri.maxPoint.z)
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

        if (setStart)
        {
            beaconStart.transform.position = p2;
            begin = closest;
        }
        else
        {
            beaconEnd.transform.position = p2;
            end = closest;
        }

        if (begin != null)
        {
            Gizmos.DrawLine(begin.v1, begin.v2);
            Gizmos.DrawLine(begin.v2, begin.v3);
            Gizmos.DrawLine(begin.v1, begin.v3);
        }

        if (end != null)
        {
            Gizmos.DrawLine(end.v1, end.v2);
            Gizmos.DrawLine(end.v2, end.v3);
            Gizmos.DrawLine(end.v1, end.v3);
        }

        if (pathEnd != null)
        {            
            Debug.Log("Draw path");
            NavMeshNode node = pathEnd;
            Stack<NavMeshNode> S = new Stack<NavMeshNode>();
            NavMeshNode arrivalNode = new NavMeshNode();
            arrivalNode.position = beaconEnd.transform.position;
            S.Push(arrivalNode);
            while (node.id != begin.i1 && node.id != begin.i2 && node.id != begin.i3)
            {
                S.Push(node);
                node = node.previous;
                Gizmos.DrawLine(node.triangle.v1, node.triangle.v2);
                Gizmos.DrawLine(node.triangle.v1, node.triangle.v3);
                Gizmos.DrawLine(node.triangle.v3, node.triangle.v2);
            }

            beaconStart.GetComponent<NavigationAgent>().path = S.ToArray();
            node = pathEnd;
            while (node.id != begin.i1 && node.id != begin.i2 && node.id != begin.i3)
            {
                node = node.previous;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(node.position, node.previous.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(node.triangle.centroid, node.previous.triangle.centroid);
            }
            Gizmos.color = Color.white;
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