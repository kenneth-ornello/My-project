using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using TMPro;

public class TrainStationManager : MonoBehaviour
{
    public string pythonInterpreterPath = "python";
    public string scriptName = "extract.py";
    public TextMeshProUGUI arDisplayLabel;

    [Tooltip("Type PF 1 or PF 2 here to test without walking")]
    public string playerCurrentPlatform = "";

    private string csvPath;
    private List<string> filteredTrains = new List<string>();
    private int currentTrainIndex = 0;

    void Start()
    {
        csvPath = Path.Combine(Application.dataPath, "data/Vidyavihar_Arrivals_2026.csv");

        // FIX: Check if a platform is already set in the Inspector
        if (!string.IsNullOrEmpty(playerCurrentPlatform))
        {
            // If PF 1 or PF 2 is already typed, load it immediately
            UpdateARDisplay();
        }
        else
        {
            // Otherwise, show the default walk prompt
            if (arDisplayLabel != null)
                arDisplayLabel.text = "Walk to a platform to see timings";
        }

        InvokeRepeating("RunPythonExtractor", 0f, 600f);
    }

    void RunPythonExtractor()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonInterpreterPath;
        start.Arguments = Path.Combine(Application.dataPath, scriptName);
        start.UseShellExecute = false;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            if (process != null)
            {
                process.WaitForExit();
                UpdateARDisplay();
            }
        }
    }

    public void UpdateARDisplay()
    {
        if (!File.Exists(csvPath)) return;

        // Strip hidden BOM characters
        string content = File.ReadAllText(csvPath).Replace("\uFEFF", "");
        string[] lines = content.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        filteredTrains.Clear();

        // Standardize the platform name for comparison
        string cleanPlayerPlatform = playerCurrentPlatform.Replace(" ", "").ToUpper();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');

            if (columns.Length > 2)
            {
                string cleanCSVPlatform = columns[2].Replace(" ", "").ToUpper();

                if (cleanCSVPlatform == cleanPlayerPlatform)
                {
                    filteredTrains.Add(columns[1] + " - " + columns[3]);
                }
            }
        }

        currentTrainIndex = 0;
        ShowCurrentTrain();
    }

    void ShowCurrentTrain()
    {
        if (string.IsNullOrEmpty(playerCurrentPlatform))
        {
            arDisplayLabel.text = "Walk to a platform to see timings";
            return;
        }

        if (filteredTrains.Count > 0)
        {
            string counter = "(" + (currentTrainIndex + 1) + "/" + filteredTrains.Count + ")";
            arDisplayLabel.text = "<b>Timings for " + playerCurrentPlatform + "</b>\n" +
                                 filteredTrains[currentTrainIndex] + "\n" +
                                 "<size=80%>" + counter + "</size>";
        }
        else
        {
            arDisplayLabel.text = "No trains found for " + playerCurrentPlatform;
        }
    }

    public void NextTrain()
    {
        if (filteredTrains.Count == 0) return;
        currentTrainIndex = (currentTrainIndex + 1) % filteredTrains.Count;
        ShowCurrentTrain();
    }

    public void PreviousTrain()
    {
        if (filteredTrains.Count == 0) return;
        currentTrainIndex--;
        if (currentTrainIndex < 0) currentTrainIndex = filteredTrains.Count - 1;
        ShowCurrentTrain();
    }
}