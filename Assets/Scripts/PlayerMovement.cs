using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float groundDrag = 5f;
    
    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public float playerHeight = 2f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;
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

    private Vector2 moveInput;
    private bool jumpRequested;
    private Vector3 targetMoveDirection;
    private Vector3 currentVelocity;
    private float targetRotation;
    private float rotationVelocity;

    private void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();

        // Configure rigidbody
        rb.freezeRotation = true;
        
        // Lock cursor for game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool CheckGround()
    {
        // Center position for ground checks
        Vector3 spherePosition = transform.position - new Vector3(0, playerHeight * 0.5f, 0);
        
        // Primary raycast check
        bool raycastHit = Physics.Raycast(
            transform.position,
            Vector3.down,
            out RaycastHit hit,
            playerHeight * 0.5f + groundCheckDistance,
            groundLayer
        );

        // Secondary spherecast for better edge detection
        bool spherecastHit = Physics.CheckSphere(
            spherePosition,
            groundCheckRadius,
            groundLayer
        );

        return raycastHit || spherecastHit;
    }

    private void Update()
    {
        // Handle input
        ProcessInput();
        
        // Ground check
        grounded = CheckGround();
        
        // Update animation states
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        // Handle physics-based movement
        ProcessMovement();
        
        // Apply drag
        ApplyGroundDrag();
        
        // Handle jumping physics
        ProcessJump();
        
        // Enforce speed limits
        ClampVelocity();
    }

    private void ProcessInput()
    {
        // Cache input values
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        // Handle jump input
        if (Input.GetKeyDown(jumpKey) && grounded && readyToJump)
        {
            jumpRequested = true;
        }

        // Calculate move direction relative to camera
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        
        targetMoveDirection = (forward.normalized * moveInput.y + right.normalized * moveInput.x).normalized;
    }

    private void ProcessMovement()
    {
        if (playerAnimator != null && (playerAnimator.IsDead || playerAnimator.IsVictorious))
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        if (targetMoveDirection != Vector3.zero)
        {
            // Calculate target rotation
            targetRotation = Mathf.Atan2(targetMoveDirection.x, targetMoveDirection.z) * Mathf.Rad2Deg;
            
            // Smooth rotation
            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation,
                ref rotationVelocity,
                0.1f
            );
            
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // Calculate target velocity
        float multiplier = grounded ? 1f : airMultiplier;
        Vector3 targetVelocity = targetMoveDirection * moveSpeed * multiplier;
        
        // Smoothly interpolate to target velocity
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 15f
        );

        // Apply horizontal velocity
        rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
    }

    private void ProcessJump()
    {
        if (jumpRequested && grounded && readyToJump)
        {
            jumpRequested = false;
            
            // Reset vertical velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            
            // Apply jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            if (playerAnimator != null)
            {
                playerAnimator.TriggerJump();
            }
            
            grounded = false;
            StartCoroutine(JumpCooldownRoutine());
        }
    }

    private void ApplyGroundDrag()
    {
        rb.drag = grounded ? groundDrag : 0f;
    }

    private void ClampVelocity()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void UpdateAnimationState()
    {
        if (playerAnimator != null)
        {
            bool isMoving = moveInput.magnitude > 0.1f;
            playerAnimator.SetIsMoving(isMoving);
            if (isMoving)
            {
                playerAnimator.SetMovementSpeed(moveInput.magnitude);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Ground collision detected");
            grounded = true;
            readyToJump = true;
            if (playerAnimator != null)
            {
                Debug.Log("Triggering land animation");
                playerAnimator.TriggerLand();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
    }

    private IEnumerator JumpCooldownRoutine()
    {
        readyToJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
        GameLogger.LogMovement("Jump cooldown finished");
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 spherePosition = transform.position - new Vector3(0, playerHeight * 0.5f, 0);
        
        // Visualize raycast
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * (playerHeight * 0.5f + groundCheckDistance)
        );
        
        // Visualize sphere check
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
    }
}