using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour 
{
    GameObject mainCamera;
    Rigidbody rigidBody;
    Animator animator;
    AnimatorStateInfo currentAnimState;
    AnimatorStateInfo prevAnimState;

    public float moveSpeed = 10;
    public float jumpPower = 10;
    public float gravityMultiplier = 1;
    public float dashDistance = 10;
    public float dashSpeed = 10;

    private Vector3 dashTarget;
    public bool isRunning { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isDashing { get; private set; }

    private Vector3 floorNormal = Vector3.up;


    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.AssertFormat(mainCamera != null, "no main camera");

        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        isRunning = false;
        isGrounded = true;
        isDashing = false;
    }

    void FixedUpdate()
    {
        // update anim state
        prevAnimState = currentAnimState;
        currentAnimState = animator.GetCurrentAnimatorStateInfo(0);

        if (AnimationChangedState())
        {
            ResetAnimationTrigger();
        }

        // are we grounded?
        CheckGroundStatus();
        // update animator
        animator.SetBool("isGrounded", isGrounded);

        var hMovement = CrossPlatformInputManager.GetAxis("L_Horizontal");
        var vMovement = CrossPlatformInputManager.GetAxis("L_Vertical");

        // find movement direction relative to the camera
        // flatten the cam's forward and right vector to the xz axis
        var camForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        var camRight = Vector3.Scale(mainCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        var moveDirection = vMovement*camForward + hMovement*camRight;
        Move(moveDirection);

        // update animation
        animator.SetBool("isRunning", isRunning);

        // check if jumping
        if (isGrounded )
        {
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                Jump();
            }

        }
        else
        {
            // airborne, apply gravity to the body
            if (!isDashing)
            {
                rigidBody.AddForce((Physics.gravity * gravityMultiplier) - Physics.gravity);
            }
        }

        // check dashing
        if (!isDashing)
        {
            if (CrossPlatformInputManager.GetButtonDown("Dash"))
            {
                Dash();
            }
        }
        else
        {
            UpdateDashing();
        }
            
    }

    void Update()
    {
        
    }

    void ResetAnimationTrigger()
    {
        // reset animation trigger that only supposed to be active until the animation state changed
        animator.ResetTrigger("jump");
    }

    bool AnimationChangedState()
    {
        return prevAnimState.fullPathHash != currentAnimState.fullPathHash;
    }

    public void Move(Vector3 direction)
    {
        if (isDashing)
            return;
        
        // project the movement direction to the floor's normal
        var moveDirection = Vector3.ProjectOnPlane(direction, floorNormal);

        // update velocity but dont mess with y velocity
        var newVelocity = moveDirection * moveSpeed;
        newVelocity.y = rigidBody.velocity.y;
        rigidBody.velocity = newVelocity;

        // update rotation to look at the move direction xz
        transform.LookAt(transform.position + direction);

        //Debug.Log("moveDirection: " + moveDirection + "; newVelocity: " + newVelocity);
        //Debug.Log("moveDirection SqMagnitude: " + moveDirection.sqrMagnitude);

        // if no movement or not grounded
        if (moveDirection.sqrMagnitude <= 0.01 || !isGrounded)
        {
            // then we're not running
            isRunning = false;
            return;
        }

        // otherwise we are moving and is grounded
        isRunning = true;
    }

    public void Jump()
    {
        if (isDashing)
            return;
        
        rigidBody.velocity += new Vector3(0, jumpPower, 0);
        isGrounded = false;
        animator.SetTrigger("jump");
    }

    public void Dash()
    {
        if (isDashing)
            return;
        
        isDashing = true;

        // get new target dash
        dashTarget = transform.position + transform.forward * dashDistance;
    }

    void UpdateDashing()
    {
        // did we pass the target yet?
        if (Vector3.Dot(dashTarget - transform.position, transform.forward) < 0)
        {
            // yes
            isDashing = false;
            return;
        }

        // dash with set speed forward
        var dashVelocity = transform.forward * dashSpeed;
        // dont mess with y
        dashVelocity.y = rigidBody.velocity.y;

        // set velocity
        rigidBody.velocity = dashVelocity;

        #if UNITY_EDITOR
        // helper to visualise the dash target
        Debug.DrawLine(transform.position + Vector3.up, dashTarget + Vector3.up);
        #endif
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        const float groundCheckDistance = 0.2f;

        #if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
        #endif

        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            isGrounded = true;
            floorNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            floorNormal = Vector3.up;
        }
    }
}