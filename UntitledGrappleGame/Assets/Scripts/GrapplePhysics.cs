using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePhysics : MonoBehaviour
{
    private Rigidbody body;
    [SerializeField] private Transform playerCam;

    [SerializeField] public Vector3 grapplePoint;
    public bool isGrappling = false;

    //Grapple test
    RaycastHit ray;
    private bool canGrapple = false;

    [SerializeField] public float grappleStrength = 10f;
    [SerializeField] public LayerMask grappleSurfaces;
    private Vector3 grappleDirection;

    //Breaking grapple options
    [SerializeField] private float grappleTimeLength = 4;
    private float grappleTimer;

    //Effects
    [SerializeField] private LineRenderer rope;
    [SerializeField] private Animator anim;


    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Knowing if the player can grapple
        if(Physics.Raycast(playerCam.position, playerCam.transform.forward, out ray, 50, grappleSurfaces)) {
            canGrapple = true;
        } else {
            canGrapple = false;
        }

        //Shooting grapple
        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(!isGrappling && canGrapple) {
                //Shooting grapple
                EnableGrapple(ray.point);
            }
        }

        //Breaking the grapple
        if(Input.GetKeyUp(KeyCode.Mouse0)) {
            if(isGrappling) {
                //Disabling the grapple
                DisableGrapple();
            }
        }

        //Crosshair animation
        anim.SetBool("CanGrapple", canGrapple);
    }

    private void FixedUpdate()
    {
        //Pulling the player
        if(isGrappling)
        {
            Grapple();

            //Breaking the grapple after a set amount of time
            if(grappleTimer <= Time.time) {
                DisableGrapple();
            }
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
            DisableGrapple();
        }

        //Updating the line renderer
        rope.SetPosition(0, transform.position);
    }

    public void EnableGrapple(Vector3 grapplePosition)
    {
        //Setting the position and enabling the bool
        grapplePoint = grapplePosition;
        isGrappling = true;

        //Enabling the timer
        grappleTimer = Time.time + grappleTimeLength;

        //Line renderer
        rope.gameObject.SetActive(true);
        rope.SetPosition(0, transform.position);
        rope.SetPosition(1, grapplePosition);

        //Animation
        anim.SetTrigger("Grapple");
    }

    public void DisableGrapple()
    {
        isGrappling = false;

        rope.gameObject.SetActive(false);

        //Animation
        anim.SetTrigger("GrappleBreak");
    }
}
