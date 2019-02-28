using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeComponent 
{    
    public int maxDepth, bucketSize;


    public OctreeNode root = new OctreeNode();

    public OctreeComponent(Vector3 position, float size, int maxDepth, int bucketSize)
    {
        root.position = position;
        root.size = size;
        root.minPoint = root.position - new Vector3(root.size * 0.5f, root.size * 0.5f, root.size * 0.5f);
        root.maxPoint = root.position + new Vector3(root.size * 0.5f, root.size * 0.5f, root.size * 0.5f);
        this.maxDepth = maxDepth;
        this.bucketSize = bucketSize;
    }

    public void InsertElements(Triangle[] elements)
    {
        foreach (Triangle tri in elements)
        {
            root.bucket.Add(tri);
        }

        Queue<OctreeNode> Q = new Queue<OctreeNode>();
        Q.Enqueue(root);
        
        while (Q.Count > 0)
        {
            OctreeNode node = Q.Dequeue();
            if (node.bucket.Count > bucketSize && node.depth < maxDepth)
            {
                Flush(node);
                foreach (OctreeNode child in node.children)
                {
                    Q.Enqueue(child);
                }
            }
        }
    }

    void Flush(OctreeNode node)
    {
        SubDivideNode(node);
        foreach (Triangle tri in node.bucket)
        {
            foreach(OctreeNode child in node.children)
            {
                if (IsInside(child, tri.v1) || IsInside(child, tri.v2) || IsInside(child, tri.v3))
                {
                    child.bucket.Add(tri);
                }
            }
        }
        node.bucket.Clear();
    }
    void SubDivideNode(OctreeNode node)
    {
        for (int i = 0; i < node.children.Length; ++i)
        {
            Vector3 newPos = node.position;
            if ((i & 4) == 4)
            {
                newPos.y += node.size * 0.25f;
            }
            else
            {
                newPos.y -= node.size * 0.25f;
            }

            if ((i & 2) == 2)
            {
                newPos.x += node.size * 0.25f;
            }
            else
            {
                newPos.x -= node.size * 0.25f;
            }

            if ((i & 1) == 1)
            {
                newPos.z += node.size * 0.25f;
            }
            else
            {
                newPos.z -= node.size * 0.25f;
            }

            node.isLeaf = false;
            node.children[i] = new OctreeNode();
            node.children[i].depth = node.depth + 1;
            node.children[i].position = newPos;
            node.children[i].size = node.size * 0.5f;
            node.children[i].minPoint = newPos - new Vector3(node.children[i].size, node.children[i].size, node.children[i].size);
            node.children[i].maxPoint = newPos + new Vector3(node.children[i].size, node.children[i].size, node.children[i].size);                        

        }
    }

    bool IsInside(OctreeNode node, Vector3 position)
    {
        return node.minPoint.x <= position.x && position.x <= node.maxPoint.x &&
                 node.minPoint.y <= position.y && position.y <= node.maxPoint.y &&
                  node.minPoint.z <= position.z && position.z <= node.maxPoint.z;
    }

    bool Intersects(Vector3 minPoint, Vector3 maxPoint, Vector3 rayOrigin, Vector3 rayDirection)
    {
        float tmin = float.NegativeInfinity;
        float tmax = float.PositiveInfinity;

        for (int a = 0; a < 3; ++a)
        {
            float invD = 1.0f / rayDirection[a];
            float t0 = (minPoint[a] - rayOrigin[a]) * invD;
            float t1 = (maxPoint[a] - rayOrigin[a]) * invD;
            if (invD < 0.0f)
            {
                float temp = t1;
                t1 = t0;
                t0 = temp;
            }

            tmin = t0 > tmin ? t0 : tmin;
            tmax = t1 < tmax ? t1 : tmax;

            if (tmax <= tmin)
                return false;
        }

        return true;
    }

    public List<Triangle> getIntesections(Vector3 point)
    {
        List<Triangle> intersections = new List<Triangle>();
        Queue<OctreeNode> Q = new Queue<OctreeNode>();
        Q.Enqueue(root);

        while (Q.Count > 0)
        {
            OctreeNode node = Q.Dequeue();
            if (!node.isLeaf && IsInside(node, point))
            {
                foreach (OctreeNode child in node.children)
                {
                    Q.Enqueue(child);
                }
            }
            else if (node.isLeaf)
            {
                if (IsInside(node, point))
                {
                    foreach (Triangle tri in node.bucket)
                        intersections.Add(tri);
                }
            }
        }
        return intersections;
    }

    public void DrawGizmos()
    {
        int c = 0;
        Queue<OctreeNode> Q = new Queue<OctreeNode>();
        Q.Enqueue(root);
        while (Q.Count > 0)
        {
            c++;
            OctreeNode node = Q.Dequeue();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(node.position, new Vector3(node.size, node.size, node.size));

            if (!node.isLeaf)
            {
                foreach (OctreeNode child in node.children)
                {
                    Q.Enqueue(child);
                }
            }
        }
    }
  
}


public class OctreeNode
{
    public ArrayList        bucket = new ArrayList();
    public bool             isLeaf = true;
    public Vector3          position, minPoint, maxPoint;
    public float            size = 0.0f;
    public int              depth = 0;
    public OctreeNode[]     children = new OctreeNode[8];
}

