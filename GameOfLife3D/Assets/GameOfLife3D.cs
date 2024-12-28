using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOfLife3D : MonoBehaviour
{
    public GameObject cubePrefab;  
    public int gridSizeX = 10;     // Grid size in X direction
    public int gridSizeY = 10;     // Grid size in Y direction
    public int gridSizeZ = 10;     // Grid size in Z direction
    private bool[,,] grid;         // 3D array to store cell states
    public TextMeshProUGUI aliveCountText;    // Reference to UI Text for alive cells
    public TextMeshProUGUI deadCountText;     // Reference to UI Text for dead cells
    public Material aliveCellMaterial;  // Material for living cells
    public float cellSize = 0.9f;       // Size of each cell
    public float cellSpacing = 1.1f;    // Space between cells
    public float transitionSpeed = 5f;  // Speed of appear/disappear animation
    private Dictionary<Vector3, GameObject> cellObjects = new Dictionary<Vector3, GameObject>();
    public ParticleSystem cellTransitionEffect;
    private int cycleCount = 0;

    void Start()
    {
        DebugWindow.Log("Game Starting...");
        grid = new bool[gridSizeX, gridSizeY, gridSizeZ];
    }

    // Randomizes the initial grid state
    public void RandomizeGrid()
    {
        DebugWindow.Clear();  // Clear previous messages
        DebugWindow.Log("=== INITIAL GRID STATE ===");
        for (int x = 0; x < gridSizeX; x++)
        {
            string layerOutput = $"\nLayer {x} (X={x}):\n";
            layerOutput += "   ";
            // Add Z-axis labels
            for (int z = 0; z < gridSizeZ; z++)
            {
                layerOutput += $"Z{z} ";
            }
            layerOutput += "\n";
            
            for (int y = 0; y < gridSizeY; y++)
            {
                layerOutput += $"Y{y} "; // Y-axis label
                for (int z = 0; z < gridSizeZ; z++)
                {
                    grid[x, y, z] = Random.value > 0.5f;
                    layerOutput += grid[x, y, z] ? "■ " : "□ "; // Using squares instead of 1/0
                }
                layerOutput += "\n";
            }
            DebugWindow.Log(layerOutput);
        }
        DebugWindow.Log("=== END OF INITIAL STATE ===\n");
    }

    // Updates the grid state according to the Game of Life rules
    void UpdateGrid()
    {
        bool[,,] newGrid = new bool[gridSizeX, gridSizeY, gridSizeZ];
        List<string> dyingCells = new List<string>();
        List<string> newAliveCells = new List<string>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    int liveNeighbors = CountLiveNeighbors(x, y, z);

                    if (grid[x, y, z])  // Cell is alive
                    {
                        newGrid[x, y, z] = liveNeighbors == 5 || liveNeighbors == 6; // Stay alive
                        if (!newGrid[x, y, z])  // If it's going to die
                        {
                            dyingCells.Add($"Cell at <color=yellow>({x}, {y}, {z})</color> is dying with {liveNeighbors} neighbors");
                        }
                    }
                    else  // Cell is dead
                    {
                        newGrid[x, y, z] = liveNeighbors == 4; // Become alive
                        if (newGrid[x, y, z])  // If it's going to become alive
                        {
                            newAliveCells.Add($"Cell at <color=yellow>({x}, {y}, {z})</color> is becoming alive with {liveNeighbors} neighbors");
                        }
                    }
                }
            }
        }

        // Print the changes with better formatting
        DebugWindow.Log($"\n=== CYCLE {cycleCount} ===");
        
        if (dyingCells.Count > 0)
        {
            DebugWindow.Log($"<color=red>DYING CELLS ({dyingCells.Count}):</color>");
            foreach (string message in dyingCells)
            {
                DebugWindow.Log($"  • {message}");
            }
        }
        else
        {
            DebugWindow.Log("<color=gray>No cells dying this cycle</color>");
        }

        if (newAliveCells.Count > 0)
        {
            DebugWindow.Log($"<color=green>NEW ALIVE CELLS ({newAliveCells.Count}):</color>");
            foreach (string message in newAliveCells)
            {
                DebugWindow.Log($"  • {message}");
            }
        }
        else
        {
            DebugWindow.Log("<color=gray>No new cells born this cycle</color>");
        }

        DebugWindow.Log($"=== END OF CYCLE {cycleCount} ===\n");

        grid = newGrid;
        UpdateGridDisplay();
        UpdateCellCounters();
        
        cycleCount++; // Increment the cycle counter
    }

    int CountLiveNeighbors(int x, int y, int z)
    {
        int liveNeighbors = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue; // Skip the cell itself

                    int nx = x + dx;
                    int ny = y + dy;
                    int nz = z + dz;

                    if (nx >= 0 && nx < gridSizeX && ny >= 0 && ny < gridSizeY && nz >= 0 && nz < gridSizeZ)
                    {
                        if (grid[nx, ny, nz]) liveNeighbors++;
                    }
                }
            }
        }

        return liveNeighbors;
    }

    // Displays the current grid by instantiating cubes for each live cell
    void UpdateGridDisplay()
    {
        // Track cells that should be removed
        HashSet<Vector3> positionsToKeep = new HashSet<Vector3>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 position = new Vector3(x * cellSpacing, y * cellSpacing, z * cellSpacing);
                    
                    if (grid[x, y, z])  // Cell should be alive
                    {
                        positionsToKeep.Add(position);
                        if (!cellObjects.ContainsKey(position))
                        {
                            // Create new cell
                            GameObject cell = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                            cell.transform.localScale = Vector3.zero;
                            cellObjects.Add(position, cell);
                            
                            // Animate cell appearance
                            StartCoroutine(AnimateCellScale(cell, Vector3.one * cellSize));
                        }
                    }
                }
            }
        }

        // Remove dead cells
        List<Vector3> positionsToRemove = new List<Vector3>();
        foreach (var kvp in cellObjects)
        {
            if (!positionsToKeep.Contains(kvp.Key))
            {
                StartCoroutine(AnimateCellScale(kvp.Value, Vector3.zero, true));
                positionsToRemove.Add(kvp.Key);
            }
        }

        foreach (var pos in positionsToRemove)
        {
            cellObjects.Remove(pos);
        }
    }

    IEnumerator AnimateCellScale(GameObject cell, Vector3 targetScale, bool destroy = false)
    {
        Vector3 startScale = cell.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            cell.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime);
            yield return null;
        }

        if (destroy)
        {
            ParticleSystem effect = Instantiate(cellTransitionEffect, cell.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
            Destroy(cell);
        }
    }

    // Add this method to count and update the UI
    void UpdateCellCounters()
    {
        int aliveCount = 0;
        int totalCells = gridSizeX * gridSizeY * gridSizeZ;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    if (grid[x, y, z]) aliveCount++;
                }
            }
        }
        
        int deadCount = totalCells - aliveCount;
        
        // Update UI texts
        if (aliveCountText != null)
            aliveCountText.text = $"Alive Cells: {aliveCount}";
        if (deadCountText != null)
            deadCountText.text = $"Dead Cells: {deadCount}";
    }

    void OnGUI()
    {
        if (!DebugWindow.IsVisible())  // Add this method to DebugWindow class
        {
            // Draw a small button in the corner to show debug is available
            if (GUI.Button(new Rect(Screen.width - 100, 10, 90, 20), "Show Debug"))
            {
                DebugWindow.Show();  // Add this method to DebugWindow class
            }
        }
    }

    public void SetCellState(int x, int y, int z, bool state)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ)
        {
            grid[x, y, z] = state;
        }
    }

    public void ClearGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    grid[x, y, z] = false;
                }
            }
        }
        UpdateGridDisplay();
    }

    // Add method to start simulation
    public void StartSimulation()
    {
        InvokeRepeating("UpdateGrid", 0f, 1f);
    }

    // Add method to stop simulation
    public void StopSimulation()
    {
        CancelInvoke("UpdateGrid");
    }

    // Add method to manually set up a pattern
    public void SetupPattern(Vector3Int[] aliveCells)
    {
        ClearGrid();
        foreach (Vector3Int cell in aliveCells)
        {
            SetCellState(cell.x, cell.y, cell.z, true);
        }
        UpdateGridDisplay();
        DebugWindow.Log("Manual pattern setup complete");
    }

    public void LoadPatternFromFile(string filePath)
    {
        try
        {
            ClearGrid();
            string[] lines = System.IO.File.ReadAllLines(filePath);
            int validCells = 0;
            
            DebugWindow.Log($"Loading pattern from: {System.IO.Path.GetFileName(filePath)}");
            
            foreach (string line in lines)
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                string[] coords = line.Split(',');
                if (coords.Length == 3)
                {
                    if (int.TryParse(coords[0], out int x) &&
                        int.TryParse(coords[1], out int y) &&
                        int.TryParse(coords[2], out int z))
                    {
                        if (x >= 0 && x < gridSizeX &&
                            y >= 0 && y < gridSizeY &&
                            z >= 0 && z < gridSizeZ)
                        {
                            SetCellState(x, y, z, true);
                            validCells++;
                        }
                        else
                        {
                            DebugWindow.Log($"Warning: Coordinates out of range: {line}");
                        }
                    }
                    else
                    {
                        DebugWindow.Log($"Warning: Invalid coordinate format: {line}");
                    }
                }
            }
            
            UpdateGridDisplay();
            DebugWindow.Log($"Pattern loaded successfully: {validCells} cells placed");
        }
        catch (System.Exception e)
        {
            DebugWindow.Log($"Error loading pattern: {e.Message}");
            throw;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) // Left or right click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 localPos = transform.InverseTransformPoint(hit.point);
                int x = Mathf.FloorToInt(localPos.x / cellSpacing);
                int y = Mathf.FloorToInt(localPos.y / cellSpacing);
                int z = Mathf.FloorToInt(localPos.z / cellSpacing);
                
                if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ)
                {
                    SetCellState(x, y, z, Input.GetMouseButtonDown(0));
                    UpdateGridDisplay();
                }
            }
        }
    }

    public void EnterCoordinatesMode()
    {
        Debug.Log("Enter coordinates in format 'x,y,z' or 'done' to finish");
        // Implementation would need UI input field
    }

    public void SetInitialState(bool[,,] initialState)
    {
        if (initialState.GetLength(0) == gridSizeX && 
            initialState.GetLength(1) == gridSizeY && 
            initialState.GetLength(2) == gridSizeZ)
        {
            grid = initialState;
            UpdateGridDisplay();
        }
    }
}
