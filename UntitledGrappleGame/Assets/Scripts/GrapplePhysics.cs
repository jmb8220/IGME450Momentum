using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePhysics : MonoBehaviour
{
    private Rigidbody body;
    [SerializeField] private Transform playerCam;

    [SerializeField] public Vector3 grapplePoint;
    public bool isGrappling = false;

    [SerializeField] public float grappleStrength = 10f;
    [SerializeField] public LayerMask canGrapple;
    private Vector3 grappleDirection;


    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Choosing a grapple point
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Shooting grapple
            RaycastHit ray;
            if(Physics.Raycast(playerCam.position, playerCam.transform.forward, out ray, 50, canGrapple))
            {
                grapplePoint = ray.point;
                isGrappling = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //Pulling the player
        if(isGrappling)
        {
            Grapple();
        }
    }

    void Grapple()
    {
        //Pulling object towards grapple point
        grappleDirection = grapplePoint - transform.position;
        grappleDirection.Normalize();

        //Applying force
        body.AddForce(grappleDirection*grappleStrength);

        //Ending the grapple
        if(Vector3.Distance(transform.position, grapplePoint) < 2)
        {
            isGrappling = false;
        }
    }
}
