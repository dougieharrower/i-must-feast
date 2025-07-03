using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    public float sneakSpeed = 1.5f; // Adjust as needed
    private bool isSneakHeld;

    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 inputMove;
    private bool isJumpPressed;
    private bool isSprintHeld;

    private InputSystem_Actions inputActions;

    private bool isFeastPressed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Initialize and enable input actions
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Movement input
        inputActions.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => inputMove = Vector2.zero;

        // Jump input
        inputActions.Player.Jump.performed += _ => isJumpPressed = true;
        inputActions.Player.Jump.canceled += _ => isJumpPressed = false;

        // Sprint input
        inputActions.Player.Sprint.performed += _ => isSprintHeld = true;
        inputActions.Player.Sprint.canceled += _ => isSprintHeld = false;

        // Sneak input
        inputActions.Player.Sneak.performed += _ => isSneakHeld = true;
        inputActions.Player.Sneak.canceled += _ => isSneakHeld = false;

        // Feast input
inputActions.Player.Feast.performed += _ => isFeastPressed = true;
inputActions.Player.Feast.canceled += _ => isFeastPressed = false;


    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Camera-relative movement
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 move = (camForward * inputMove.y + camRight * inputMove.x).normalized;
        float currentSpeed = isSneakHeld ? sneakSpeed : (isSprintHeld ? runSpeed : walkSpeed);


        // Rotate Baune to face movement direction
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (isJumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public bool IsSneaking()
    {
        return isSneakHeld;
    }
    public bool IsStandingStill()
    {
        return inputMove == Vector2.zero;
    }

public bool IsFeastPressed()
{
    return isFeastPressed;
}

}
