using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // For Basic UI (Start/Destination)
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro; // For your new Coach Dropdown

public class GameController : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Camera mainCam;

    [Header("UI Elements")]
    public Dropdown startDropdown;       // Basic UI
    public Dropdown destinationDropdown; // Basic UI
    public TMP_Dropdown coachDropdown;   // TextMeshPro (This fix allows dragging your new UI)
    public GameObject uiPanel;
    public LineRenderer pathLine;
    public Toggle movementToggle;

    [Header("Joystick Setup")]
    public Joystick joystick;

    [Header("Settings")]
    public float manualMoveSpeed = 12.0f;
    public float manualRotateSpeed = 150.0f;
    public Transform[] stationLocations;

    [Header("Manual Coach Setup")]
    // Drag your specific poles from the Hierarchy into this list
    public Transform[] platform1Coaches; 

    [Header("Platform Triggers")]
    public Transform platform1Trigger; 
    public Transform platform2Trigger; 
    public float detectionRadius = 10.0f; 

    [Header("Platform Detection")]
    public TrainStationManager stationManager;

    void Start()
    {
        if (stationManager != null)
        {
            stationManager.playerCurrentPlatform = "";
            stationManager.UpdateARDisplay(); 
        }

        // Listener for Teleporting (Your Location)
        if (startDropdown != null)
            startDropdown.onValueChanged.AddListener(delegate { TeleportToStartLocation(); });

        // Listener for Platform Selection (Your Destination)
        if (destinationDropdown != null)
            destinationDropdown.onValueChanged.AddListener(delegate { OnDestinationChanged(); });

        // Keep coach dropdown hidden at the start
        if (coachDropdown != null) coachDropdown.gameObject.SetActive(false);
    }

    void OnDestinationChanged()
    {
        if (destinationDropdown == null) return;

        string selectedText = destinationDropdown.options[destinationDropdown.value].text;

        if (selectedText.Contains("Platform 1"))
        {
            SetupCoachDropdown(); 
            coachDropdown.gameObject.SetActive(true); 
        }
        else
        {
            coachDropdown.gameObject.SetActive(false); 
        }
    }

    void SetupCoachDropdown()
    {
        if (platform1Coaches == null || coachDropdown == null) return;

        coachDropdown.ClearOptions();
        List<string> coachNames = new List<string>();

        foreach (Transform coach in platform1Coaches)
        {
            if (coach != null) coachNames.Add(coach.name);
        }

        coachDropdown.AddOptions(coachNames);
    }

    public void StartNavigation()
    {
        if (Agent == null || destinationDropdown == null) return;

        Agent.Warp(transform.position);
        if (movementToggle != null) movementToggle.isOn = false;

        Vector3 target;

        if (coachDropdown != null && coachDropdown.gameObject.activeSelf && platform1Coaches.Length > 0)
        {
            target = platform1Coaches[coachDropdown.value].position;
        }
        else
        {
            target = stationLocations[destinationDropdown.value].position;
        }

        Agent.isStopped = false;
        Agent.SetDestination(target);
        if (uiPanel != null) uiPanel.SetActive(false);
        if (pathLine != null) pathLine.enabled = true;
    }

    public void TeleportToStartLocation()
    {
        if (startDropdown == null || stationLocations == null) return;
        Vector3 targetPos = stationLocations[startDropdown.value].position;
        transform.position = targetPos;
        if (Agent != null && Agent.isOnNavMesh) Agent.Warp(targetPos);
        if (pathLine != null) pathLine.enabled = false;
    }

    void Update() 
    { 
        bool isFreeRoam = movementToggle != null && movementToggle.isOn;
        if (joystick != null) joystick.gameObject.SetActive(isFreeRoam);
        if (isFreeRoam) HandleManualOverride();
        else if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            if (Agent.hasPath) DrawPathLine();
            CheckArrival();
        }
        HandleRotation();
        CheckPlatformDistance();
    }

    void HandleManualOverride() 
    { 
        if (Agent == null || joystick == null) return;
        Vector3 input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        if (input.magnitude > 0.1f)
        {
            if (Agent.isOnNavMesh) { Agent.isStopped = true; Agent.ResetPath(); }
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

    void HandleRotation() 
    { 
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * manualRotateSpeed * Time.deltaTime;
            Agent.transform.Rotate(Vector3.up * mouseX);
        }
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

    void CheckPlatformDistance() 
    { 
        if (stationManager == null || Agent == null) return;
        if (platform1Trigger != null)
        {
            float dist1 = Vector3.Distance(Agent.transform.position, platform1Trigger.position);
            if (dist1 < detectionRadius && stationManager.playerCurrentPlatform != "PF 1") SetPlatform("PF 1");
        }
        if (platform2Trigger != null)
        {
            float dist2 = Vector3.Distance(Agent.transform.position, platform2Trigger.position);
            if (dist2 < detectionRadius && stationManager.playerCurrentPlatform != "PF 2") SetPlatform("PF 2");
        }
    }

    void SetPlatform(string p) 
    { 
        if (stationManager == null) return;
        stationManager.playerCurrentPlatform = p;
        stationManager.UpdateARDisplay();
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

    private void OnTriggerEnter(Collider other) 
    { 
        if (stationManager == null) return;
        if (other.CompareTag("PF1") || other.name.Contains("PF1")) SetPlatform("PF 1");
        else if (other.CompareTag("PF2") || other.name.Contains("PF2")) SetPlatform("PF 2");
    }
}