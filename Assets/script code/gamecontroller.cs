using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Camera mainCam;

    [Header("UI Elements")]
    public Dropdown startDropdown;
    public Dropdown destinationDropdown;
    public GameObject uiPanel;
    public LineRenderer pathLine;
    public Toggle movementToggle;

    [Header("Joystick Setup")]
    public Joystick joystick;

    [Header("Settings")]
    public float manualMoveSpeed = 12.0f;
    public float manualRotateSpeed = 150.0f;
    public Transform[] stationLocations;

    [Header("Platform Triggers")]
    public Transform platform1Trigger; 
    public Transform platform2Trigger; 
    public float detectionRadius = 10.0f; // Increased default for better mobile detection

    [Header("Platform Detection")]
    public TrainStationManager stationManager;

    private float xRotation = 0f;

    void Start()
    {
        if (stationManager != null)
        {
            // Reset state on launch
            stationManager.playerCurrentPlatform = "";
            stationManager.UpdateARDisplay(); 
        }
    }

    void Update()
    {
        bool isFreeRoam = movementToggle != null && movementToggle.isOn;

        if (startDropdown != null) startDropdown.interactable = !isFreeRoam;
        if (joystick != null) joystick.gameObject.SetActive(isFreeRoam);

        if (isFreeRoam)
        {
            HandleManualOverride();
        }
        else
        {
            if (Agent != null && Agent.enabled)
            {
                if (Agent.isOnNavMesh) Agent.isStopped = false;
                if (Agent.hasPath) DrawPathLine();
                CheckArrival();
            }
        }

        HandleRotation();
        
        // Secondary check for mobile if triggers are missed
        CheckPlatformDistance();
    }

    // --- DETECTION LOGIC ---
    
    private void OnTriggerEnter(Collider other)
    {
        if (stationManager == null) return;

        // Using .Contains to handle naming variations like "PF1 (Clone)"
        if (other.CompareTag("PF1") || other.name.Contains("PF1"))
        {
            SetPlatform("PF 1");
        }
        else if (other.CompareTag("PF2") || other.name.Contains("PF2"))
        {
            SetPlatform("PF 2");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (stationManager == null) return;

        if (other.CompareTag("PF1") || other.CompareTag("PF2"))
        {
            // Optional: Uncomment to clear data when walking away
            // SetPlatform("");
        }
    }

    void CheckPlatformDistance()
    {
        if (stationManager == null || Agent == null) return;

        // Check PF1
        if (platform1Trigger != null)
        {
            float dist1 = Vector3.Distance(Agent.transform.position, platform1Trigger.position);
            if (dist1 < detectionRadius && stationManager.playerCurrentPlatform != "PF 1")
            {
                SetPlatform("PF 1");
            }
        }

        // Check PF2
        if (platform2Trigger != null)
        {
            float dist2 = Vector3.Distance(Agent.transform.position, platform2Trigger.position);
            if (dist2 < detectionRadius && stationManager.playerCurrentPlatform != "PF 2")
            {
                SetPlatform("PF 2");
            }
        }
    }

    void SetPlatform(string platformName)
    {
        if (stationManager == null) return;
        
        stationManager.playerCurrentPlatform = platformName;
        stationManager.UpdateARDisplay();
        
        // Debugging for your phone screen
        if (stationManager.arDisplayLabel != null && !string.IsNullOrEmpty(platformName))
        {
            Debug.Log("Switched to " + platformName);
        }
    }

    // --- MOVEMENT & NAVIGATION ---

    void HandleManualOverride()
    {
        if (Agent == null || joystick == null) return;

        Vector3 input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);

        if (input.magnitude > 0.1f) // Slightly more sensitive for phone joysticks
        {
            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }

            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDirection = (camForward * input.z + camRight * input.x).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Agent.transform.rotation = Quaternion.Slerp(Agent.transform.rotation, targetRotation, Time.deltaTime * 8.0f);
            }

            Agent.Move(moveDirection * manualMoveSpeed * Time.deltaTime);
        }
    }

    public void StartNavigation()
    {
        if (Agent == null || destinationDropdown == null) return;

        if (movementToggle != null) movementToggle.isOn = false;

        int destinationIndex = destinationDropdown.value;
        Agent.isStopped = false;
        Agent.SetDestination(stationLocations[destinationIndex].position);

        if (uiPanel != null) uiPanel.SetActive(false);
        if (pathLine != null) pathLine.enabled = true;
    }

    void HandleRotation()
    {
        // PC Mouse Rotation
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * manualRotateSpeed * Time.deltaTime;
            Agent.transform.Rotate(Vector3.up * mouseX);
        }

        // Mobile Touch Rotation
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.position.x > Screen.width * 0.4f && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    float hRotation = touch.deltaPosition.x * (manualRotateSpeed * 0.002f);
                    Agent.transform.Rotate(Vector3.up * hRotation);
                }
            }
        }
    }

    void CheckArrival()
    {
        if (Agent != null && uiPanel != null && !uiPanel.activeSelf)
        {
            if (Agent.isOnNavMesh && !Agent.pathPending && Agent.remainingDistance < 0.8f)
            {
                uiPanel.SetActive(true);
                if (pathLine != null) pathLine.enabled = false;
            }
        }
    }

    void DrawPathLine()
    {
        if (pathLine == null || !Agent.hasPath) return;
        pathLine.positionCount = Agent.path.corners.Length;
        for (int i = 0; i < Agent.path.corners.Length; i++)
        {
            Vector3 point = Agent.path.corners[i];
            point.y += 0.2f;
            pathLine.SetPosition(i, point);
        }
    }
}