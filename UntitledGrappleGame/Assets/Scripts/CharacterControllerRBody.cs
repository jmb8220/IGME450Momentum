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

    public PlayerState currentState;
    public PlayerState prevState;

    //make this dynamic once it works
    public float playerHeight = 1.0f;

    //acceleration multipliers
    [SerializeField] float walkSpeed = 9f;
    [SerializeField] float sprintSpeed = 11f;
    [SerializeField] float crouchSpeed = 5f;
    [SerializeField] float airSpeed = 5f;
    [SerializeField] float grappleSpeed = 5f;
    [SerializeField] float slideBoost = 15f;

    float globalMovementMult = 10f;
    //float airMovementMult = 0.4f;

    [SerializeField] float jumpImpulse = 60f;

    //this is friction
    float airDragUp = 0.6f;
    float airDragDown = 0.05f;
    float groundDrag = 6f;
    float slidingDrag = 2f;

    float xMovementInput;
    float zMovementInput;

    Vector3 movementInputDirection;

    Vector3 slopeMovementDirection;

    public Rigidbody physicsBody;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    RaycastHit slopeHit;

    

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
            Debug.Log("Player is Grounded!");

            //check for walk and sprint
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.LeftShift) && currentState != PlayerState.Sliding && !Input.GetKey(KeyCode.LeftControl))
            {
                currentState = PlayerState.Sprinting;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 100f, fovTime);
                ManageDrag(groundDrag);
            }
            //check for arbitrary number as a minimum forward velocity to start sliding
            else if (Input.GetKey(KeyCode.LeftControl) && physicsBody.velocity.magnitude >= 5f)
            {
                //need to also move the camera down but I want it to be smooth so it's not here quite yet
                currentState = PlayerState.Sliding;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 102f, fovTime);
                ManageDrag(slidingDrag);
            }
            else if (currentState == PlayerState.Sliding)
            {
                if (physicsBody.velocity.x <= 0.1f)
                {
                    currentState = PlayerState.Crouching;
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                currentState = PlayerState.Crouching;
            }
            else
            {
                currentState = PlayerState.Walking;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 95f, fovTime);
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
        }

        //find different movement vector if on slope
        slopeMovementDirection = Vector3.ProjectOnPlane(movementInputDirection, slopeHit.normal);
        slopeMovementDirection.Normalize();
    }

    //FixedUpdate follows physics ticks
    private void FixedUpdate()
    {

        MovePlayer();

        prevState = currentState;
        //prevXZMovement = xzMovementInput;

    }

    void GetInput()
    {
        //get normalized inputs every update
        xMovementInput = Input.GetAxis("Horizontal");
        zMovementInput = Input.GetAxis("Vertical");
        movementInputDirection = orientation.right * xMovementInput + orientation.forward * zMovementInput;

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

                physicsBody.AddForce(movementInputDirection * walkSpeed * globalMovementMult, ForceMode.Acceleration);

                break;

            case PlayerState.Sprinting:

                physicsBody.AddForce(movementInputDirection * sprintSpeed * globalMovementMult, ForceMode.Acceleration);

                break;

            case PlayerState.Midair:

                physicsBody.AddForce(movementInputDirection * airSpeed, ForceMode.Acceleration);

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
        }

        if (isGrounded && OnSlope())
        {
            physicsBody.AddForce(slopeMovementDirection * grappleSpeed, ForceMode.Acceleration);
        }

    }
}
