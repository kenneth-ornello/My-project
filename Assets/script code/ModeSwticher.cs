using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ModeSwitcher : MonoBehaviour {
    public GameObject simGroup; // Drag Simulation_Group here
    public GameObject arGroup;  // Drag AR_Group here
    public ARSession arSession; // Drag AR Session here

    public void SwitchToAR() {
        simGroup.SetActive(false);
        arGroup.SetActive(true);
        arSession.enabled = true; // Turns on phone camera
    }

    public void SwitchToSim() {
        arGroup.SetActive(false);
        simGroup.SetActive(true);
        arSession.enabled = false; // Turns off phone camera to save battery
    }
}