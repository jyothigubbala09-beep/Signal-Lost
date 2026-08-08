using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CablePuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject puzzlePanel;
    public Transform gridParent;
    public GameObject tilePrefab; 
    public Text towerNameText;
    public Button closeButton;

    [Header("Colors")]
    public Color poweredColor = new Color(0.2f, 0.8f, 0.2f, 1.0f);   // Glowing Green
    public Color unpoweredColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); // Muted Gray

    [Header("Sprites")]
    public Sprite straightSprite;
    public Sprite cornerSprite;
    public Sprite tJunctionSprite;
    public Sprite crossSprite;
    public Sprite endPointSprite;

    private Tower activeTower;
    private CableTile[,] grid;
    private int gridWidth = 5;
    private int gridHeight = 5;

    // Power source position
    private int sourceX = 0;
    private int sourceY = 2;

    // Target emitter position
    private int targetX = 4;
    private int targetY = 2;

    void Start()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePuzzle);
        }
    }

    public void SetGridSize(int w, int h)
    {
        gridWidth = w;
        gridHeight = h;
        sourceX = 0;
        sourceY = h / 2;
        targetX = w - 1;
        targetY = h / 2;

        // Dynamically adjust grid layout spacing if gridParent contains a GridLayoutGroup
        var gridLayout = gridParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = w;
            
            // Adjust cell size dynamically to fit within panel area (assuming 300x300 canvas grid space)
            float totalWidth = 300f;
            float totalHeight = 300f;
            float cellW = (totalWidth - (gridLayout.spacing.x * (w - 1))) / w;
            float cellH = (totalHeight - (gridLayout.spacing.y * (h - 1))) / h;
            float cellSize = Mathf.Min(cellW, cellH);
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
    }

    public void StartPuzzle(Tower tower)
    {
        activeTower = tower;
        if (towerNameText != null)
        {
            towerNameText.text = "REPAIR: " + tower.towerName.ToUpper();
        }

        // Disable player movement
        var player = FindObjectOfType<DroneController>();
        if (player != null) player.enabled = false;

        // Open puzzle panel
        puzzlePanel.SetActive(true);

        // Generate grid
        GeneratePuzzleGrid();
        RecalculatePowerFlow();
    }

    void GeneratePuzzleGrid()
    {
        // Clear old children
        if (gridParent != null)
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }
        }

        grid = new CableTile[gridWidth, gridHeight];

        // Seed layouts based on tower name to provide variety
        int seed = activeTower != null ? activeTower.towerName.GetHashCode() : 12345;
        Random.InitState(seed);

        // Layout template fallback for standard 5x5
        CableType[,] fixed5x5 = new CableType[5, 5] {
            { CableType.Corner,    CableType.Straight,  CableType.TJunction, CableType.Straight,  CableType.Corner },
            { CableType.Straight,  CableType.Corner,    CableType.Straight,  CableType.Corner,    CableType.Straight },
            { CableType.TJunction, CableType.Cross,     CableType.TJunction, CableType.Cross,     CableType.TJunction },
            { CableType.Straight,  CableType.Corner,    CableType.Straight,  CableType.Corner,    CableType.Straight },
            { CableType.Corner,    CableType.Straight,  CableType.TJunction, CableType.Straight,  CableType.Corner }
        };

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (tilePrefab == null) continue;

                GameObject tileObj = Instantiate(tilePrefab, gridParent);
                CableTile tileScript = tileObj.GetComponent<CableTile>();
                if (tileScript == null)
                {
                    tileScript = tileObj.AddComponent<CableTile>();
                }

                Button btn = tileObj.GetComponent<Button>();
                if (btn == null)
                {
                    btn = tileObj.AddComponent<Button>();
                }
                btn.onClick.RemoveAllListeners();

                Image img = tileObj.GetComponent<Image>();
                if (img == null)
                {
                    img = tileObj.AddComponent<Image>();
                }

                // Choose type: use fixed layout if 5x5, otherwise generate procedurally
                CableType currentType = CableType.Straight;
                if (gridWidth == 5 && gridHeight == 5)
                {
                    currentType = fixed5x5[y, x];
                }
                else
                {
                    int rand = Random.Range(0, 100);
                    if (rand < 25) currentType = CableType.Straight;
                    else if (rand < 55) currentType = CableType.Corner;
                    else if (rand < 75) currentType = CableType.TJunction;
                    else if (rand < 90) currentType = CableType.Cross;
                    else currentType = CableType.EndPoint;

                    // Ensure key path elements have connections
                    if (x == sourceX && y == sourceY) currentType = CableType.TJunction;
                    if (x == targetX && y == targetY) currentType = CableType.TJunction;
                }

                switch (currentType)
                {
                    case CableType.Straight:  img.sprite = straightSprite; break;
                    case CableType.Corner:    img.sprite = cornerSprite; break;
                    case CableType.TJunction: img.sprite = tJunctionSprite; break;
                    case CableType.Cross:     img.sprite = crossSprite; break;
                    case CableType.EndPoint:  img.sprite = endPointSprite; break;
                }

                // Random initial rotation (0 to 3)
                int startRotation = Random.Range(0, 4);

                tileScript.Init(currentType, x, y, startRotation, this);

                btn.onClick.AddListener(() => tileScript.RotateTile());

                grid[x, y] = tileScript;
            }
        }
    }

    public void OnTileClicked()
    {
        RecalculatePowerFlow();
        CheckVictoryCondition();

        // Increment moves counter in GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.IncrementMoves();
        }
    }

    void RecalculatePowerFlow()
    {
        if (grid == null) return;

        // Reset all powered states
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].isPowered = false;
                }
            }
        }

        Queue<CableTile> queue = new Queue<CableTile>();

        // Check if source start tile is connected to the power grid on the left
        CableTile startTile = grid[sourceX, sourceY];
        if (startTile == null) return;

        bool[] startConns = startTile.GetCurrentConnections();

        // Must connect LEFT (index 3) to draw power from source indicator
        if (startConns[CableConnection.LEFT])
        {
            startTile.isPowered = true;
            queue.Enqueue(startTile);
        }

        HashSet<CableTile> visited = new HashSet<CableTile>();
        if (queue.Count > 0)
        {
            visited.Add(startTile);
        }

        // BFS path trace
        while (queue.Count > 0)
        {
            CableTile current = queue.Dequeue();
            bool[] currentConns = current.GetCurrentConnections();

            // Clockwise: 0 = Up, 1 = Right, 2 = Down, 3 = Left
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (!currentConns[i]) continue;

                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    CableTile neighbor = grid[nx, ny];
                    if (neighbor == null || visited.Contains(neighbor)) continue;

                    int oppDir = CableConnection.GetOpposite(i);
                    bool[] neighborConns = neighbor.GetCurrentConnections();

                    // Complete path check
                    if (neighborConns[oppDir])
                    {
                        neighbor.isPowered = true;
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Update visuals for all tiles
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].UpdateVisuals();
                }
            }
        }
    }

    void CheckVictoryCondition()
    {
        CableTile targetTile = grid[targetX, targetY];
        if (targetTile != null && targetTile.isPowered)
        {
            bool[] targetConns = targetTile.GetCurrentConnections();
            // Target tile must connect RIGHT (index 1) to bridge to receiver output
            if (targetConns[CableConnection.RIGHT])
            {
                Invoke("CompletePuzzle", 0.5f);
            }
        }
    }

    void CompletePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Resume player movement
        var player = FindObjectOfType<DroneController>();
        if (player != null) player.enabled = true;

        if (activeTower != null)
        {
            activeTower.ActivateTower();
        }
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Resume player movement
        var player = FindObjectOfType<DroneController>();
        if (player != null) player.enabled = true;
    }
}
