using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour 
{
    GameObject mainCamera;
    Rigidbody rigidBody;

    public float moveSpeed = 5;

    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.AssertFormat(mainCamera != null, "no main camera");

        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var hMovement = CrossPlatformInputManager.GetAxis("Horizontal");
        var vMovement = CrossPlatformInputManager.GetAxis("Vertical");

        // find movement direction relative to the camera
        // flatten the cam's forward and right vector to the xz axis
        var camForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        var camRight = Vector3.Scale(mainCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        var moveDirection = vMovement*camForward + hMovement*camRight;
        rigidBody.velocity = moveDirection * moveSpeed;

        // update rotation to look at the move direction
        transform.LookAt(transform.position + moveDirection);
    }
}