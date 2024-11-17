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
            // Configure camera rigs
            freeLookCam.m_Orbits[0].m_Height = 4.5f;  // Top view
            freeLookCam.m_Orbits[0].m_Radius = 7f;
            
            freeLookCam.m_Orbits[1].m_Height = 2.5f;  // Middle view
            freeLookCam.m_Orbits[1].m_Radius = 7f;
            
            freeLookCam.m_Orbits[2].m_Height = 0.5f;  // Bottom view
            freeLookCam.m_Orbits[2].m_Radius = 7f;

            // Set follow target
            freeLookCam.Follow = player;
            freeLookCam.LookAt = player;

            // Camera rotation speeds
            freeLookCam.m_XAxis.m_MaxSpeed = 300f;  // Horizontal rotation
            freeLookCam.m_YAxis.m_MaxSpeed = 2f;    // Vertical rotation
        }
    }
}