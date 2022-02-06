using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player states
/*public enum PlayerState
{
    Grounded,
    Walking,
    Sprinting,
    Midair,
    Sliding,
    Clambering,
    Grappling
}*/

public class CharacterMovement: MonoBehaviour
{
    public PlayerState currentState;
    public PlayerState prevState;

    //public bool grappling = false;

    //physics parameters
    public float maxWalkSpeed = 180.0f;
    public float maxSprintSpeed = 220.0f;
    public float gravity = -40f;
    public float jumpImpulse = 500f;
    public float frictionCoefficient = 5.0f;

    public float speed = 0;

    //for FOV shifting
    [SerializeField] public Camera playerCam;
    public float fovTime = 0.01f;

    float xMoveInput, zMoveInput;

    float terminalVelocity = -200.55f;
    float airMovementMaxDelta = 5.0f;



    public CharacterController controller;

    //master movement vector, velocity is gravity and momentum
    //xzmovement is player input
    //prevxzmovement stores the last movement vector to stop movement control as needed
    public Vector3 velocity = Vector3.zero;
    public Vector3 prevVelocity = Vector3.zero;
    public Vector3 xzMovement = Vector3.zero;
    public Vector3 prevXZMovement = Vector3.zero;
    public Vector3 frictionVector = Vector3.zero;
    public Vector3 normalizedVelocity = Vector3.zero;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool isGrounded;

    //Grapple node
    private GrapplePhysics grapplingHook;

    // Start is called before the first frame update
    void Start()
    {
        grapplingHook = GetComponent<GrapplePhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        //Setting default terminal velocity
        terminalVelocity = -55.55f;


        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        //Debug.Log("Current Speed: " + maxSprintSpeed);
        //update state
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            Debug.Log("Player is Grounded!");

            //check for walk and sprint
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Input.GetKey(KeyCode.LeftShift) && currentState != PlayerState.Sliding)
            {
                currentState = PlayerState.Sprinting;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 100f, fovTime);
                Debug.Log("Player is Sprinting!");
            }
            //check for arbitrary number as a minimum forward velocity to start sliding
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                //need to also move the camera down but I want it to be smooth so it's not here quite yet
                currentState = PlayerState.Sliding;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 102f, fovTime);
                Debug.Log("Player is Sliding!");
            }
            else
            {
                currentState = PlayerState.Walking;
                //playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 95f, fovTime);
                Debug.Log("Player is Walking!");
            }
            

        }
        //midair check, clamber will come later because I've never done it before
        else if (!isGrounded)
        {
            currentState = PlayerState.Midair;
            Debug.Log("Player is Falling/Midair!");

        }
        
        //grappling check, overrides the current state if the GrapplePhysics object is enabled
        if(grapplingHook.isGrappling)
        {
            currentState = PlayerState.Grappling;
        }

        //get inputs
        xMoveInput = Input.GetAxis("Horizontal");
        zMoveInput = Input.GetAxis("Vertical");
        xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;
        //movement according to state
        //TODO: Update this when we add grappling because the params will need to be different
        switch (currentState)
        {
            case PlayerState.Walking:
                speed = Mathf.Lerp(speed, maxWalkSpeed, 7.0f);
                velocity = Vector3.MoveTowards(velocity, xzMovement * speed * Time.deltaTime, maxWalkSpeed);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y += jumpImpulse;
                    Debug.Log("Jumped!");
                }

                break;

            case PlayerState.Sprinting:
                speed = Mathf.Lerp(speed, maxSprintSpeed, 9.0f);
                velocity = Vector3.MoveTowards(velocity, xzMovement * speed * Time.deltaTime, maxSprintSpeed);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y += jumpImpulse;
                    Debug.Log("Jumped!");
                }

                break;

            case PlayerState.Midair:
                //Store the previous velocity vector for delta clamping
                if (prevState != PlayerState.Midair)
                {
                    prevVelocity = velocity;
                }

                //we allow air control, just to a lesser extent
                xzMovement = transform.right * xMoveInput + transform.forward * zMoveInput;

                //Apply velocity of player movement to vector
                velocity = Vector3.MoveTowards(velocity, (xzMovement * speed * Time.deltaTime), .5f);

                //clamp velocity change to prevent infinite speed
                velocity.x = Mathf.Clamp(velocity.x, prevVelocity.x - airMovementMaxDelta, prevVelocity.x - airMovementMaxDelta);
                velocity.z = Mathf.Clamp(velocity.z, prevVelocity.z - airMovementMaxDelta, prevVelocity.z - airMovementMaxDelta);
                break;

            case PlayerState.Sliding:
                if (prevState == PlayerState.Sprinting)
                {
                    speed += 2.5f;
                }
                speed = Mathf.Lerp(speed, 0.0f, 0.02f);
                velocity = Vector3.MoveTowards(velocity, xzMovement * speed * Time.deltaTime * 100f, 2f);
                
                if (speed == 0)
                {
                    currentState = PlayerState.Walking;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y += jumpImpulse;
                    Debug.Log("Jumped!");
                }

                break;
                

            case PlayerState.Clambering:
                break;

            case PlayerState.Grappling:
                //Reducing terminal velocity
                terminalVelocity = -5.0f;

                //grappling steering

                velocity = Vector3.MoveTowards(velocity, xzMovement * speed, .5f);

                break;

        }

        //This will need to be modified conditional to the player's state of grappling, flying, etc

        //calculate and apply friction

        //gravity is always a thing that happens
        velocity.y -= gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, terminalVelocity, 1000f);

        controller.Move(velocity * Time.deltaTime);

        //store current movement direction and state
        prevState = currentState;
        prevXZMovement = xzMovement;

        //Debug.Log("Current Gravity Vector: " + velocity.y);
    }
}
