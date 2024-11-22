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

private void Update()
{
    // Ground check
    grounded = Physics.CheckSphere(
        transform.position - new Vector3(0, playerHeight * 0.5f, 0),
        groundCheckRadius,
        groundLayer
    );
    
    MyInput();
    
    // Handle drag
    rb.drag = grounded ? groundDrag : 0f;
}

private void FixedUpdate()
{
    MovePlayer();
    SpeedControl();
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

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Handle movement animation
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);
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
        if (Input.GetKeyDown(jumpKey) && grounded)
        {
            Debug.Log($"Jump attempt - readyToJump: {readyToJump}, grounded: {grounded}");
            Jump();
        }
    }

    private void MovePlayer()
    {
        if (playerAnimator != null && (playerAnimator.IsDead || playerAnimator.IsVictorious))
        {
            GameLogger.LogMovement("Movement blocked - player dead or victorious");
            return;
        }

        // Calculate movement direction relative to camera
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        
        // Keep movement on the ground plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // Only rotate if there's movement input
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                Time.deltaTime * rotationSpeed
            );
        }

        // Apply movement force
        float multiplier = grounded ? 1f : airMultiplier;
        rb.velocity = new Vector3(
            moveDirection.x * moveSpeed * multiplier,
            rb.velocity.y,
            moveDirection.z * moveSpeed * multiplier
        );
    }

    private void Jump()
    {
        if (!grounded)
        {
            GameLogger.LogMovement("Jump failed - not grounded", LogType.Warning);
            return;
        }
        
        if (!readyToJump)
        {
            GameLogger.LogMovement("Jump failed - on cooldown", LogType.Warning);
            return;
        }
        
        GameLogger.LogMovement("Jump initiated");
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        if (playerAnimator != null)
        {
            playerAnimator.TriggerJump();
        }
        
        grounded = false;
        StartCoroutine(JumpCooldownRoutine());
    }

    private void SpeedControl()
    {
        // Limit velocity if needed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
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
        // Visualize ground check
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position - new Vector3(0, playerHeight * 0.5f, 0),
            groundCheckRadius
        );
    }
}