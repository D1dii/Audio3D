﻿
using UnityEditor.VersionControl;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

/*
    This file has a commented version with details about how each line works. 
    The commented version contains code that is easier and simpler to read. This file is minified.
*/


/// <summary>
/// Main script for third-person movement of the character in the game.
/// Make sure that the object that will receive this script (the player) 
/// has the Player tag and the Character Controller component.
/// </summary>
public class ThirdPersonController : MonoBehaviour
{
  
    public AudioMixerSnapshot unPaused;
    public AudioMixerSnapshot Paused;


    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 18f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    public GameObject menu;

    // Player states
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;
     bool inputMenu;
    Animator animator;
    CharacterController cc;

    [SerializeField] private GameObject interactBubble;
    public bool isMoving = false;

    public Transform floorDetectorTransform;

    private bool menuIsActive;
    public enum FloorType
    {
        Normal,
        Carpet,
    }

    [SerializeField] public FloorType floorType;

    void Start()
    {
        menuIsActive = false;
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {

        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        isMoving = inputHorizontal != 0 || inputVertical != 0;
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        // Unfortunately GetAxis does not work with GetKeyDown, so inputs must be taken individually
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        inputMenu = Input.GetKeyDown(KeyCode.Escape);

        // Check if you pressed the crouch input key and change the player's state
        if ( inputCrouch )
            isCrouching = !isCrouching;

        // Run and Crouch animation
        // If dont have animator component, this block wont run
        if ( cc.isGrounded && animator != null )
        {

            // Crouch
            // Note: The crouch animation does not shrink the character's collider
            animator.SetBool("crouch", isCrouching);
            
            // Run
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed );

            // Sprint
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting );

        }

        // Jump animation
        if( animator != null )
            animator.SetBool("air", cc.isGrounded == false );

        // Handle can jump or not
        if ( inputJump && cc.isGrounded )
        {
            isJumping = true;
            // Disable crounching when jumping
            //isCrouching = false; 
        }

        if (inputMenu && menuIsActive == false)
        {
         Debug.Log("Menu is active");
            ShowMenu();

        }
        else if (inputMenu && menuIsActive == true)
        {
            Debug.Log("Menu is not active");
            UnShowMenu();
        }



        HeadHittingDetect();

    }

    void PlayFootsteps()
    {
        AudioManager.instance.PlayFootSteps();  
    }

    void PlayJump()
    {
        AudioManager.instance.PlayJumpSound();
    }
    void ShowMenu()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        menu.SetActive(true);

        Time.timeScale = 0;
        menuIsActive = true;
        Paused.TransitionTo(0.01f);
    }

    void UnShowMenu()
    {

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        menu.SetActive(false);
        Time.timeScale = 1f;
        menuIsActive = false;

        unPaused.TransitionTo(0.01f);
    }
    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        

        // Sprinting velocity boost or crounching desacelerate
        float velocityAdittion = 0;
        if ( isSprinting )
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion =  - (velocity * 0.50f); // -50% velocity

        // Direction movement
        float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        // Jump handler
        if ( isJumping )
        {

            // Apply inertia and smoothness when climbing the jump
            // It is not necessary when descending, as gravity itself will gradually pulls
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            // Jump timer
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        // Add gravity to Y axis
        directionY = directionY - gravity * Time.deltaTime;

        
        // --- Character rotation --- 

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Relate the front with the Z direction (depth) and right with X (lateral movement)
        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- End rotation ---

        
        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move( moviment );

        if (Physics.CheckSphere(floorDetectorTransform.position, 0.1f, LayerMask.GetMask("Floor")))
        {
            floorType = FloorType.Normal;
        }
        else if (Physics.CheckSphere(floorDetectorTransform.position, 0.1f, LayerMask.GetMask("Carpet")))
        {
            floorType = FloorType.Carpet;
        }
        else
        {
            floorType = FloorType.Normal;
        }

        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(floorDetectorTransform.position, 0.1f);
    }

    //This function makes the character end his jump if he hits his head on something
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        // Uncomment this line to see the Ray drawed in your characters head
        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Toaster"))
        {
            AudioManager.instance.PlayToaster();
        }

        if (other.CompareTag("WC"))
        {
            interactBubble.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WC"))
        { 
            if (Input.GetKey(KeyCode.E) && interactBubble.activeSelf)
            {
                interactBubble.SetActive(false);
                AudioManager.instance.PlayWC();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WC"))
        {
            interactBubble.SetActive(false);
        }
    }

    
}



