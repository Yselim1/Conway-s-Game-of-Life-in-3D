using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOfLifeUI : MonoBehaviour
{
    public GameOfLife3D gameOfLife;
    public Button startButton;
    public Button stopButton;
    public Button clearButton;
    public Button randomButton;
    public Button oscillatorButton;
    public Button loadFileButton;
    
    // Add references for button texts
    public TextMeshProUGUI startButtonText;
    public TextMeshProUGUI stopButtonText;
    public TextMeshProUGUI clearButtonText;
    public TextMeshProUGUI randomButtonText;
    public TextMeshProUGUI oscillatorButtonText;
    public TextMeshProUGUI loadFileButtonText;

    // Pattern definitions
    private static readonly Vector3Int[] OSCILLATOR_PATTERN = new Vector3Int[]
    {
        new Vector3Int(0, 0, 7),
        new Vector3Int(0, 0, 8),
        new Vector3Int(0, 1, 7),
        new Vector3Int(1, 0, 7)
    };

    void Start()
    {
        SetupButtons();
        SetupButtonListeners();
    }

    void SetupButtons()
    {
        // Set button texts
        startButtonText.text = "Start";
        stopButtonText.text = "Stop";
        clearButtonText.text = "Clear";
        randomButtonText.text = "Random";
        oscillatorButtonText.text = "Load Oscillator";
        loadFileButtonText.text = "Load File";


        // Set initial button states
        stopButton.interactable = false;
    }

   

    void SetupButtonListeners()
    {
        startButton.onClick.AddListener(OnStartClick);
        stopButton.onClick.AddListener(OnStopClick);
        clearButton.onClick.AddListener(OnClearClick);
        randomButton.onClick.AddListener(OnRandomClick);
        oscillatorButton.onClick.AddListener(OnOscillatorClick);
        loadFileButton.onClick.AddListener(OnLoadFileClick);
    }

    void OnStartClick()
    {
        gameOfLife.StartSimulation();
        startButton.interactable = false;
        stopButton.interactable = true;
        randomButton.interactable = false;
        oscillatorButton.interactable = false;
    }

    void OnStopClick()
    {
        gameOfLife.StopSimulation();
        startButton.interactable = true;
        stopButton.interactable = false;
        randomButton.interactable = true;
        oscillatorButton.interactable = true;
    }

    void OnClearClick()
    {
        gameOfLife.ClearGrid();
        startButton.interactable = true;
        stopButton.interactable = false;
        randomButton.interactable = true;
        oscillatorButton.interactable = true;
    }

    void OnRandomClick()
    {
        gameOfLife.RandomizeGrid();
    }

    void OnOscillatorClick()
    {
        gameOfLife.SetupPattern(OSCILLATOR_PATTERN);
    }

    void OnLoadFileClick()
    {
        try
        {
            // Open file dialog in Assets/Patterns folder
            string path = UnityEditor.EditorUtility.OpenFilePanel(
                "Select Pattern File",
                Application.dataPath + "/Patterns",
                "txt"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                if (System.IO.File.Exists(path))
                {
                    gameOfLife.LoadPatternFromFile(path);
                    // Disable appropriate buttons
                    startButton.interactable = true;
                    stopButton.interactable = false;
                    randomButton.interactable = true;
                    oscillatorButton.interactable = true;
                }
                else
                {
                    Debug.LogError("File not found: " + path);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading pattern file: " + e.Message);
        }
    }
} 