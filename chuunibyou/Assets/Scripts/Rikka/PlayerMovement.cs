using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour 
{
    GameObject mainCamera;
    Rigidbody rigidBody;

    public float moveSpeed = 10;
    public float jumpPower = 10;
    public float gravityMultiplier = 1;
    public float dashDistance = 10;
    public float dashSpeed = 10;

    private Vector3 dashTarget;
    private bool isGrounded = true;
    private bool isDashing = false;

    private Vector3 floorNormal = Vector3.up;


    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.AssertFormat(mainCamera != null, "no main camera");

        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var hMovement = CrossPlatformInputManager.GetAxis("L_Horizontal");
        var vMovement = CrossPlatformInputManager.GetAxis("L_Vertical");

        // find movement direction relative to the camera
        // flatten the cam's forward and right vector to the xz axis
        var camForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        var camRight = Vector3.Scale(mainCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        var moveDirection = vMovement*camForward + hMovement*camRight;
        Move(moveDirection);

        CheckGroundStatus();

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

    void Move(Vector3 direction)
    {
        if (isDashing)
            return;
        
        // project the movement direction to the floor's normal
        var moveDirection = Vector3.ProjectOnPlane(direction, floorNormal);

        // dont mess with y velocity
        var newVelocity = moveDirection * moveSpeed;
        newVelocity.y = rigidBody.velocity.y;
        rigidBody.velocity = newVelocity;

        // update rotation to look at the move direction
        transform.LookAt(transform.position + moveDirection);
    }

    void Jump()
    {
        if (isDashing)
            return;
        
        rigidBody.velocity += new Vector3(0, jumpPower, 0);
        isGrounded = false;
    }

    void Dash()
    {
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
        const float groundCheckDistance = 0.15f;

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