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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (freeLookCam == null)
        {
            freeLookCam = FindObjectOfType<CinemachineFreeLook>();
            if (freeLookCam == null)
            {
                Debug.LogError("No CinemachineFreeLook camera found in the scene!");
                return;
            }
        }

        ConfigureFreeLookCamera();
    }

    private void ConfigureFreeLookCamera()
    {
        if (freeLookCam != null)
        {
            // Adjust camera rigs for better jump visibility
            freeLookCam.m_Orbits[0].m_Height = 6f;    // Top view (increased height)
            freeLookCam.m_Orbits[0].m_Radius = 8f;
            
            freeLookCam.m_Orbits[1].m_Height = 3.5f;  // Middle view
            freeLookCam.m_Orbits[1].m_Radius = 7f;
            
            freeLookCam.m_Orbits[2].m_Height = 1f;    // Bottom view
            freeLookCam.m_Orbits[2].m_Radius = 6f;

            // Set follow and look targets
            freeLookCam.Follow = player;
            freeLookCam.LookAt = player;

            // Adjust camera settings
            freeLookCam.m_XAxis.m_MaxSpeed = 300f;
            freeLookCam.m_YAxis.m_MaxSpeed = 2f;
            
            // Improve camera behavior during jumps
            freeLookCam.m_YAxis.m_AccelTime = 0.1f;
            freeLookCam.m_YAxis.m_DecelTime = 0.1f;
            
            // Adjust camera damping
            freeLookCam.m_Heading.m_Bias = 0f;
            freeLookCam.m_XAxis.m_AccelTime = 0.1f;
            freeLookCam.m_XAxis.m_DecelTime = 0.1f;
        }
    }
}