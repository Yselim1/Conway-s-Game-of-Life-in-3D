using UnityEngine;
using System.Collections.Generic;

public class DebugWindow : MonoBehaviour
{
    private static List<string> logMessages = new List<string>();
    private static int maxMessages = 10000;
    private Vector2 scrollPosition;
    private bool showWindow = false;
    private Rect windowRect = new Rect(Screen.width - 420, 20, 400, 300);
    private static DebugWindow instance;
    private GUIStyle messageStyle;
    private GUIStyle headerStyle;
    private GUIStyle windowStyle;
    private bool stylesInitialized = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        // Message style
        messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.richText = true;
        messageStyle.wordWrap = true;
        messageStyle.fontSize = 14;
        messageStyle.normal.textColor = Color.white;

        // Header style
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;

        // Window style
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 14;
        windowStyle.fontStyle = FontStyle.Bold;

        stylesInitialized = true;
    }

    public static void Log(string message)
    {
        if (instance == null)
        {
            Debug.LogWarning("DebugWindow instance not found!");
            return;
        }

        if (logMessages.Count > maxMessages)
        {
            logMessages.RemoveRange(0, 1000);
        }
        
        string timestamp = $"[{System.DateTime.Now.ToString("HH:mm:ss")}] ";
        logMessages.Add(timestamp + message);
    }

    public static void Clear()
    {
        logMessages.Clear();
    }

    void OnGUI()
    {
        if (!stylesInitialized)
        {
            InitializeStyles();
        }

        if (showWindow)
        {
            windowRect = GUILayout.Window(0, windowRect, DrawWindow, "Game of Life Debug Console", windowStyle);
        }
    }

    void DrawWindow(int windowID)
    {
        GUI.backgroundColor = new Color(0, 0, 0, 0.6f);
        
        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

        // Buttons row with message count
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(25)))
        {
            Clear();
        }
        if (GUILayout.Button("Copy", GUILayout.Width(60), GUILayout.Height(25)))
        {
            GUIUtility.systemCopyBuffer = string.Join("\n", logMessages);
        }
        
        // Show message count
        GUILayout.Label($"Messages: {logMessages.Count}", GUILayout.Height(25));
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("_", GUILayout.Width(25), GUILayout.Height(25)))
        {
            windowRect.height = 50;
        }
        if (GUILayout.Button("□", GUILayout.Width(25), GUILayout.Height(25)))
        {
            windowRect.height = 300;
        }
        if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
        {
            showWindow = false;
        }
        GUILayout.EndHorizontal();

        // Log messages area
        if (windowRect.height > 50)  // Only show messages if not minimized
        {
            GUI.backgroundColor = new Color(0, 0, 0, 0.4f);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box);
            foreach (string message in logMessages)
            {
                GUILayout.Label(message, messageStyle);
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();

        // Make window draggable
        GUI.DragWindow();
    }

    void Update()
    {
        // Toggle console with ~ key
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            showWindow = !showWindow;
        }
    }

    void OnEnable()
    {
        // Position window on the right side
        windowRect.x = Screen.width - 420;
    }

    public static bool IsVisible()
    {
        return instance != null && instance.showWindow;
    }

    public static void Show()
    {
        if (instance != null)
        {
            instance.showWindow = true;
        }
    }
} 