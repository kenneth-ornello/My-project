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

    [Header("Movement Tuning")]
    // Set this to 2 or 3 in the Inspector for a slow, realistic body turn
    public float turnSmoothness = 5.0f; 

    private float xRotation = 0f;

    void Update()
    {
        // Toggle ON = Manual Joystick Mode | Toggle OFF = AI Mode
        bool isManualMode = movementToggle != null && movementToggle.isOn;

        if (isManualMode)
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

        // Handles Right-Click (Laptop) and Right-Thumb (Phone)
        HandleRotation();
    }

    void HandleManualOverride()
    {
        if (Agent == null || joystick == null) return;

        Vector3 input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);

        // Deadzone of 0.2f prevents "twitchy" movement
        if (input.magnitude > 0.2f)
        {
            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                Agent.velocity = Vector3.zero;
            }

            // Calculate direction relative to Camera orientation
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * input.z + camRight * input.x).normalized;

            // Character turns to face movement direction using turnSmoothness
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Agent.transform.rotation = Quaternion.Slerp(Agent.transform.rotation, targetRotation, Time.deltaTime * turnSmoothness);
            }

            // Move the agent forward at manualMoveSpeed
            Agent.Move(moveDirection * manualMoveSpeed * Time.deltaTime);

            if (pathLine != null) pathLine.enabled = false;
        }
    }

    void HandleRotation()
    {
        // LAPTOP: Right-Click (Mouse 1) rotation
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * manualRotateSpeed * Time.deltaTime;
            Agent.transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * manualRotateSpeed * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -30f, 45f);
            mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

        // PHONE: Right-Thumb/Multi-touch rotation
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    float hRotation = touch.deltaPosition.x * (manualRotateSpeed * 0.005f);
                    Agent.transform.Rotate(Vector3.up * hRotation);

                    float vRotation = touch.deltaPosition.y * (manualRotateSpeed * 0.005f);
                    xRotation -= vRotation;
                    xRotation = Mathf.Clamp(xRotation, -30f, 45f);
                    mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
                }
            }
        }
    }

    public void StartNavigation()
    {
        // Now starts from CURRENT position instead of teleporting to startDropdown
        if (Agent != null && destinationDropdown != null)
        {
            // Switch off manual mode automatically
            if (movementToggle != null) movementToggle.isOn = false;

            int destinationIndex = destinationDropdown.value;

            // Ensure agent is active and ready to move
            Agent.enabled = true;
            Agent.isStopped = false;

            // Set destination from current coordinates
            Agent.SetDestination(stationLocations[destinationIndex].position);

            if (uiPanel != null) uiPanel.SetActive(false);
            if (pathLine != null) pathLine.enabled = true;
        }
    }

    void CheckArrival()
    {
        if (Agent != null && uiPanel != null && !uiPanel.activeSelf)
        {
            if (Agent.isOnNavMesh && !Agent.pathPending && Agent.remainingDistance < 0.5f)
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
            point.y += 0.1f;
            pathLine.SetPosition(i, point);
        }
    }
}