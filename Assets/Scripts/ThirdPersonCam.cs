using UnityEngine;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public CinemachineFreeLook freeLookCam;

    private PlayerAnimator playerAnimator;
    
    [Header("Camera Settings")]
    [SerializeField] private float rotationSpeed = 7f;

 [Header("Double Jump Camera")]
    public float doubleJumpHeight = 3f;
    public float doubleJumpDistance = 5f;
    private Vector3 initialDoubleJumpPosition;
    private Quaternion initialDoubleJumpRotation;
    private bool isInDoubleJump;
    
    public float defaultMiddleRigRadius = 7f;
    public float doubleJumpMiddleRigRadius = 10f;
    public float transitionSpeed = 5f;
    public float doubleJumpYAngle = 0.5f;  // Add this line
    public float doubleJumpXAngle = 180f;  // Add this line 
    private float lastXInput;
    private float lastYInput;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeReferences();
    }

private void InitializeReferences()
{
    if (player != null)
    {
        playerAnimator = player.GetComponentInChildren<PlayerAnimator>();
    }
        
    if (freeLookCam == null)
    {
        freeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCam == null)
        {
            Debug.LogError("No CinemachineFreeLook camera found in the scene!");
            return;
        }
    }

    // Configure FreeLook camera settings
    ConfigureFreeLookCamera();
}

private void Update()
    {
        if (player == null || freeLookCam == null) return;

        HandleDoubleJumpCamera();
    }

   private void HandleDoubleJumpCamera()
    {
        if (playerAnimator == null) return;

        if (playerAnimator.IsDoubleJumping)
        {
            // Store initial position and rotation when double jump starts
            if (!isInDoubleJump)
            {
                isInDoubleJump = true;
                initialDoubleJumpPosition = freeLookCam.transform.position;
                initialDoubleJumpRotation = freeLookCam.transform.rotation;

                // Disable camera input during double jump
                freeLookCam.m_XAxis.m_InputAxisValue = 0;
                freeLookCam.m_YAxis.m_InputAxisValue = 0;
            }

            // Calculate fixed camera position during double jump
            Vector3 playerPos = player.position;
            Vector3 targetPos = playerPos - player.forward * doubleJumpDistance;
            targetPos.y = playerPos.y + doubleJumpHeight;

            // Smoothly move camera to target position
            freeLookCam.transform.position = Vector3.Lerp(
                freeLookCam.transform.position,
                targetPos,
                Time.deltaTime * 10f
            );

            // Ensure camera always looks at player
            Vector3 lookDirection = (playerPos + Vector3.up - freeLookCam.transform.position).normalized;
            freeLookCam.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        else if (isInDoubleJump)
        {
            // Smoothly return to normal camera behavior
            isInDoubleJump = false;
            
            // Re-enable camera input
            freeLookCam.m_XAxis.m_InputAxisValue = Input.GetAxis("Mouse X");
            freeLookCam.m_YAxis.m_InputAxisValue = Input.GetAxis("Mouse Y");
        }
    }

private void ConfigureFreeLookCamera()
    {
        if (freeLookCam != null)
        {
            // Configure orbits
            freeLookCam.m_Orbits[0].m_Height = 4.5f;  // Top rig
            freeLookCam.m_Orbits[0].m_Radius = 7f;
            
            freeLookCam.m_Orbits[1].m_Height = 2.5f;  // Middle rig
            freeLookCam.m_Orbits[1].m_Radius = 7f;
            
            freeLookCam.m_Orbits[2].m_Height = 0.5f;  // Bottom rig
            freeLookCam.m_Orbits[2].m_Radius = 7f;

            // Set up follow settings
            freeLookCam.Follow = player;
            freeLookCam.LookAt = player;

            // Configure FOV and clipping
            freeLookCam.m_Lens.FieldOfView = 60f;
            freeLookCam.m_Lens.NearClipPlane = 0.3f;
            freeLookCam.m_Lens.FarClipPlane = 1000f;

            // Configure axis settings
            freeLookCam.m_XAxis.m_MaxSpeed = 300f;
            freeLookCam.m_XAxis.m_AccelTime = 0.1f;
            freeLookCam.m_XAxis.m_DecelTime = 0.1f;
            
            freeLookCam.m_YAxis.m_MaxSpeed = 2f;
            freeLookCam.m_YAxis.m_AccelTime = 0.2f;
            freeLookCam.m_YAxis.m_DecelTime = 0.2f;
            freeLookCam.m_YAxis.m_MinValue = 0.1f;
            freeLookCam.m_YAxis.m_MaxValue = 0.9f;

            // Set camera damping
            var composer = freeLookCam.GetComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_HorizontalDamping = 1f;
                composer.m_VerticalDamping = 1f;
            }
        }
    }


private void UpdateCameraRadius()
{
    if (freeLookCam != null && playerAnimator != null)
    {
        if (playerAnimator.IsDoubleJumping)
        {
            // Set specific camera angle for double jump
            freeLookCam.m_XAxis.Value = Mathf.Lerp(freeLookCam.m_XAxis.Value, doubleJumpXAngle, Time.deltaTime * transitionSpeed);
            freeLookCam.m_YAxis.Value = Mathf.Lerp(freeLookCam.m_YAxis.Value, doubleJumpYAngle, Time.deltaTime * transitionSpeed);
            
            float targetRadius = doubleJumpMiddleRigRadius;
            freeLookCam.m_Orbits[1].m_Radius = Mathf.Lerp(freeLookCam.m_Orbits[1].m_Radius, targetRadius, Time.deltaTime * transitionSpeed);
        }
        else
        {
            float targetRadius = defaultMiddleRigRadius;
            freeLookCam.m_Orbits[1].m_Radius = Mathf.Lerp(freeLookCam.m_Orbits[1].m_Radius, targetRadius, Time.deltaTime * transitionSpeed);
        }
    }
}

    private void HandleRotation()
    {
        if (playerAnimator != null && !playerAnimator.IsDead && !playerAnimator.IsVictorious)
        {
            float horizontalInput = Input.GetAxis("Mouse X");
            playerObj.Rotate(Vector3.up * horizontalInput * rotationSpeed);
        }
    }
}