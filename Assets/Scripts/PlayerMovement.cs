using UnityEngine;

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
    // Ground check with debug
    grounded = Physics.CheckSphere(
        transform.position - new Vector3(0, playerHeight * 0.5f, 0),
        groundCheckRadius,
        groundLayer
    );
    
    Debug.Log($"Ground Check - Position: {transform.position}, Height Offset: {playerHeight * 0.5f}, Is Grounded: {grounded}");

    MyInput();
    
    // Handle drag with debug
    rb.drag = grounded ? groundDrag : 0f;
    Debug.Log($"Current Drag: {rb.drag}, Velocity: {rb.velocity}");
}

private void FixedUpdate()
{
    MovePlayer();
    SpeedControl();
    
    // Debug physics state
    Debug.Log($"FixedUpdate - Position: {transform.position}, Velocity: {rb.velocity}, IsKinematic: {rb.isKinematic}");
}

private void OnCollisionEnter(Collision collision)
{
    Debug.Log($"Collision Enter with: {collision.gameObject.name}, Normal: {collision.contacts[0].normal}");
}

private void OnCollisionStay(Collision collision)
{
    Debug.Log($"Collision Stay with: {collision.gameObject.name}, Contact Count: {collision.contactCount}");
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
        if (Input.GetKeyDown(jumpKey))
        {
            Debug.Log($"Jump key pressed. Grounded: {grounded}, ReadyToJump: {readyToJump}");
            if (readyToJump && grounded)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
    }

    private void MovePlayer()
    {
        if (playerAnimator != null && (playerAnimator.IsDead || playerAnimator.IsVictorious))
            return;

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
    Debug.Log($"Jump triggered. Grounded: {grounded}, ReadyToJump: {readyToJump}");
    Debug.Log($"Pre-Jump Velocity: {rb.velocity}");
    
    // Reset y velocity
    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    Debug.Log($"Reset Velocity: {rb.velocity}");
    
    // Apply jump force
    Vector3 jumpVector = Vector3.up * jumpForce;
    rb.AddForce(jumpVector, ForceMode.Impulse);
    Debug.Log($"Applied Jump Force: {jumpVector}, New Velocity: {rb.velocity}");
    
    if (playerAnimator != null)
    {
        playerAnimator.TriggerJump();
        Debug.Log("Jump animation triggered");
    }
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

    private void ResetJump()
    {
        readyToJump = true;
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