using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Public values
        //References to other scripts
    public CharacterController controller;
    public InputMaster controls;
    public Transform followingCamera;

    //Private values
        //Player properties
    private Vector2 characterDirection;
    private float speed;
    private float turnDirectionSmoothTime;
    private float turnDirectionSmoothVelocity;
    private float jumpForce;
        //Script function vals
    private Dictionary<Vector2, bool> movementKeysActivation;
    public float gravity;
    private bool isJumping;
    private float verticalVelocity;
    private Vector3 lastMoveVectorForJump;

    // Awake is called befor Start
    private void Awake()
    {
        //Getting the reference to the Input Script
        controls = new InputMaster();
        //Reading when any of the movement buttons get pressed and sending Vector that represents the movement
        //(for some reason InputSystem doesn't send accurate Vector2 Data when various keys are pressed so I do it myself)
        controls.Player.MoveForward.performed += context => Move(new Vector2(0f, 1f));
        controls.Player.MoveBack.performed += context => Move(new Vector2(0f, -1f));
        controls.Player.MoveLeft.performed += context => Move(new Vector2(-1f, 0f));
        controls.Player.MoveRight.performed += context => Move(new Vector2(1f, 0f));
        //Reading Jump button press
        controls.Player.Jump.performed += context => Jump();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initializing properties
        characterDirection = new Vector2(0f, 0f);
        speed = 5f;
        turnDirectionSmoothTime = 0.1f;
        jumpForce = 2.5f;
        //Initializing Script values
        movementKeysActivation = new Dictionary<Vector2, bool> {
            { new Vector2(0f, 1f), false },
            { new Vector2(0f, -1f), false },
            { new Vector2(-1f, 0f), false },
            { new Vector2(1f, 0f), false }
        };
        gravity = 9.81f;
        isJumping = false;
        verticalVelocity = 0f;
        lastMoveVectorForJump = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 finalMovement = new Vector3(characterDirection.x, 0f, characterDirection.y).normalized;
        //finalMovement = Vector3.ClampMagnitude(finalMovement, speed);

        if (controller.isGrounded)
        {
            //If player is moving read the camera view to rotate the character and make movement always follow it's orientation
            //making the third person shooter movement scheme.
            if (finalMovement.magnitude >= 0.1) {
                float targetAngle = Mathf.Atan2(finalMovement.x, finalMovement.z) * Mathf.Rad2Deg + followingCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnDirectionSmoothVelocity, turnDirectionSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                finalMovement = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }

            //apply gravity if in ground (because the world works like that)
            verticalVelocity = -gravity * Time.deltaTime;

            //If player jumps, apply jump force to our Y velocity and save the last movement vector to follow the jump orientation
            //at start and finish the parabola even if there is not movement input at mid air.
            if (isJumping)
            {
                verticalVelocity = jumpForce;
                lastMoveVectorForJump = finalMovement;
                isJumping = false;
            }
        }
        else { //While in midair, apply gravity and set the movement vector as the one at the beginning of the jump.
            verticalVelocity -= gravity * Time.deltaTime;
            finalMovement = lastMoveVectorForJump;
        }

        //Whatever happened with the inputs, apply Y velocity to the vector and Move, adding speed of player.
        finalMovement.y = verticalVelocity;
        controller.Move(finalMovement * speed * Time.deltaTime);
    }

    void Move(Vector2 direction) {
        //Receive a Vector2 info to add or substract to change movement direction
        if (movementKeysActivation.ContainsKey(direction)) {
            if (movementKeysActivation[direction]) //If Vector2 in dictionary is true, then the player have released pressing so we substract
            {
                characterDirection -= direction;
                movementKeysActivation[direction] = false;
            }
            else { //If false then player started pressing, let's add to the direction
                characterDirection += direction;
                movementKeysActivation[direction] = true;
            }
        }
    }

    void Jump() {
        //Checking if player is in ground, if does activate jump
        //(Do we really need this function? Maybe, to keep code clean(?)).
        if (controller.isGrounded) {
            isJumping = true;
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}

