using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    public float speed = 10f;

    public CharacterController characterController;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    public AnimationCurve PlayerGravityCurve;

    private float walkSpeed;
    private Rigidbody RB_player;
    private Transform T_player;
    private Vector3 moveDirection = Vector3.zero;
    #endregion  
    
    // Dampen the ability to jump to stop jump abuse
    public float jumpModifier = 0;
    private float jumpModifierMax = 1f;

    // The position A in a lerp.
    private Vector3 cur_position;
    private Quaternion cur_rotation;

    // The position being lerp'd too
    private Vector3 next_position;
    private Quaternion next_rotation;

    // Hand Animations
    public Animator HandAnimator;

    // UI
    public UI_Controller ui_controller;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 165;
        
        // Setup components
        // RB_player = GetComponent<Rigidbody>();
        T_player = transform;
        characterController = GetComponent<CharacterController>();
        ui_controller = FindObjectOfType<UI_Controller>();
        walkSpeed = speed * 0.78f;
    }

    Vector3 vel = Vector3.zero;

    void FixedUpdate()
    {
        HandleMoveInput();
        HandleMovement();
    }

    void HandleMoveInput()
    {
        vel = Vector3.zero;

        // Movement
        if (Input.GetKey(KeyCode.W))
            vel += transform.forward;

        if (Input.GetKey(KeyCode.S))
            vel += -transform.forward;

        if (Input.GetKey(KeyCode.D))
            vel += transform.right;

        if (Input.GetKey(KeyCode.A))
            vel += -transform.right;
    }

    public Vector3 move;
    public Vector3 airMove;

    float TimeWhenJumpStarted = 0;
    void HandleMovement()
    {
        // Reset movement vector
        move = Vector3.zero;

        // Cross hair spread
        ui_controller.ModifyCrosshairSpread(-Time.deltaTime * 2f); // Crosshair visuals

        // If Aiming or Shooting FORCE walk speed
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetKey(KeyCode.S))
        {
            move = (vel * walkSpeed);
            HandAnimator.SetFloat("PlayerSpeed", 0.5f);

            // Cross hair spread
            //ui_controller.ModifyCrosshairSpread(Time.deltaTime * 25f); // Crosshair visuals
        }
        // If Sprint button && not shooting || aiming => player can run
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            move = (vel * speed);
            HandAnimator.SetFloat("PlayerSpeed", 1f);

            // Cross hair spread
            //ui_controller.ModifyCrosshairSpread(Time.deltaTime * 50f); // Crosshair visuals
        }
        // Walking
        else
        {
            move = (vel * walkSpeed);
            HandAnimator.SetFloat("PlayerSpeed", 0.5f);

            // Cross hair spread
            //ui_controller.ModifyCrosshairSpread(Time.deltaTime * 25f); // Crosshair visuals
        }
        
        
        // IF player is on the ground allow them to jump
        if (characterController.isGrounded)
        {
            // Change jumpModifier
            if(jumpModifier > 0)
                jumpModifier -= Time.deltaTime;

            // if player pressess Jump command
            if (Input.GetKey(KeyCode.Space) /*|| Input.mouseScrollDelta.y != 0*/)
            {
                // Record elapsed time when the player hit the jump key
                TimeWhenJumpStarted = Time.time;

                // JumpModifier dampens how much & how often the player can fully jump
                moveDirection.y = jumpSpeed * (1f - (jumpModifier / jumpModifierMax));
                airMove = vel * walkSpeed;  // Save the movement input at the time player jumps
            }
        }
        // If the player is in the air increase modifier
        else
        {
            // Cross hair spread
            //ui_controller.ModifyCrosshairSpread(Time.deltaTime * 25f); // Crosshair visuals

            // stop increasing jumpmodifier to make sure it doesnt get too high
            if (jumpModifier < jumpModifierMax)
                jumpModifier += Time.deltaTime * .5f;
        }

        // Update gravity using acceleration curve. this will give the jump a non linear feel (depending on the graph)
        gravity = PlayerGravityCurve.Evaluate(Time.time - TimeWhenJumpStarted);

        // Gravity
        if(!characterController.isGrounded)
            moveDirection.y -= gravity;

        // Combine move and jump vectors
        move += moveDirection;

        // Call final movement on the player using calcuated values
        if (characterController.isGrounded)
        {
            // Final movement call when the player isn't in the air to avoid damning movement
            characterController.Move(move * Time.deltaTime);
        }
        else
        {
            // Calculate an airmovement vector for when the player is in air
            airMove.y = move.y;
            airMove.x = Mathf.Lerp(airMove.x, move.x, .1f);
            airMove.z = Mathf.Lerp(airMove.z, move.z, .1f);

            // Final Movement Call using a reduced airmovement
            characterController.Move(airMove * Time.deltaTime);
        }

        // if Player isnt receiving input, send animator playerSpeed = 0
        if (vel.x == 0 && vel.z == 0)
        {
            HandAnimator.SetFloat("PlayerSpeed", 0f);

        }
        else
        {
            // Cross hair spread
            ui_controller.ModifyCrosshairSpread(Time.deltaTime * 30f); // Crosshair visuals
        }
    }
}
