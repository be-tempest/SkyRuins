using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // ------------- 定数 / フィールド -------------
    public static BoardManager Instance { get; private set; }

    public GameObject blockPrefab;
    public GameObject obstaclePrefab;
    public GameObject playerPrefab;
    public GameObject[] itemPrefabs = new GameObject[3]; // Inspectorに3つセット

    public int coreSize = 7;
    private int fullSize = 9;

    public GameObject[,] grid;
    private GameObject[,] occupants;
    public int[,] gridData;

    private const int PLAYER = 1;
    private const int OBSTACLE = 2;
    private const int ITEM_A = 3;
    private const int ITEM_B = 4;
    private const int ITEM_C = 5;

    public enum GamePhase { Add, PlayerMove, Slide, Remove }
    public GamePhase phase = GamePhase.Add;

    public enum Direction { Left, Up, Right, Down }

    private int level = 1;
    private Dictionary<Direction, int> addCount = new Dictionary<Direction, int>()
    {
        { Direction.Left, 1 },
        { Direction.Up, 0 },
        { Direction.Right, 0 },
        { Direction.Down, 0 }
    };

    private List<(int x, int y)> insertedBlocks = new List<(int, int)>();

    private int playerX;
    private int playerY;
    private GameObject playerObj;

    private int lastPlayerX;
    private int lastPlayerY;
    private bool attackPerformedThisSlide = false;

    private bool isGameOver = false;

    [SerializeField] private float slideDuration = 0.28f;

    [Header("Spawn Probabilities")]
    [Range(0f,1f)] public float itemProbEach = 0.05f;
    [Range(0f,1f)] public float obstacleBaseProb = 0.20f;
    [Range(0f,1f)] public float obstacleIncreasePerLevel = 0.02f;

    private int[] itemsCount = new int[3] {0,0,0};

    // --- アイテム使用のための一時状態（ターゲット選択など）
    // pendingItem: -1 = none, 0=bomb,1=barrier,2=jump
    private int pendingItem = -1;
    // pending modes: 0 none, 1 = bomb-direction selection, 2 = jump-direction selection
    private int pendingMode = 0;

    // バリア状態（次回の攻撃を一度だけ無効化）
    private bool shieldActive = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        grid = new GameObject[fullSize, fullSize];
        occupants = new GameObject[fullSize, fullSize];
        gridData = new int[fullSize, fullSize];

        InitBoard();

        int center = coreSize / 2 + 1;
        playerX = center;
        playerY = center;
        gridData[playerX, playerY] = PLAYER;
        PlaceObjectOnBlock(playerX, playerY);
        playerObj = occupants[playerX, playerY];

        lastPlayerX = playerX;
        lastPlayerY = playerY;
    }

    void Update()
    {
        if (isGameOver) return;

        // Add phase
        if (Input.GetKeyDown(KeyCode.A) && phase == GamePhase.Add)
        {
            AddBlocks();
            phase = GamePhase.PlayerMove;
            lastPlayerX = playerX;
            lastPlayerY = playerY;
            attackPerformedThisSlide = false;
            return;
        }

        // PlayerMove phase: handle pending item target selection first
        if (phase == GamePhase.PlayerMove)
        {
            if (pendingMode != 0)
            {
                // awaiting direction for pending item (bomb single or jump)
                if (HandlePendingDirectionInput()) return; // consumed input
            }
            else
            {
                // item keys
                if (Input.GetKeyDown(KeyCode.Alpha1)) { StartUseItem(0); return; }
                if (Input.GetKeyDown(KeyCode.Alpha2)) { StartUseItem(1); return; }
                if (Input.GetKeyDown(KeyCode.Alpha3)) { StartUseItem(2); return; }

                HandlePlayerInput();
                return;
            }
        }

        // Slide phase: perform attack once before slide
        if (phase == GamePhase.Slide && !attackPerformedThisSlide)
        {
            PerformAttackAtLastPosition();
            attackPerformedThisSlide = true;
            if (isGameOver) return;
        }

        // S to start slide
        if (Input.GetKeyDown(KeyCode.S) && phase == GamePhase.Slide)
        {
            StartCoroutine(SlideBlocksCoroutine(slideDuration));
            return;
        }

        // D to remove
        if (Input.GetKeyDown(KeyCode.D) && phase == GamePhase.Remove)
        {
            RemoveBlocks();
            LevelUp();
            phase = GamePhase.Add;
            return;
        }
    }

    // ---------------- init ----------------
    void InitBoard()
    {
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                SpawnBlock(x,y);
                gridData[x,y] = GenerateCellValueByProbability();
                PlaceObjectOnBlock(x,y);
            }
        }
    }

    void SpawnBlock(int x,int y)
    {
        Vector3 pos = new Vector3(x,0,y);
        grid[x,y] = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
    }

    // ---------------- player input ----------------
    void HandlePlayerInput()
    {
        int newX = playerX;
        int newY = playerY;
        bool acted = false;

        if (Input.GetKeyDown(KeyCode.UpArrow)) { newY++; acted = true; }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { newY--; acted = true; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { newX--; acted = true; }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { newX++; acted = true; }
        else if (Input.GetKeyDown(KeyCode.Return)) { acted = true; } // stay

        if (!acted) return;

        if (newX < 1 || newY < 1 || newX > coreSize || newY > coreSize) return;

        int cell = gridData[newX, newY];
        if (cell == OBSTACLE) return;

        if ((cell == ITEM_A || cell == ITEM_B || cell == ITEM_C) && !(newX == playerX && newY == playerY))
        {
            int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
            itemsCount[idx]++;
            Debug.Log($"Picked up item {idx+1}. Now have {itemsCount[idx]}");
            if (occupants[newX,newY] != null) { Destroy(occupants[newX,newY]); occupants[newX,newY] = null; }
            gridData[newX,newY] = 0;
        }

        if (!(newX == playerX && newY == playerY))
        {
            MovePlayerTo(playerX, playerY, newX, newY, playerObj);
            playerX = newX;
            playerY = newY;
        }

        phase = GamePhase.Slide;
    }

    // ---------------- add / level ----------------
    void LevelUp()
    {
        level++;
        Debug.Log("Level Up! " + level);
        Direction dirToIncrease = (Direction)((level - 1) % 4);
        addCount[dirToIncrease] = Mathf.Min(3, addCount[dirToIncrease] + 1);
    }

    void AddBlocks()
    {
        insertedBlocks.Clear();
        HashSet<int> usedRows = new HashSet<int>();
        HashSet<int> usedCols = new HashSet<int>();

        foreach (var dir in new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down })
        {
            int count = addCount[dir];
            if (count == 0) continue;

            List<int> candidates = new List<int>();
            for (int i = 1; i <= coreSize; i++) candidates.Add(i);
            Shuffle(candidates);

            int added = 0;
            foreach (var idx in candidates)
            {
                if (dir == Direction.Left || dir == Direction.Right)
                {
                    if (usedRows.Contains(idx)) continue;
                    usedRows.Add(idx);
                }
                else
                {
                    if (usedCols.Contains(idx)) continue;
                    usedCols.Add(idx);
                }

                AddBlockFromDirection(dir, idx);
                added++;
                if (added >= count) break;
            }
        }
    }

    void AddBlockFromDirection(Direction dir, int idx)
    {
        int x = 0, y = 0;
        switch(dir)
        {
            case Direction.Left: x = 0; y = idx; break;
            case Direction.Right: x = coreSize + 1; y = idx; break;
            case Direction.Up: x = idx; y = coreSize + 1; break;
            case Direction.Down: x = idx; y = 0; break;
        }

        SpawnBlock(x,y);
        gridData[x,y] = GenerateCellValueByProbability();
        PlaceObjectOnBlock(x,y);
        insertedBlocks.Add((x,y));
        Debug.Log($"Added {dir} block at ({x},{y}) value={gridData[x,y]}");
    }

    // ---------------- slide (grouped same-edge simultaneous) ----------------
    private IEnumerator RunAndFlag(IEnumerator routine, List<bool> doneFlags, int index)
    {
        if (isGameOver) { doneFlags[index] = true; yield break; }
        yield return StartCoroutine(routine);
        doneFlags[index] = true;
    }

    private IEnumerator SlideBlocksCoroutine(float duration)
    {
        var groups = new Dictionary<Direction, List<(int x,int y)>>()
        {
            { Direction.Left, new List<(int,int)>() },
            { Direction.Up, new List<(int,int)>() },
            { Direction.Right, new List<(int,int)>() },
            { Direction.Down, new List<(int,int)>() }
        };

        foreach (var (x,y) in insertedBlocks)
        {
            if (x == 0) groups[Direction.Left].Add((x,y));
            else if (x == coreSize + 1) groups[Direction.Right].Add((x,y));
            else if (y == 0) groups[Direction.Down].Add((x,y));
            else if (y == coreSize + 1) groups[Direction.Up].Add((x,y));
        }

        Direction[] order = new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

        foreach (var dir in order)
        {
            if (isGameOver) yield break;
            var list = groups[dir];
            if (list == null || list.Count == 0) continue;

            var doneFlags = new List<bool>(new bool[list.Count]);

            for (int i = 0; i < list.Count; i++)
            {
                if (isGameOver) { doneFlags[i] = true; continue; }
                int localIndex = i;
                var (x,y) = list[localIndex];
                IEnumerator routine;
                if (dir == Direction.Left || dir == Direction.Right)
                {
                    int row = y;
                    routine = SlideRowAnimated(row, dir, duration);
                }
                else
                {
                    int col = x;
                    routine = SlideColumnAnimated(col, dir, duration);
                }
                StartCoroutine(RunAndFlag(routine, doneFlags, localIndex));
            }

            while (true)
            {
                if (isGameOver) break;
                bool allDone = true;
                for (int k = 0; k < doneFlags.Count; k++) if (!doneFlags[k]) { allDone = false; break; }
                if (allDone) break;
                yield return null;
            }

            yield return new WaitForSeconds(0.03f);
        }

        phase = GamePhase.Remove;
        yield break;
    }

    private IEnumerator MoveTransformOverTime(Transform t, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (isGameOver) yield break;
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(from, to, t01);
            yield return null;
        }
        t.position = to;
    }

    private IEnumerator SlideRowAnimated(int row, Direction dir, float duration)
    {
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Left)
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x,row] != null)
                {
                    Vector3 from = grid[x,row].transform.position;
                    Vector3 to = new Vector3(x+1,0,row);
                    movers.Add((grid[x,row], from, to));
                }
            }
        }
        else
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x,row] != null)
                {
                    Vector3 from = grid[x,row].transform.position;
                    Vector3 to = new Vector3(x-1,0,row);
                    movers.Add((grid[x,row], from, to));
                }
            }
        }

        var routines = new List<Coroutine>();
        foreach (var m in movers)
        {
            if (isGameOver) yield break;
            routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        }
        foreach (var c in routines) yield return c;

        if (dir == Direction.Left)
        {
            for (int x = fullSize - 1; x >= 1; x--)
            {
                grid[x,row] = grid[x-1,row];
                occupants[x,row] = occupants[x-1,row];
                gridData[x,row] = gridData[x-1,row];
                if (gridData[x,row] == PLAYER) { playerX = x; playerY = row; }
            }
            grid[0,row]=null; occupants[0,row]=null; gridData[0,row]=0;
        }
        else
        {
            for (int x = 0; x <= fullSize - 2; x++)
            {
                grid[x,row] = grid[x+1,row];
                occupants[x,row] = occupants[x+1,row];
                gridData[x,row] = gridData[x+1,row];
                if (gridData[x,row] == PLAYER) { playerX = x; playerY = row; }
            }
            grid[fullSize-1,row]=null; occupants[fullSize-1,row]=null; gridData[fullSize-1,row]=0;
        }

        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize) playerObj = occupants[playerX,playerY];
        yield break;
    }

    private IEnumerator SlideColumnAnimated(int col, Direction dir, float duration)
    {
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Up)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col,y] != null)
                {
                    Vector3 from = grid[col,y].transform.position;
                    Vector3 to = new Vector3(col,0,y-1);
                    movers.Add((grid[col,y], from, to));
                }
            }
        }
        else
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col,y] != null)
                {
                    Vector3 from = grid[col,y].transform.position;
                    Vector3 to = new Vector3(col,0,y+1);
                    movers.Add((grid[col,y], from, to));
                }
            }
        }

        var routines = new List<Coroutine>();
        foreach (var m in movers)
        {
            if (isGameOver) yield break;
            routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        }
        foreach (var c in routines) yield return c;

        if (dir == Direction.Up)
        {
            for (int y = 1; y <= fullSize - 1; y++)
            {
                grid[col,y-1] = grid[col,y];
                occupants[col,y-1] = occupants[col,y];
                gridData[col,y-1] = gridData[col,y];
                if (gridData[col,y-1] == PLAYER) { playerX = col; playerY = y-1; }
            }
            grid[col,fullSize-1]=null; occupants[col,fullSize-1]=null; gridData[col,fullSize-1]=0;
        }
        else
        {
            for (int y = fullSize - 2; y >= 0; y--)
            {
                grid[col,y+1] = grid[col,y];
                occupants[col,y+1] = occupants[col,y];
                gridData[col,y+1] = gridData[col,y];
                if (gridData[col,y+1] == PLAYER) { playerX = col; playerY = y+1; }
            }
            grid[col,0]=null; occupants[col,0]=null; gridData[col,0]=0;
        }

        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize) playerObj = occupants[playerX,playerY];
        yield break;
    }

    // ---------------- attack ----------------
    void PerformAttackAtLastPosition()
    {
        Debug.Log($"Attack at ({lastPlayerX},{lastPlayerY})");
        // バリアがアクティブなら消費して攻撃を無効化
        if (shieldActive)
        {
            // Only consume shield if attack would hit player
            if (playerX == lastPlayerX && playerY == lastPlayerY)
            {
                shieldActive = false;
                Debug.Log("Shield protected the player from attack!");
                return;
            }
            // If shield active but player not in attacked cell, keep shield (spec: shield protects only if hit)
        }

        if (playerX == lastPlayerX && playerY == lastPlayerY)
        {
            Debug.Log("Player hit by attack! Game Over.");
            GameOver();
        }
    }

    // ---------------- remove ----------------
    void RemoveBlocks()
    {
        foreach (var (x,y) in insertedBlocks)
        {
            if (isGameOver) break;

            if (x == 0) { int tx = fullSize - 1, ty = y; DestroyAt(tx,ty); }
            else if (x == coreSize + 1) { int tx = 0, ty = y; DestroyAt(tx,ty); }
            else if (y == 0) { int tx = x, ty = fullSize - 1; DestroyAt(tx,ty); }
            else if (y == coreSize + 1) { int tx = x, ty = 0; DestroyAt(tx,ty); }
        }
        insertedBlocks.Clear();
    }

    void DestroyAt(int x,int y)
    {
        if (occupants[x,y] != null && occupants[x,y] == playerObj)
        {
            Debug.Log("Player pushed out of board! Game Over.");
            GameOver();
        }
        if (grid[x,y] != null) Destroy(grid[x,y]);
        if (occupants[x,y] != null) Destroy(occupants[x,y]);
        grid[x,y] = null; occupants[x,y] = null; gridData[x,y] = 0;
    }

    void GameOver()
    {
        isGameOver = true;
        phase = GamePhase.Add;
        Debug.Log("=== GAME OVER ===");
    }

    // ---------------- generate / place / consume ----------------
    public void PlaceObjectOnBlock(int x, int y)
    {
        if (grid[x,y] == null) return;
        if (occupants[x,y] != null) { Destroy(occupants[x,y]); occupants[x,y] = null; }

        int cell = gridData[x,y];
        if (cell == 0) return;
        if (cell == OBSTACLE) occupants[x,y] = Instantiate(obstaclePrefab, grid[x,y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x,y].transform);
        else if (cell == PLAYER) occupants[x,y] = Instantiate(playerPrefab, grid[x,y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x,y].transform);
        else if (cell == ITEM_A || cell == ITEM_B || cell == ITEM_C)
        {
            int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
            if (itemPrefabs != null && itemPrefabs.Length > idx && itemPrefabs[idx] != null)
                occupants[x,y] = Instantiate(itemPrefabs[idx], grid[x,y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x,y].transform);
            else Debug.LogWarning($"itemPrefabs[{idx}] not set");
        }
    }

    public void ConsumeItemAt(int x,int y)
    {
        int cell = gridData[x,y];
        if (!(cell == ITEM_A || cell == ITEM_B || cell == ITEM_C)) return;
        int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
        itemsCount[idx]++;
        if (occupants[x,y] != null) { Destroy(occupants[x,y]); occupants[x,y] = null; }
        gridData[x,y] = 0;
    }

    public void MovePlayerTo(int oldX,int oldY,int newX,int newY, GameObject playerGO)
    {
        if (occupants[newX,newY] != null && occupants[newX,newY] != playerGO)
        {
            Destroy(occupants[newX,newY]);
            occupants[newX,newY] = null;
        }
        if (occupants[oldX,oldY] == playerGO) occupants[oldX,oldY] = null;
        gridData[oldX,oldY] = 0;
        gridData[newX,newY] = PLAYER;
        occupants[newX,newY] = playerGO;
        playerGO.transform.SetParent(grid[newX,newY].transform);
        playerGO.transform.localPosition = Vector3.up * 0.5f;
        playerObj = playerGO;
    }

    // ---------------- item use / behaviors ----------------
    // Called when pressing 1/2/3 in PlayerMove phase
    void StartUseItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex > 2) return;
        if (pendingMode != 0) { Debug.Log("Already selecting target for an item."); return; }

        if (itemIndex == 0)
        {
            if (itemsCount[0] >= 5)
            {
                itemsCount[0] -= 5;
                DestroyAllObstacles();
                Debug.Log("Used 5x Bomb: cleared all obstacles.");
                return;
            }
            if (itemsCount[0] >= 3)
            {
                itemsCount[0] -= 3;
                DestroyCrossAroundPlayer();
                Debug.Log("Used 3x Bomb: destroyed cross obstacles.");
                return;
            }
            if (itemsCount[0] >= 1)
            {
                pendingItem = 0;
                pendingMode = 1; // bomb target
                Debug.Log("Bomb: select direction with arrow key to destroy obstacle one tile away.");
                return;
            }
            Debug.Log("No bombs to use.");
        }
        else if (itemIndex == 1)
        {
            if (itemsCount[1] <= 0) { Debug.Log("No shield to use."); return; }
            itemsCount[1]--;
            shieldActive = true;
            Debug.Log("Used Shield: next attack will be negated if it would hit you.");
            return;
        }
        else if (itemIndex == 2)
        {
            if (itemsCount[2] <= 0) { Debug.Log("No jump boots to use."); return; }
            pendingItem = 2;
            pendingMode = 2; // jump target
            Debug.Log("Jump Boots: select direction with arrow key to jump 2 tiles.");
            return;
        }
    }

    // Handles arrow input while pendingMode != 0. Returns true if input consumed.
    bool HandlePendingDirectionInput()
    {
        int dx = 0, dy = 0;
        bool pressed = false;
        if (Input.GetKeyDown(KeyCode.UpArrow)) { dx = 0; dy = 1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { dx = 0; dy = -1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { dx = -1; dy = 0; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { dx = 1; dy = 0; pressed = true; }

        if (!pressed) return false;

        if (pendingMode == 1 && pendingItem == 0)
        {
            int tx = playerX + dx;
            int ty = playerY + dy;
            if (tx < 1 || ty < 1 || tx > coreSize || ty > coreSize)
            {
                Debug.Log("Target out of bounds. Bomb wasted.");
            }
            else
            {
                bool destroyed = DestroyObstacleAt(tx, ty);
                if (destroyed) Debug.Log($"Bomb destroyed obstacle at ({tx},{ty})");
                else Debug.Log($"Bomb targeted ({tx},{ty}) but no obstacle there.");
            }
            itemsCount[0]--; // consume 1
            pendingItem = -1; pendingMode = 0;
            return true;
        }
        else if (pendingMode == 2 && pendingItem == 2)
        {
            int tx = playerX + dx * 2;
            int ty = playerY + dy * 2;
            if (tx < 1 || ty < 1 || tx > coreSize || ty > coreSize)
            {
                Debug.Log("Jump target out of bounds.");
                pendingItem = -1; pendingMode = 0;
                return true;
            }
            if (gridData[tx,ty] == OBSTACLE)
            {
                Debug.Log("Cannot jump: destination occupied by obstacle.");
                pendingItem = -1; pendingMode = 0;
                return true;
            }

            // perform jump: can jump over obstacle in between
            itemsCount[2]--;
            Debug.Log($"Jumped to ({tx},{ty}). Remaining boots: {itemsCount[2]}");

            // if destination has item, pick up
            if (gridData[tx,ty] == ITEM_A || gridData[tx,ty] == ITEM_B || gridData[tx,ty] == ITEM_C)
            {
                ConsumeItemAt(tx,ty);
            }

            MovePlayerTo(playerX, playerY, tx, ty, playerObj);
            playerX = tx; playerY = ty;
            pendingItem = -1; pendingMode = 0;

            // <<< NEW: treat jump as player movement -> proceed to Slide phase >>>
            phase = GamePhase.Slide;

            return true;
        }

        pendingItem = -1; pendingMode = 0;
        return true;
    }

    // Destroy a single obstacle at x,y. Returns true if destroyed.
    bool DestroyObstacleAt(int x,int y)
    {
        if (x < 1 || y < 1 || x > coreSize || y > coreSize) return false;
        if (gridData[x,y] != OBSTACLE) return false;
        if (occupants[x,y] != null) { Destroy(occupants[x,y]); occupants[x,y] = null; }
        gridData[x,y] = 0;
        Debug.Log($"Destroyed obstacle at ({x},{y})");
        return true;
    }

    void DestroyCrossAroundPlayer()
    {
        DestroyObstacleAt(playerX, playerY + 1);
        DestroyObstacleAt(playerX, playerY - 1);
        DestroyObstacleAt(playerX - 1, playerY);
        DestroyObstacleAt(playerX + 1, playerY);
    }

    void DestroyAllObstacles()
    {
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                if (gridData[x,y] == OBSTACLE)
                {
                    if (occupants[x,y] != null) { Destroy(occupants[x,y]); occupants[x,y] = null; }
                    gridData[x,y] = 0;
                }
            }
        }
        Debug.Log("All obstacles cleared.");
    }

    // ---------------- utility / helpers ----------------
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    int GenerateCellValueByProbability()
    {
        float obstacleProb = Mathf.Clamp01(obstacleBaseProb + (level - 1) * obstacleIncreasePerLevel);
        float r = Random.value;
        if (r < itemProbEach) return ITEM_A;
        if (r < itemProbEach * 2f) return ITEM_B;
        if (r < itemProbEach * 3f) return ITEM_C;
        if (r < itemProbEach * 3f + obstacleProb) return OBSTACLE;
        return 0;
    }
}
