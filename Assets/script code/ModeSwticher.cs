using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ModeSwitcher : MonoBehaviour 
{
    public GameController gameController; // Drag the object with GameController script here
    public GameObject simulationGroup; 
    public GameObject arGroup;         
    public ARSession arSession;      
    
    public Camera simCamera; // Drag your 3D Simulation Camera here
    public Camera arCamera;  // Drag the Camera inside XR Origin here

    public void SwitchToAR() 
    {
        Screen.orientation = ScreenOrientation.Portrait;
        
        simulationGroup.SetActive(false);
        arGroup.SetActive(true);
        arSession.enabled = true; 

        // Update the GameController to use the AR Camera
        if (gameController != null) gameController.mainCam = arCamera;
    }

    public void SwitchToSim() 
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        arGroup.SetActive(false);
        simulationGroup.SetActive(true);
        arSession.enabled = false; 

        // Update the GameController to use the Simulation Camera
        if (gameController != null) gameController.mainCam = simCamera;
    }
}