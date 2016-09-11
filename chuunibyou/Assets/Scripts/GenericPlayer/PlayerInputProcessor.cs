using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace chuunibyou 
{
    public class PlayerInputProcessor : MonoBehaviour 
    {
        PlayerMovement movementManager;
        ComboManager comboManager;

        void Awake()
        {
            movementManager = GetComponent<PlayerMovement>();
            comboManager = GetComponent<ComboManager>();

            Debug.AssertFormat(movementManager != null, "{0} doesnt have PlayerMovement", this.name);
            Debug.AssertFormat(comboManager != null, "{0} doesnt have ComboManager", this.name);
        }

        // we need to put the input processing in FixedUpdate, since Animator.applyRootMotion is checked
        // applyRootMotion basically move the game object's body based on the animation's velocity
        // during the Update() loop
        // so we put it here so that the velocity changes we do can also be included
        // another option is to disable applyRootMotion and enable Animator.animatePhysics
        void FixedUpdate()
        {
            // update movement
            var hMovement = CrossPlatformInputManager.GetAxis("L_Horizontal");
            var vMovement = CrossPlatformInputManager.GetAxis("L_Vertical");
            movementManager.Move(hMovement, vMovement);

            // update jumping
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                movementManager.Jump();
            }

            // update dashing
            if (CrossPlatformInputManager.GetButtonDown("Dash"))
            {
                movementManager.Dash();
            }

            // update attacks
            if (CrossPlatformInputManager.GetButtonDown("LightAttack"))
            {
                comboManager.DoAction(ComboActionType.LightAttack);
            }

            if(CrossPlatformInputManager.GetButtonDown("HeavyAttack"))
            {
                comboManager.DoAction(ComboActionType.HeavyAttack);
            }
        }
    }
}