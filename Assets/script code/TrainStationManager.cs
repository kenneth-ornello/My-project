using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class TrainStationManager : MonoBehaviour
{
    [Header("Mobile Settings")]
    public string csvFileName = "Vidyavihar_Arrivals_2026.csv";
    public TextMeshProUGUI arDisplayLabel;

    [Header("PC Debugging Only")]
    public string pythonInterpreterPath = "python";
    public string scriptName = "extract.py";

    [Tooltip("Type PF 1 or PF 2 here to test without walking")]
    public string playerCurrentPlatform = "";

    private List<string> filteredTrains = new List<string>();
    private int currentTrainIndex = 0;
    private string csvPath;

    void Start()
    {
        // Use StreamingAssetsPath for mobile compatibility
        csvPath = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!string.IsNullOrEmpty(playerCurrentPlatform))
        {
            UpdateARDisplay();
        }
        else if (arDisplayLabel != null)
        {
            arDisplayLabel.text = "Walk to a platform to see timings";
        }

        // Only run Python if we are in the Unity Editor on your PC
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            InvokeRepeating("RunPythonExtractor", 0f, 600f);
        }
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
        StartCoroutine(LoadCSVOnMobile());
    }

    // NEW: Coroutine to read file correctly on Android/iOS
    IEnumerator LoadCSVOnMobile()
    {
        string result = "";

        // Android requires WebRequest to look inside the APK
        if (csvPath.Contains("://") || csvPath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(csvPath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                result = www.downloadHandler.text;
            }
            else
            {
                arDisplayLabel.text = "Error: CSV not found in StreamingAssets";
                yield break;
            }
        }
        else // Standard PC path
        {
            if (File.Exists(csvPath))
            {
                result = File.ReadAllText(csvPath);
            }
        }

        if (!string.IsNullOrEmpty(result))
        {
            ParseCSVData(result.Replace("\uFEFF", ""));
        }
    }

    void ParseCSVData(string content)
    {
        string[] lines = content.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        filteredTrains.Clear();

        string cleanPlayerPlatform = playerCurrentPlatform.Replace(" ", "").ToUpper();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length > 2)
            {
                string cleanCSVPlatform = columns[2].Replace(" ", "").ToUpper();
                if (cleanCSVPlatform == cleanPlayerPlatform)
                {
                    // Index 1 is Time, Index 3 is Destination/Notes
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