using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrapplePhysics : MonoBehaviour
{
    private Rigidbody body;
    [SerializeField] private Transform playerCam;

    [SerializeField] public Vector3 grapplePoint;
    public bool isGrappling = false;
    private float startingY;

    //Grapple test
    RaycastHit ray;
    private bool canGrapple = false;

    [SerializeField] public float grappleReach = 50f;
    [SerializeField] public float grappleStrength = 10f;
    [SerializeField] public LayerMask grappleSurfaces;
    private Vector3 grappleDirection;

    //Breaking grapple options
    [SerializeField] private float grappleTimeLength = 4;
    private float grappleTimer;

    //Effects
    [SerializeField] private LineRenderer rope;
    [SerializeField] private Animator anim;

    public AudioSource grappleShot;
    public AudioSource grappleRetract;

    //Grapple Count
    [SerializeField] private Text gCounter;
    public int gCount;


    private void Start()
    {
        body = GetComponent<Rigidbody>();
        UpdateGrappleCount();
    }

    // Update is called once per frame
    void Update()
    {
        //Knowing if the player can grapple
        canGrapple = false;
        if(Physics.Raycast(playerCam.position, playerCam.transform.forward, out ray, grappleReach, grappleSurfaces)) {
            if(ray.collider.tag == "CanGrapple") {
                canGrapple = true;
            }
        }

        //Shooting grapple
        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(!isGrappling && canGrapple) {
                //grapple sound
                grappleShot.Play();

                //Shooting grapple
                EnableGrapple(ray.point);

                gCount++;
                UpdateGrappleCount();
            }
        }

        //Breaking the grapple
        if(Input.GetKeyUp(KeyCode.Mouse0)) {
            if(isGrappling) {

                //grapple retraction sound
                grappleRetract.Play();
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

        //Increasing vertical strength
        grappleDirection = new Vector3(grappleDirection.x, grappleDirection.y*2, grappleDirection.z);

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
        StartCoroutine(Reel());

        //Marking the initial y position of the player
        startingY = transform.position.y;

        //Animation
        anim.SetTrigger("Grapple");
    }

    public void DisableGrapple()
    {
        //Applying an upward boost to the player
        /*
        body.velocity = new Vector3(body.velocity.x,
            (grapplePoint.y - startingY - (grapplePoint.y - transform.position.y))*1.25f,
            body.velocity.z);*/

        //Disabling grapple bool
        isGrappling = false;

        //Reeling in
        StartCoroutine(Reel());

        //Animation
        anim.SetTrigger("GrappleBreak");
    }

    //Grapple animation
    IEnumerator Reel()
    {
        float i = 0;

        while(i < 1) {
            i += Time.deltaTime*10;

            //Animating grapple point
            if(isGrappling) {
                //Shooting out
                rope.SetPosition(1, Vector3.Lerp(transform.position, grapplePoint, i));
            } else {
                //Reeling in
                rope.SetPosition(0, transform.position);
                rope.SetPosition(1, Vector3.Lerp(grapplePoint, transform.position, i));
            }

            yield return null;
        }

        if(isGrappling) {
            rope.SetPosition(1, grapplePoint);
        } else {
            rope.gameObject.SetActive(false);
        }
    }

    public void UpdateGrappleCount()
    {
        gCounter.text = "Grapples Used: " + gCount;
    }
}
