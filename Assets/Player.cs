using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed;
    public float gravity = 9.8f;
    public float jumpSpeed;
    public float rotSpeed = 10.0f;

    public Camera camera;

    Rigidbody rb;
    bool landed = false;
    float vertical = 0.0f;
    // Start is called before the first frame update
    void Start()
    {        
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 gravityDirection = -transform.position.normalized;
        //transform.up = -gravityDirection;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation, 0.1f);
    
        Physics.gravity = gravityDirection * gravity;
        if (landed)
        {
            if (Input.GetKey(KeyCode.W))
            {
                rb.velocity = transform.forward * speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                rb.velocity = -transform.forward * speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                rb.velocity = -transform.right * speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                rb.velocity = transform.right * speed;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                rb.velocity = rb.velocity + transform.up * jumpSpeed;
            }
        }

        float xRot = Input.GetAxis("Mouse X");

        transform.RotateAround(transform.position, transform.position + transform.up, xRot * rotSpeed * Time.deltaTime);

        vertical -= Input.GetAxis("Mouse Y") * rotSpeed * Time.deltaTime;
        vertical = Mathf.Clamp(vertical, -80, 80);
        
        Camera.main.transform.localRotation = Quaternion.Euler(vertical, 0, 0);
    }

    private void OnCollisionStay(Collision collision)
    {
        landed = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        landed = false;
    }
}
