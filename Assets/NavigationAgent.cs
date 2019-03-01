using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{

    public float arrivedDistance;
    public bool moving = false;
    public float speed = 1.0f;
    public float turningSpeed = 0.1f;
    public NavMeshNode[] path;

    Rigidbody rb;
    int target = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moving && path != null)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.forward, -(transform.position - path[target].position)) * transform.rotation, turningSpeed);
            rb.velocity = transform.forward * speed;
            if (Vector3.Distance(transform.position, path[target].position) < arrivedDistance)
            {
                ++target;
            }
            if (target > path.Length)
            {
                moving = false;
                path = null;
            }
        }
        else
        {
            target = 0;
        }
    }
}
