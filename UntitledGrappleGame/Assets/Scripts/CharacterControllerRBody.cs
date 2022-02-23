using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player states
public enum PlayerState
{
    Grounded,
    Walking,
    Sprinting,
    Crouching,
    Midair,
    Sliding,
    Clambering,
    Grappling
}

public class CharacterControllerRBody : MonoBehaviour
{
    public Transform orientation;
    public Camera playerCam;

    public GameObject camContainer;

    public PlayerState currentState;
    public PlayerState prevState;

    public KeyCode prevKeyPressed;

    //make this dynamic once it works
    public float playerHeight = 1.0f;

    //acceleration multipliers
    [SerializeField] float walkSpeed = 12f;
    [SerializeField] float sprintSpeed = 15f;
    [SerializeField] float crouchSpeed = 8f;
    [SerializeField] float airSpeed = 25f;
    [SerializeField] float grappleSpeed = 25f;
    [SerializeField] float slideBoost = 2f;

    float globalMovementMult = 10f;
    //float airMovementMult = 0.4f;

    [SerializeField] float jumpImpulse = 200f;

    //this is friction
    float airDragUp = 0.6f;
    float airDragDown = 0.05f;
    float groundDrag = 7f;
    float slidingDrag = 2f;
    float grappleDrag = 1f;

    float additionalGravity = 1.8f;

    float xMovementInput;
    float zMovementInput;

    Vector3 movementInputDirection;

    Vector3 slopeMovementDirection;

    public Rigidbody physicsBody;

    public Transform groundCheck;
    public float groundDistance = 0.9f;
    public LayerMask groundMask;

    RaycastHit slopeHit;

    public AudioSource windLoop;

    bool isGrounded;

    //Grapple node
    private GrapplePhysics grapplingHook;

    //slope detection
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        physicsBody = GetComponent<Rigidbody>();
        physicsBody.freezeRotation = true;

        grapplingHook = GetComponent<GrapplePhysics>();

        windLoop.volume = 0f;
        windLoop.Play();
    }

    private void Update()
    {
        //check for grounding
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //get player input direction
        GetInput();

        //update state
        if (isGrounded)
        {
            //Debug.Log("Player is Grounded!");

            //check if the player is not inputting anything and slow to zero if so
            if ((!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)) && !Input.GetKey(KeyCode.Space))
            {
                SlowToZero();
            }

                //check for walk and sprint
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.LeftShift) && currentState != PlayerState.Sliding && !Input.GetKey(KeyCode.LeftControl))
            {
                currentState = PlayerState.Sprinting;
                playerCam.fieldOfView = Mathf.MoveTowards(playerCam.fieldOfView, 90f, 150*Time.deltaTime);
                ManageDrag(groundDrag);
            }
            //check for arbitrary number as a minimum forward velocity to start sliding
            else if (Input.GetKey(KeyCode.LeftControl) && physicsBody.velocity.magnitude >= 5f && (prevState == PlayerState.Sprinting || prevState ==  PlayerState.Midair))
            {
                //need to also move the camera down but I want it to be smooth so it's not here quite yet
                currentState = PlayerState.Sliding;
                playerCam.fieldOfView = Mathf.MoveTowards(playerCam.fieldOfView, 95f, 150*Time.deltaTime);
                ManageDrag(slidingDrag);
            }
            else if (currentState == PlayerState.Sliding)
            {
                if (physicsBody.velocity.x <= 0.1f)
                {
                    currentState = PlayerState.Crouching;
                }
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    currentState = PlayerState.Walking;
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                currentState = PlayerState.Crouching;
            }
            else
            {
                currentState = PlayerState.Walking;
                playerCam.fieldOfView = Mathf.MoveTowards(playerCam.fieldOfView, 80f, 50*Time.deltaTime);
                ManageDrag(groundDrag);
            }

        }
        //midair check, clamber will come later because I've never done it before
        else if (!isGrounded)
        {
            currentState = PlayerState.Midair;
            Debug.Log("Player is In Air!");
            if (physicsBody.velocity.y > 0f)
            {
                ManageDrag(airDragUp);
            }
            else
            {
                ManageDrag(airDragDown);
            }
            
        }

        //grappling check, overrides the current state if the GrapplePhysics object is enabled
        if (grapplingHook.isGrappling)
        {
            currentState = PlayerState.Grappling;
            ManageDrag(grappleDrag);
        }

        //find different movement vector if on slope
        slopeMovementDirection = Vector3.ProjectOnPlane(movementInputDirection, slopeHit.normal);
        slopeMovementDirection.Normalize();


        //wind audio loop when flying
        if ((currentState == PlayerState.Grappling || currentState == PlayerState.Midair))
        {
            if (windLoop.volume < physicsBody.velocity.magnitude / 70)
            {
                windLoop.volume += 0.03f;
            }
                
        }
        else
        {
            StartCoroutine(FadeAudioSource.StartFade(windLoop, .7f, 0f));
        }

    }

    //FixedUpdate follows physics ticks
    private void FixedUpdate()
    {

        MovePlayer();

        prevState = currentState;

       

    }

    void GetInput()
    {
        //get normalized inputs every update
        xMovementInput = Input.GetAxis("Horizontal");
        zMovementInput = Input.GetAxis("Vertical");
        movementInputDirection = orientation.right * xMovementInput + camContainer.transform.forward * zMovementInput;

        movementInputDirection.Normalize();
    }

    //use this later to calculate resistance during different movement states
    void ManageDrag(float frictionCoefficient)
    {
        physicsBody.drag = frictionCoefficient;
    }

    void MovePlayer()
    {

        //move the player based on current player state
        switch (currentState)
        {
            case PlayerState.Walking:
                //this impulse force is for faster directional change
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    physicsBody.AddForce(movementInputDirection * globalMovementMult * 2, ForceMode.Impulse);
                }
                physicsBody.AddForce(movementInputDirection * walkSpeed * globalMovementMult, ForceMode.Acceleration);

                break;

            case PlayerState.Sprinting:
                //this impulse force is for faster directional change
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    physicsBody.AddForce(movementInputDirection * globalMovementMult * 2, ForceMode.Impulse);
                }
                physicsBody.AddForce(movementInputDirection * sprintSpeed * globalMovementMult, ForceMode.Acceleration);

                break;

            case PlayerState.Midair:

                physicsBody.AddForce(movementInputDirection * airSpeed, ForceMode.Acceleration);

                //fall faster up to terminal velocity
                if (physicsBody.velocity.y >= -55.5f && physicsBody.velocity.y < 0f)
                {
                    physicsBody.AddForce(-transform.up * additionalGravity, ForceMode.Acceleration);
                }
                

                break;

            case PlayerState.Sliding:

                if (prevState == PlayerState.Sprinting)
                {
                    physicsBody.AddForce(orientation.transform.forward * slideBoost * globalMovementMult, ForceMode.Impulse);
                }

                break;

            case PlayerState.Crouching:

                physicsBody.AddForce(movementInputDirection * crouchSpeed * globalMovementMult, ForceMode.Acceleration);

                
                break;


            case PlayerState.Clambering:

                break;

            case PlayerState.Grappling:

                physicsBody.AddForce(movementInputDirection * grappleSpeed, ForceMode.Acceleration);

                break;

        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            physicsBody.AddForce(transform.up * jumpImpulse, ForceMode.Impulse);

            //physicsBody.velocity = new Vector3(physicsBody.velocity.x, jumpImpulse/5, physicsBody.velocity.z);
        }

        if (isGrounded && OnSlope())
        {
            physicsBody.AddForce(slopeMovementDirection * grappleSpeed, ForceMode.Acceleration);
        }

    }

    //slow the player to zero on no input when grounded, makes feel snappier
    void SlowToZero()
    {
        if (Mathf.Abs(physicsBody.velocity.x) > 0.0f || Mathf.Abs(physicsBody.velocity.z) > 0.0f)
        {
            physicsBody.AddForce(-physicsBody.velocity * globalMovementMult * 1.5f, ForceMode.Acceleration);
        }
    }
}
