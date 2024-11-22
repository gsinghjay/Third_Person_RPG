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

    [Header("Dynamic Camera Settings")]
    [SerializeField] private float minRadius = 6f;
    [SerializeField] private float maxRadius = 10f;
    [SerializeField] private float radiusTransitionSpeed = 5f;
    [SerializeField] private float maxPlayerSpeed = 10f; // Maximum expected player speed
    
    [Header("Combat Camera Settings")]
    [SerializeField] private float combatFOV = 60f;
    [SerializeField] private float normalFOV = 50f;
    [SerializeField] private float fovTransitionSpeed = 3f;

    private float currentRadius;
    private float targetRadius;
    private float currentFOV;
    private PlayerMovement playerMovement;

    private void Start()
    {
        InitializeComponents();
        ConfigureFreeLookCamera();
        InitializeDynamicSettings();
    }

    private void InitializeComponents()
    {
        GameLogger.LogCamera("Initializing third person camera");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (freeLookCam == null)
        {
            freeLookCam = FindObjectOfType<CinemachineFreeLook>();
            if (freeLookCam == null)
            {
                GameLogger.LogCamera("No CinemachineFreeLook camera found in the scene!", LogType.Error);
                return;
            }
        }

        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void InitializeDynamicSettings()
    {
        currentRadius = minRadius;
        targetRadius = minRadius;
        currentFOV = normalFOV;
    }

    private void Update()
    {
        if (!freeLookCam || !rb) return;
        
        UpdateCameraRadius();
        UpdateCameraFOV();
    }

    private void UpdateCameraRadius()
    {
        // Calculate target radius based on player speed
        float speedRatio = rb.velocity.magnitude / maxPlayerSpeed;
        targetRadius = Mathf.Lerp(minRadius, maxRadius, speedRatio);

        // Smoothly transition current radius
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * radiusTransitionSpeed);

        // Apply to all orbits while maintaining their relative proportions
        float baseRadius = freeLookCam.m_Orbits[1].m_Radius; // Middle orbit as reference
        if (baseRadius != 0)
        {
            for (int i = 0; i < freeLookCam.m_Orbits.Length; i++)
            {
                float radiusRatio = freeLookCam.m_Orbits[i].m_Radius / baseRadius;
                freeLookCam.m_Orbits[i].m_Radius = currentRadius * radiusRatio;
            }
        }
    }

    private void UpdateCameraFOV()
    {
        // Adjust FOV based on player state (you can add more conditions)
        float targetFOV = playerMovement && playerMovement.IsGrounded() ? normalFOV : combatFOV;
        
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);
        freeLookCam.m_Lens.FieldOfView = currentFOV;
    }

    private void ConfigureFreeLookCamera()
    {
        if (freeLookCam != null)
        {
            GameLogger.LogCamera("Configuring CinemachineFreeLook camera settings");
            
            // Configure orbits
            freeLookCam.m_Orbits[0].m_Height = 6f;
            freeLookCam.m_Orbits[0].m_Radius = 8f;
            GameLogger.LogCamera("Top rig configured - Height: 6, Radius: 8");
            
            freeLookCam.m_Orbits[1].m_Height = 3.5f;
            freeLookCam.m_Orbits[1].m_Radius = 7f;
            GameLogger.LogCamera("Middle rig configured - Height: 3.5, Radius: 7");
            
            freeLookCam.m_Orbits[2].m_Height = 1f;
            freeLookCam.m_Orbits[2].m_Radius = 6f;
            GameLogger.LogCamera("Bottom rig configured - Height: 1, Radius: 6");

            // Set targets
            freeLookCam.Follow = player;
            freeLookCam.LookAt = player;
            GameLogger.LogCamera("Camera targets set");

            // Configure camera behavior
            ConfigureCameraSettings();
        }
        else
        {
            GameLogger.LogCamera("Failed to configure camera - freeLookCam is null", LogType.Error);
        }
    }

    private void ConfigureCameraSettings()
    {
        GameLogger.LogCamera("Configuring camera behavior settings");
        
        freeLookCam.m_XAxis.m_MaxSpeed = 300f;
        freeLookCam.m_YAxis.m_MaxSpeed = 2f;
        
        freeLookCam.m_YAxis.m_AccelTime = 0.1f;
        freeLookCam.m_YAxis.m_DecelTime = 0.1f;
        
        freeLookCam.m_Heading.m_Bias = 0f;
        freeLookCam.m_XAxis.m_AccelTime = 0.1f;
        freeLookCam.m_XAxis.m_DecelTime = 0.1f;
        
        GameLogger.LogCamera("Camera settings configured successfully");
    }
}