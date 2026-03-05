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
    public float turnSmoothness = 5.0f;

    private float xRotation = 0f;

    void Update()
    {
        // Toggle ON = Free Roam | Toggle OFF = Menu/Stationary Mode
        bool isFreeRoam = movementToggle != null && movementToggle.isOn;

        // Manage UI interactability based on the toggle state
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
    }

    void HandleManualOverride()
    {
        if (Agent == null || joystick == null) return;

        Vector3 input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);

        if (input.magnitude > 0.2f)
        {
            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                Agent.velocity = Vector3.zero;
            }

            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDirection = (camForward * input.z + camRight * input.x).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Agent.transform.rotation = Quaternion.Slerp(Agent.transform.rotation, targetRotation, Time.deltaTime * turnSmoothness);
            }

            Agent.Move(moveDirection * manualMoveSpeed * Time.deltaTime);
            if (pathLine != null) pathLine.enabled = false;
        }
    }

    public void StartNavigation()
    {
        if (Agent != null && destinationDropdown != null)
        {
            bool isFreeRoam = movementToggle != null && movementToggle.isOn;

            // 1. If NOT in Free Roam, Teleport to the selected "Your Location"
            if (!isFreeRoam && startDropdown != null)
            {
                int startIndex = startDropdown.value;
                Agent.enabled = false; 
                Agent.transform.position = stationLocations[startIndex].position;
                Agent.enabled = true;
            }
            // 2. If IN Free Roam, we skip teleport and walk from the current spot automatically

            // 3. Always turn off Free Roam once navigation starts to let AI drive
            if (movementToggle != null) movementToggle.isOn = false;

            // 4. Set the destination
            int destinationIndex = destinationDropdown.value;
            Agent.isStopped = false;
            Agent.SetDestination(stationLocations[destinationIndex].position);

            if (uiPanel != null) uiPanel.SetActive(false);
            if (pathLine != null) pathLine.enabled = true;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * manualRotateSpeed * Time.deltaTime;
            Agent.transform.Rotate(Vector3.up * mouseX);
            float mouseY = Input.GetAxis("Mouse Y") * manualRotateSpeed * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -30f, 45f);
            mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

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