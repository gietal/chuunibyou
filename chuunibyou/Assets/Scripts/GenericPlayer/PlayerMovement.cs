using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace chuunibyou
{
        
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
        public float dashDistance
        {
            get
            {
                return _dashDistance;
            }
            set
            {
                _dashDistance = value;
                RecalculateDashTimeout();
            }
        }
        public float dashSpeed
        {
            get
            {
                return _dashSpeed;
            }
            set
            {
                _dashSpeed = value;
                RecalculateDashTimeout();
            }
        }
        public float dashTimeout { get; private set; }

        private Vector3 dashTarget;
        public bool isRunning
        {
            get
            {
                return _isRunning;
            }
            private set
            {
                // update animation
                _isRunning = value;
                animator.SetBool("isRunning", value);
            }
        }

        public bool isGrounded
        {
            get
            {
                return _isGrounded;
            }
            private set
            {
                // update animation
                _isGrounded = value; 
                animator.SetBool("isGrounded", value);
            }
        }
        public bool isDashing
        {
            get
            {
                return _isDashing;
            }
            private set
            {
                // update animation
                _isDashing = value;
                animator.SetBool("isDashing", value);
            }
        }

        private bool _isRunning;
        private bool _isGrounded;
        private bool _isDashing;

        [SerializeField]
        private float _dashDistance;
        [SerializeField]
        private float _dashSpeed;

        private Vector3 floorNormal = Vector3.up;


        void Awake()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            Debug.AssertFormat(mainCamera != null, "no main camera");

            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            RecalculateDashTimeout();
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


            if (isGrounded)
            {
                // land movement
                if (rigidBody.velocity.sqrMagnitude < 0.01f)
                    isRunning = false;
            }
            else 
            {
                // airborne movement

                // dash doesnt get affected by gravity
                // only apply gravity while not dashing 
                if (!isDashing)
                {
                    // this negates the standard gravity force that will be applied to all objects 
                    // by the physics engine every loop
                    // and inject our own gravity magnitude to the body
                    rigidBody.AddForce((Physics.gravity * gravityMultiplier) - Physics.gravity);
                }
            }

            // check dashing
            UpdateDashing();


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

        // generic move that takes a normalized axis [-1, 1]
        public bool Move(float horizontalAxis, float verticalAxis)
        {
            // find movement direction relative to the camera
            // flatten the cam's forward and right vector to the xz axis
            var camForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            var camRight = Vector3.Scale(mainCamera.transform.right, new Vector3(1, 0, 1)).normalized;
            var moveDirection = verticalAxis*camForward + horizontalAxis*camRight;
            return MoveWithDirection(moveDirection);
        }

        protected bool MoveWithDirection(Vector3 direction)
        {
            if (isDashing)
                return false;
            
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
                return false;
            }

            // otherwise we are moving and is grounded
            isRunning = true;
            return true;
        }

        public bool Jump()
        {
            if (isDashing)
                return false;
            
            rigidBody.velocity += new Vector3(0, jumpPower, 0);
            isGrounded = false;
            animator.SetTrigger("jump");

            return true;
        }

        public bool Dash()
        {
            if (isDashing)
                return false;
            
            isDashing = true;

            // get new target dash
            dashTarget = transform.position + transform.forward * dashDistance;

            // start timer to stop dashing if we got stuck
            StartCoroutine(DashTimeoutCoroutine());

            return true;
        }

        void UpdateDashing()
        {
            if (!isDashing)
                return;
            
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

        public void StopDashing()
        {
            StopCoroutine(DashTimeoutCoroutine());
            isDashing = false;
        }

        IEnumerator DashTimeoutCoroutine()
        {
            yield return new WaitForSeconds(dashTimeout);
            StopDashing();
        }

        void RecalculateDashTimeout()
        {
            // v = d/t
            // t = d / v
            dashTimeout = dashDistance / dashSpeed;
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
}