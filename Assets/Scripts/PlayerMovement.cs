using System.Collections; 
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;  // Add this line
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

    private void Jump(bool isDoubleJump)
    {
        // Reset vertical velocity before jump
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        // Apply jump force (same for both jumps)
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        // Trigger appropriate animation
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
    }


private void MovePlayer()
{
    if (playerAnimator != null && (playerAnimator.IsDead || playerAnimator.IsVictorious))
        return;

    // Get camera's forward and right vectors
    Vector3 forward = Camera.main.transform.forward;
    Vector3 right = Camera.main.transform.right;
    
    // Keep movement on the ground plane
    forward.y = 0;
    right.y = 0;
    forward.Normalize();
    right.Normalize();
    
    // Calculate movement direction relative to camera
    Vector3 moveDir = (forward * verticalInput + right * horizontalInput).normalized;

    // Only rotate if there's movement input
    if (moveDir != Vector3.zero)
    {
        // Rotate player to face movement direction
        transform.rotation = Quaternion.Lerp(transform.rotation, 
            Quaternion.LookRotation(moveDir), 
            Time.deltaTime * rotationSpeed);
    }

    // Apply movement
    if (moveDir.magnitude > 0)
    {
        rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);
    }
    else
    {
        // Counter sliding when no input
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.velocity = new Vector3(horizontalVel.x * 0.9f, rb.velocity.y, horizontalVel.z * 0.9f);
    }

    // Update animator
    if (playerAnimator != null)
    {
        playerAnimator.SetIsMoving(moveDir.magnitude > 0.1f);
        playerAnimator.SetMovementSpeed(moveDir.magnitude);
    }
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

    private void ResetJump()
    {
        readyToJump = true;
    }
}