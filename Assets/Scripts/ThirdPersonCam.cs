using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    private PlayerAnimator playerAnimator;

    [Header("Camera Settings")]
    public float rotationSpeed = 7f;
    public float followSpeed = 10f;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float minDistance = 1f; // Add minimum distance
    public float maxDistance = 10f; // Add maximum distance

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat,
        Topdown
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player != null)
        {
            playerAnimator = player.GetComponentInChildren<PlayerAnimator>();
        }
        
        // Set initial camera style
        SwitchCameraStyle(CameraStyle.Basic);
    }

    private void LateUpdate()
    {
        if (player == null) return;

        HandleCameraPosition();
        HandleCameraRotation();
        HandleCameraStyleSwitch();
    }

private void HandleCameraPosition()
{
    if (player == null) return;

    // Calculate desired position
    Vector3 targetPos = player.position + cameraOffset;
    
    // Check for obstacles
    RaycastHit hit;
    Vector3 directionToCamera = (targetPos - player.position).normalized;
    float targetDistance = cameraOffset.magnitude;
    
    if (Physics.Raycast(player.position, directionToCamera, out hit, targetDistance))
    {
        // If there's an obstacle, adjust the camera position
        targetPos = hit.point - directionToCamera * 0.5f;
    }
    
    // Smoothly move camera
    transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    
    // Always look at player
    transform.LookAt(player.position + Vector3.up * 1.5f); // Offset to look at upper body
}

    private void HandleCameraRotation()
     {
        if (player == null) return;

        // Calculate view direction with safety checks
        Vector3 viewDir = (player.position - transform.position);
        if (viewDir.magnitude > 0.01f) // Prevent division by zero
        {
            orientation.forward = viewDir.normalized;
        }

        // Handle player rotation based on camera style
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            HandleBasicRotation();
        }
        else if (currentStyle == CameraStyle.Combat)
        {
            HandleCombatRotation();
        }
    }

    private void HandleBasicRotation()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleCombatRotation()
    {
        if (combatLookAt != null)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;
            playerObj.forward = dirToCombatLookAt.normalized;
        }
    }

    private void HandleCameraStyleSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Combat);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.Topdown);
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        // Disable all cameras first
        if (combatCam != null) combatCam.SetActive(false);
        if (thirdPersonCam != null) thirdPersonCam.SetActive(false);
        if (topDownCam != null) topDownCam.SetActive(false);

        // Enable the selected camera
        switch (newStyle)
        {
            case CameraStyle.Basic:
                if (thirdPersonCam != null) thirdPersonCam.SetActive(true);
                break;
            case CameraStyle.Combat:
                if (combatCam != null) combatCam.SetActive(true);
                break;
            case CameraStyle.Topdown:
                if (topDownCam != null) topDownCam.SetActive(true);
                break;
        }

        currentStyle = newStyle;
    }
}