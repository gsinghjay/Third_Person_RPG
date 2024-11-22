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

    private void Start()
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

        ConfigureFreeLookCamera();
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