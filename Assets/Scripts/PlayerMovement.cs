using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public float playerHeight = 2f;

    [Header("Jump Settings")]
    public int maxJumpCount = 2;
    private int jumpCount = 0;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    // Private variables
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private PlayerAnimator playerAnimator;
    private bool grounded;
    private bool readyToJump = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        readyToJump = true;
    }

    private void Update()
    {
        bool wasGrounded = grounded;
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + groundCheckDistance, groundLayer);
        
        // Reset jump count when grounded
        if (grounded)
        {
            jumpCount = 0;
        }
        
        // Check if we just landed
        if (!wasGrounded && grounded && playerAnimator != null)
        {
            playerAnimator.TriggerLand();
        }
        
        MyInput();
        SpeedControl();
        
        // Handle drag
        rb.drag = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate if we're moving
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);
        
        // Update animator
        if (playerAnimator != null)
        {
            playerAnimator.SetIsMoving(isMoving);
            if (isMoving)
            {
                float movementMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
                playerAnimator.SetMovementSpeed(movementMagnitude);
            }
        }

        // Handle jumping
        if (Input.GetKeyDown(jumpKey) && readyToJump && jumpCount < maxJumpCount)
        {
            readyToJump = false;
            jumpCount++;
            
            if (jumpCount == 1)
            {
                Jump(false);
            }
            else if (jumpCount == 2)
            {
                Jump(true);
            }

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        if (playerAnimator != null && (playerAnimator.IsDead || playerAnimator.IsVictorious))
            return;

        // Calculate movement direction
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;
        
        // Keep movement on the ground plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Calculate movement direction
        Vector3 moveDir = (forward * verticalInput + right * horizontalInput).normalized;

        // If there's no input, apply counter-force to stop sliding
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            // Get the horizontal velocity
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            
            // Apply counter force
            if (horizontalVel.magnitude > 0.01f)
            {
                Vector3 counterForce = -horizontalVel.normalized * moveSpeed * 2f;
                rb.AddForce(counterForce, ForceMode.Acceleration);
            }
            else
            {
                // If velocity is very low, just stop horizontal movement
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
            return;
        }

        // Calculate force to apply
        float force = moveSpeed * 10f;
        if (!grounded)
        {
            force *= airMultiplier;
        }
        else
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Force);
        }

        // Apply movement force
        rb.AddForce(moveDir * force, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump(bool isDoubleJump)
    {
        // Reset Y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Trigger appropriate jump animation
        if (playerAnimator != null)
        {
            if (isDoubleJump)
            {
                playerAnimator.TriggerDoubleJump();
            }
            else
            {
                playerAnimator.TriggerJump();
            }
        }

        // Apply jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}