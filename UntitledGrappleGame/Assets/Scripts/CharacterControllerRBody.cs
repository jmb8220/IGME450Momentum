using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    Grappling,
    Idle
}

public class CharacterControllerRBody : MonoBehaviour
{
    [Header("Camera Properties")]

    public Transform orientation;
    public Camera playerCam;
    public GameObject camContainer;

    private AudioManager audioManager;

    //air time text object
    [SerializeField] Canvas airTimer;
    public bool startable;

    public PlayerState currentState;
    public PlayerState prevState;

    public KeyCode prevKeyPressed;

    //make this dynamic once it works
    public float playerHeight = 1.0f;

    [Header("Movement Values")]

    //acceleration multipliers
    [SerializeField] float walkSpeed = 12f;
    [SerializeField] float sprintSpeed = 15f;
    [SerializeField] float crouchSpeed = 8f;
    [SerializeField] float airSpeed = 25f;
    [SerializeField] float grappleSpeed = 25f;
    [SerializeField] float slideBoost = 7f;

    float globalMovementMult = 10f;
    //float airMovementMult = 0.4f;

    [SerializeField] float jumpImpulse = 255f;

    [Header("Friction Values")]

    //this is friction
    float airDragUp = 1.2f;
    float airDragDown = 0.1f;
    float groundDrag = 7f;
    float slidingDrag = 1f;
    float grappleDrag = 1f;

    float additionalGravity = 7.6f;

    float xMovementInput;
    float zMovementInput;

    Vector3 movementInputDirection;

    Vector3 slopeMovementDirection;

    [Header("Helpers")]

    public Rigidbody physicsBody;

    public Transform groundCheck;
    public float groundDistance = 0.9f;
    public LayerMask groundMask;

    RaycastHit slopeHit;

    [Header("Audio")]

    public AudioSource windLoop;
    public AudioSource grappleLoop;
    public AudioSource jumpFall;
    public AudioSource jump;
    public AudioSource slideBoostSound;

    //audio intervals
    float walkInterval = 0.4f;
    float sprintInterval = 0.38f;
    float crouchInterval = 0.65f;

    public AudioSource[] steps;
    public AudioSource[] clothSteps;

    //randomization helpers for footsteps
    int rand1;
    int rand2;
    int prevRand1;
    int prevRand2;

    float randVolume;

    private bool isPlayingSprintSounds;
    private bool isPlayingCrouchSounds;

    bool isGrounded;
    bool hasReachedMaxWindVolume;
    
    [Header("Clamber")]

    //Clamber Information
    [SerializeField] private float clamberDistance = 3.0f;
    [SerializeField] public LayerMask clamberSurfaces;
    private Vector3 ClamberOrig = Vector3.zero; //Position where the player started the clamber
    private Vector3 LedgePos = Vector3.zero; //Position where the player is clambering to
    private float clamberTimer = 0.0f;

    //Grapple node
    private GrapplePhysics grapplingHook;

    //Dash effect components
    [SerializeField] private Transform speedLinesContainer;
    [SerializeField] private ParticleSystem speedEffect;

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

    void PlayRandomStep()
    {
        rand1 = Random.Range(0, steps.Length);
        rand2 = Random.Range(0, clothSteps.Length);

        randVolume = Random.Range(0.1f, 0.3f);

        //ensure no direct repeats
        while (rand1 == prevRand1) {
            rand1 = Random.Range(0, steps.Length);
        }
        while (rand2 == prevRand2) {
            rand2 = Random.Range(0, steps.Length);
        }

        prevRand1 = rand1;
        prevRand2 = rand2;

        steps[rand1].volume = randVolume;
        steps[rand1].Play();
        clothSteps[rand2].volume = randVolume;
        clothSteps[rand2].Play();
    }

    //handles footstep timing
    public IEnumerator SprintStepSound()
    {
        for(; ; )
        {
            if (currentState == PlayerState.Sprinting)
            {
                if (prevState == PlayerState.Midair)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                isPlayingSprintSounds = true;

                PlayRandomStep();
                yield return new WaitForSeconds(sprintInterval);
            }

            yield return null;
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        physicsBody = GetComponent<Rigidbody>();
        physicsBody.freezeRotation = true;

        grapplingHook = GetComponent<GrapplePhysics>();
        audioManager = GetComponent<AudioManager>();

        StartCoroutine(SprintStepSound());

        windLoop.volume = 0f;
        windLoop.Play();

        grappleLoop.volume = 0.5f;
    }

    private void Update()
    {
        //check for grounding
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //Disabling grounded if the player is clambering
        if(currentState == PlayerState.Clambering) {
            isGrounded = false;
        }

        //get player input direction
        GetInput();

        //Updating the state
        UpdateState();

        //Playing landing sound
        if (isGrounded && prevState == PlayerState.Midair) {
            jumpFall.Play();
        }

        //find different movement vector if on slope
        slopeMovementDirection = Vector3.ProjectOnPlane(movementInputDirection, slopeHit.normal);
        slopeMovementDirection.Normalize();


        //wind audio loop when flying
        if (currentState == PlayerState.Grappling || currentState == PlayerState.Midair) {
            if (windLoop.volume < physicsBody.velocity.magnitude/90) {
                windLoop.volume += 0.03f;
            } else {
                hasReachedMaxWindVolume = true;
            }

            if (hasReachedMaxWindVolume) {
                windLoop.volume = physicsBody.velocity.magnitude/90;
            }
            
        } else {
            StartCoroutine(FadeAudioSource.StartFade(windLoop, 1.1f, 0f));
            hasReachedMaxWindVolume = false;
        }

        //kill grapple sound if not grappling
        if (currentState != PlayerState.Grappling)
        {
            if (grappleLoop.isPlaying)
            {
                grappleLoop.Stop();
            }
            if (grappleLoop.volume <= 0.01f)
            {
                grappleLoop.Stop();
            }
        }

    }

    private void UpdateState()
    {
        //CLAMBER (overrides any other state)
        if(currentState == PlayerState.Clambering) {
            return;
        } else if(Input.GetKey(KeyCode.Space) && !isGrounded) {
            //Checking if the player can clamber
            LedgePos = CanClamber(clamberDistance);
            if (LedgePos != Vector3.zero) {
                currentState = PlayerState.Clambering;
                clamberTimer = 0.0f;
                ClamberOrig = transform.position;

                //Disabling the grapple
                grapplingHook.DisableGrapple();

                return;
            }
        }

        //GRAPPLING STATE
        if (grapplingHook.isGrappling) {
            currentState = PlayerState.Grappling;
            ManageDrag(grappleDrag);
            return;
        }

        //NON-OVERRIDE STATES
        if(isGrounded) {
            //Updating drag
            ManageDrag(groundDrag);

            //IDLE
            if (physicsBody.velocity.x < 0.1f && physicsBody.velocity.z < 0.1f) {
                currentState = PlayerState.Idle;
            }

            //Check if the player isn't inputting anything and slow to zero if so
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.Space)) {
                SlowToZero();
                currentState = PlayerState.Idle;
                return;
            }

            //SLIDING
            if (Input.GetKeyDown(KeyCode.LeftShift) && physicsBody.velocity.magnitude >= 3f && (prevState == PlayerState.Sprinting || prevState == PlayerState.Midair)) {
                //need to also move the camera down but I want it to be smooth so it's not here quite yet
                currentState = PlayerState.Sliding;
                playerCam.fieldOfView = Mathf.MoveTowards(playerCam.fieldOfView, 95f, 150 * Time.deltaTime);
                ManageDrag(slidingDrag);
                return;
            }
            //Ending slide
            if(currentState == PlayerState.Sliding) {
                if (physicsBody.velocity.x <= 0.1f && physicsBody.velocity.y <= 0.1f) {
                    currentState = PlayerState.Sprinting;
                    ManageDrag(groundDrag);
                    return;
                }
                if (!Input.GetKey(KeyCode.LeftShift)) {
                    currentState = PlayerState.Sprinting;
                    return;
                }
            }

            //SPRINTING
            currentState = PlayerState.Sprinting;
            playerCam.fieldOfView = Mathf.MoveTowards(playerCam.fieldOfView, 80f, 50 * Time.deltaTime);
            return;

        //AIRBORN STATES
        } else {
            //MIDAIR
            currentState = PlayerState.Midair;

            //Changing drag based on fall direction
            if (physicsBody.velocity.y > 0f) {
                ManageDrag(airDragUp);
            } else {
                ManageDrag(airDragDown);
            }

            return;
        }
    }



    //FixedUpdate follows physics ticks
    private void FixedUpdate()
    {
        //Executing state
        MovePlayer();

        prevState = currentState;

        //Effects
        SpeedLines();

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
            case PlayerState.Sprinting:
                //this impulse force is for faster directional change
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    physicsBody.AddForce(movementInputDirection * globalMovementMult * 2, ForceMode.Impulse);

                    if (!isPlayingSprintSounds)
                    {
                        isPlayingSprintSounds = true;
                    }
                }
                
                physicsBody.AddForce(movementInputDirection * sprintSpeed * globalMovementMult, ForceMode.Acceleration);
                //Steps
                
                //pause air timer
                airTimer.GetComponent<Timer>().paused = true;
                break;

            case PlayerState.Midair:

                physicsBody.AddForce(movementInputDirection * airSpeed, ForceMode.Acceleration);

                //fall faster up to terminal velocity
                
                if (physicsBody.velocity.y >= -55.5f && physicsBody.velocity.y < 0f)
                {
                    physicsBody.AddForce(-transform.up * additionalGravity, ForceMode.Acceleration);
                }

                physicsBody.AddForce(-transform.up * additionalGravity, ForceMode.Acceleration);


                //resume air timer
                airTimer.GetComponent<Timer>().paused = false;
                break;

            case PlayerState.Sliding:

                if (prevState == PlayerState.Sprinting)
                {
                    slideBoostSound.Play();
                    physicsBody.AddForce((movementInputDirection + orientation.transform.forward) * slideBoost * globalMovementMult, ForceMode.Impulse);
                }

                //pause air timer
                airTimer.GetComponent<Timer>().paused = true;
                break;

            case PlayerState.Crouching:

                physicsBody.AddForce(movementInputDirection * crouchSpeed * globalMovementMult, ForceMode.Acceleration);
                if (!isPlayingCrouchSounds)
                {
                    isPlayingCrouchSounds = true;
                }




                //pause air timer
                airTimer.GetComponent<Timer>().paused = true;
                break;


            case PlayerState.Clambering:
                //Healting movement
                physicsBody.velocity = Vector3.zero;

                //Increasing the clamber timer
                clamberTimer += 3*Time.deltaTime;

                //Moving the player's position to the target
                transform.position = new Vector3(
                    ClamberOrig.x + (LedgePos.x - ClamberOrig.x)*Mathf.Sin(clamberTimer*Mathf.PI/2),
                    ClamberOrig.y + (LedgePos.y - ClamberOrig.y)*Mathf.Sin(clamberTimer*Mathf.PI/2),
                    ClamberOrig.z + (LedgePos.z - ClamberOrig.z)*Mathf.Sin(clamberTimer*Mathf.PI/2));

                //Ending the clamber
                if(clamberTimer >= 1)
                {
                    currentState = PlayerState.Midair;
                    LedgePos = Vector3.zero;
                }

                //pause air timer
                airTimer.GetComponent<Timer>().paused = true;
                break;

            case PlayerState.Grappling:
                //Air control
                physicsBody.AddForce(movementInputDirection * grappleSpeed, ForceMode.Acceleration);

                //Audio
                if (!grappleLoop.isPlaying)
                {
                    grappleLoop.volume = 0.5f;
                    grappleLoop.Play();
                }

                //resume air timer if in air
                if (!isGrounded)
                    airTimer.GetComponent<Timer>().paused = false;
                else
                    airTimer.GetComponent<Timer>().paused = true;


                break;
        }

        //Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            physicsBody.AddForce(transform.up * jumpImpulse, ForceMode.Impulse);
            jump.Play();
            //physicsBody.velocity = new Vector3(physicsBody.velocity.x, jumpImpulse/5, physicsBody.velocity.z);
        }

        //Slope handle
        if (isGrounded && OnSlope())
        {
            physicsBody.AddForce(slopeMovementDirection * grappleSpeed, ForceMode.Acceleration);
        }

        //Disabling spring audio
        if (currentState != PlayerState.Sprinting)
        {
            isPlayingSprintSounds = false;
        }

    }

    //Checking if the player can clamber
    Vector3 CanClamber(float clamberDisance)
    {
        //Player straight orientation
        Vector3 faceDir = new Vector3(orientation.forward.x, 0, orientation.forward.z);
        faceDir = Vector3.Normalize(faceDir);

        Vector3 rayPos = transform.position + (faceDir * clamberDistance/2) + new Vector3(0, clamberDistance/2, 0);

        RaycastHit hitVertical;
        RaycastHit hitHorizontal;

        //Detecting the top of the ledge
        if(!Physics.Raycast(rayPos, -transform.up, out hitVertical, clamberDisance, clamberSurfaces))
        {
            return Vector3.zero;
        }

        //Checking to make sure nothing is blocking the ledge
        Physics.Raycast(transform.position, faceDir, out hitHorizontal, clamberDisance/2, clamberSurfaces);

        if(hitVertical.collider != hitHorizontal.collider)
        {
            return Vector3.zero;
        }

        return hitVertical.point + new Vector3(0, playerHeight*2, 0);
    }


    //slow the player to zero on no input when grounded, makes feel snappier
    void SlowToZero()
    {
        if (Mathf.Abs(physicsBody.velocity.x) > 0.0f || Mathf.Abs(physicsBody.velocity.z) > 0.0f)
        {
            physicsBody.AddForce(-physicsBody.velocity * globalMovementMult * 1.5f, ForceMode.Acceleration);
        }
    }

    //Dash Effect Function
    void SpeedLines()
    {
        //Knowing if the lines show up
        if(physicsBody.velocity.magnitude < 15) {
            //Stopping the effect
            if(speedEffect.isPlaying)
                speedEffect.Stop();

            return;
        }

        //Play the effect
        if(!speedEffect.isPlaying)
            speedEffect.Play();

        //Pointing in the proper direction
        speedLinesContainer.LookAt(transform.position + physicsBody.velocity);
    }
}
