using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrapplePhysics : MonoBehaviour
{
    private Rigidbody body;
    [SerializeField] private Transform playerCam;

    [SerializeField] private float aimAssistRadius = 1;
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

    [Header ("Grapple Display")]

    //Effects
    [SerializeField] private LineRenderer rope;
    [SerializeField] private Animator anim;

    //Grapple Count
    [SerializeField] private Text gCounter;
    public int gCount;
    public bool gUpdate;

    [Header ("Audio")]

    public AudioSource grappleShot;
    public AudioSource grappleRetract;


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

        //Detecting where the player is aiming with aim assist
        if(!Physics.Raycast(playerCam.position, playerCam.transform.forward, out ray, grappleReach, grappleSurfaces)) {
            Physics.SphereCast(playerCam.position, aimAssistRadius, playerCam.transform.forward, out ray, grappleReach, grappleSurfaces);
        }

        if(ray.collider && ray.collider.tag == "CanGrapple") {
            canGrapple = true;
        }

        //Shooting grapple
        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(!isGrappling && canGrapple) {
                //grapple sound
                grappleShot.Play();

                //Shooting grapple
                EnableGrapple(ray.point);

                if(gUpdate) gCount++;
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

        //Increasing vertical strength when moving up
        if(grappleDirection.y > 0) {
            grappleDirection = new Vector3(grappleDirection.x, grappleDirection.y*3.5f, grappleDirection.z);
        }

        //Applying force
        body.AddForce(grappleDirection*grappleStrength);

        //Updating the line renderer
        rope.SetPosition(0, transform.position);
    }

    public void EnableGrapple(Vector3 grapplePosition)
    {
        //Setting the position and enabling the bool
        isGrappling = true;
        grapplePoint = grapplePosition;

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
        //Making sure the player is grappling before breaking
        if(!isGrappling)
            return;

        //Reeling in
        StartCoroutine(Reel());

        //Animation
        anim.SetTrigger("GrappleBreak");

        //Disabling grapple bool
        isGrappling = false;
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
