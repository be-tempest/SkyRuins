using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager3 : MonoBehaviour
{
    public static BoardManager3 Instance { get; private set; }

    
    // 参照
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;

    
    [SerializeField] private MonoBehaviour addManagerBehaviour;
    private AddManager addManager;

    
    [SerializeField] private MonoBehaviour slideManagerBehaviour;
    private ISlideManager slideManager; 

    
    [SerializeField] private MonoBehaviour deleteManagerBehaviour;
    private DeleteManager deleteManager;

    
    [SerializeField] private MonoBehaviour itemManagerBehaviour;
    private ItemManager itemManager;

    // 盤面サイズ
    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    // 盤面データ
    private GameObject[,] grid => gridStorage.GridObjects;
    private GameObject[,] occupants => gridStorage.Occupants;
    private int[,] gridData => gridStorage.GridData;

    public int PLAYER = 1;
    public int OBSTACLE = 2;
    public int ITEM_A = 3;
    public int ITEM_B = 4;
    public int ITEM_C = 5;

    //public int CellPlayer => PLAYER;
    //public int CellObstacle => OBSTACLE;
    //public int CellItemA => ITEM_A;
    //public int CellItemB => ITEM_B;
    //public int CellItemC => ITEM_C;

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

    // player state
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

    void Awake()
    {
        Instance = this;

        // addManagerBehaviour を AddManager にキャスト
        if (addManagerBehaviour != null && addManagerBehaviour is AddManager)
        {
            addManager = (AddManager)addManagerBehaviour;
        }
        else
        {
            addManager = null;
            if (addManagerBehaviour == null) Debug.LogWarning("[BoardManager3] addManagerBehaviour not assigned in Inspector.");
            else Debug.LogError("[BoardManager3] addManagerBehaviour is not AddManager. Please assign the AddManager component.");
        }

        // slideManagerBehaviour を ISlideManager にキャスト
        if (slideManagerBehaviour != null && slideManagerBehaviour is ISlideManager)
        {
            slideManager = (ISlideManager)slideManagerBehaviour;
        }
        else
        {
            slideManager = null;
            if (slideManagerBehaviour == null) Debug.LogWarning("[BoardManager3] slideManagerBehaviour not assigned in Inspector.");
            else Debug.LogError("[BoardManager3] slideManagerBehaviour does not implement ISlideManager. Please assign the SlideManager component.");
        }

        // deleteManagerBehaviour を DeleteManager にキャスト
        if (deleteManagerBehaviour != null && deleteManagerBehaviour is DeleteManager)
        {
            deleteManager = (DeleteManager)deleteManagerBehaviour;
        }
        else
        {
            deleteManager = null;
            if (deleteManagerBehaviour == null) Debug.LogWarning("[BoardManager3] deleteManagerBehaviour not assigned in Inspector.");
            else Debug.LogError("[BoardManager3] deleteManagerBehaviour does not implement DeleteManager. Please assign the DeleteManager component.");
        }

        // itemManagerBehaviour を ItemManager にキャスト
        if (itemManagerBehaviour != null && itemManagerBehaviour is ItemManager)
        {
            itemManager = (ItemManager)itemManagerBehaviour;
        }
        else
        {
            itemManager = null;
            if (itemManagerBehaviour == null) Debug.LogWarning("[BoardManager3] itemManagerBehaviour not assigned in Inspector.");
            else Debug.LogError("[BoardManager3] itemManagerBehaviour does not implement ItemManager. Please assign the ItemManager component.");
        }
    }

    void Start()
    {
        if (gridStorage == null)
        {
            Debug.LogError("[BoardManager] GridStorage not assigned!");
            return;
        }
        if (boardSpawner == null)
        {
            Debug.LogError("[BoardManager] BoardSpawner not assigned!");
            return;
        }
        if (addManager == null)
        {
            Debug.LogError("[BoardManager] addManager not available. Assign AddManager component to addManagerBehaviour.");
            return;
        }
        if (slideManager == null)
        {
            Debug.LogError("[BoardManager] slideManager (ISlideManager) not available. Assign SlideManager component to slideManagerBehaviour.");
            return;
        }
        if (deleteManager == null)
        {
            Debug.LogError("[BoardManager] deletemanager not available. Assign DeleteManager component to deleteManagerBehaviour.");
            return;
        }
        if (itemManager == null)
        {
            Debug.LogError("[BoardManager] itemManager not available. Assign ItemManager component to itemManagerBehaviour.");
            return;
        }

        // Init board via GridStorage + BoardSpawner
        InitBoard();

        int center = (fullSize - 1) / 2;
        playerX = center;
        playerY = center;
        gridData[playerX, playerY] = PLAYER;
        playerObj = boardSpawner.SpawnOccupant(PLAYER, playerX, playerY);

        lastPlayerX = playerX;
        lastPlayerY = playerY;

        // make sure itemManager knows boardManager reference (ItemManager.boardManager is public; set here)
        itemManager.boardManager = this;

        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[BoardManager] GameStateManager not found in scene. Please add one.");
        }
        else
        {
            GameStateManager.Instance.OnRequestAdd += OnAddRequested;
            GameStateManager.Instance.OnRequestSlide += OnSlideRequested;
            GameStateManager.Instance.OnRequestDelete += OnDeleteRequested;
            GameStateManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnRequestAdd -= OnAddRequested;
            GameStateManager.Instance.OnRequestSlide -= OnSlideRequested;
            GameStateManager.Instance.OnRequestDelete -= OnDeleteRequested;
            GameStateManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    // ---------------- Event handlers (modified Add/Delete routing) ----------------
    void OnAddRequested()
    {
        if (isGameOver) return;

        // Use AddManager to perform block spawning and get insertedBlocks
        if (addManager == null)
        {
            Debug.LogError("[BoardManager3] addManager not set - cannot AddBlocks.");
        }
        else
        {
            insertedBlocks = addManager.AddBlocks(addCount);

            // BoardManager still responsible to set gridData values and spawn occupants according to own probabilities
            foreach (var (x,y) in insertedBlocks)
            {
                // set gridData using BoardManager's probability logic
                gridData[x,y] = GenerateCellValueByProbability();
                if (gridData[x,y] != 0)
                {
                    boardSpawner.SpawnOccupant(gridData[x,y], x, y);
                }
            }
        }

        lastPlayerX = playerX;
        lastPlayerY = playerY;
        attackPerformedThisSlide = false;

        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.PlayerMove);
    }

    void OnSlideRequested()
    {
        if (isGameOver) return;
        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);

        // SlideManager に処理を委譲（完了したら Delete フェーズへ）
        StartCoroutine(RunSlideAndThenDelete());
    }

    private IEnumerator RunSlideAndThenDelete()
    {
        if (slideManager == null)
        {
            Debug.LogError("[BoardManager3] slideManager is null. Cannot slide.");
            yield break;
        }

        // デバッグチェック: SlideManager と同じ GridStorage を参照しているか確認
        var slideMgr = slideManagerBehaviour as SlideManager;
        if (slideMgr != null && slideMgr.gridStorage != gridStorage)
        {
            Debug.LogError("[BoardManager3] WARNING: SlideManager.gridStorage != BoardManager.gridStorage. Assign the same GridStorage instance in Inspector.");
        }

        bool callbackCalled = false;
        SlideResult slideResult = null;

        // call with callback to receive SlideResult
        yield return StartCoroutine(slideManager.SlideInsertedBlocksCoroutine(insertedBlocks, slideDuration, (result) =>
        {
            callbackCalled = true;
            slideResult = result;
        }));

        if (!callbackCalled)
        {
            Debug.LogWarning("[BoardManager3] Slide completed but callback not called. Falling back to ResyncFromGrid().");
            ResyncFromGrid();
            GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
            yield break;
        }

        if (slideResult == null || !slideResult.Success)
        {
            Debug.LogError("[BoardManager3] SlideResult null or failed. Falling back to ResyncFromGrid().");
            ResyncFromGrid();
            GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
            yield break;
        }

        // Use SlideResult to update player pos deterministically if provided
        if (slideResult.PlayerPos.x >= 0 && slideResult.PlayerPos.y >= 0)
        {
            playerX = slideResult.PlayerPos.x;
            playerY = slideResult.PlayerPos.y;
            // update playerObj reference if occupant present
            if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
            {
                playerObj = occupants[playerX, playerY];
            }
            else playerObj = null;
        }
        else
        {
            Debug.LogWarning("[BoardManager3] SlideResult did not contain PLAYER position. Will attempt full resync.");
        }

        // Apply transform / parent fixes for all occupied cells (keep in sync visually)
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                var block = grid[x,y];
                var occ = occupants[x,y];
                if (block != null && occ != null)
                {
                    if (occ.transform.parent != block.transform)
                    {
                        occ.transform.SetParent(block.transform);
                    }
                    occ.transform.localPosition = Vector3.up * 0.5f;
                }
                if (block != null && gridStorage != null)
                {
                    block.transform.position = gridStorage.WorldPosition(x, y);
                }
            }
        }

        // Extra safety: detect surprising holes/duplicates via slideResult. If large diffs, log for debugging.
        int changesCount = slideResult.Changes != null ? slideResult.Changes.Count : 0;
        if (changesCount > (fullSize * fullSize) / 4) // heuristic: too many changes?
        {
            Debug.LogWarning($"[BoardManager3] Large number of cell changes reported by SlideResult: {changesCount}. Check logic.");
        }

        // スライド後の状態を受けて Delete フェーズへ
        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
    }

    void OnDeleteRequested()
    {
        if (isGameOver) return;

        // Delegate actual destruction to DeleteManager
        if (deleteManager == null)
        {
            Debug.LogError("[BoardManager3] deleteManager not set - cannot DeleteBlocks.");
        }
        else
        {
            deleteManager.DeleteBlocks(insertedBlocks);
        }

        // BoardManager keeps responsibility for bookkeeping and leveling
        insertedBlocks.Clear();
        LevelUp();
        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Add);
    }

    void OnPhaseChanged(GameStateManager.GamePhase newPhase)
    {
        if (newPhase == GameStateManager.GamePhase.Slide && !attackPerformedThisSlide)
        {
            PerformAttackAtLastPosition();
            attackPerformedThisSlide = true;
        }
    }

    // Perform attack (now queries ItemManager for shield)
    void PerformAttackAtLastPosition()
    {
        Debug.Log($"Attack at ({lastPlayerX},{lastPlayerY})");

        // ask itemManager to consume shield if it applies
        if (itemManager != null && itemManager.TryConsumeShieldProtect(playerX, lastPlayerX, lastPlayerY))
        {
            // shield protected — nothing else to do
            return;
        }

        if (playerX == lastPlayerX && playerY == lastPlayerY)
        {
            Debug.Log("Player hit by attack! Game Over.");
            GameOver();
        }
    }

    // ---------------- Init / Slide / Delete / Items (元のまま) ----------------

    void InitBoard()
    {
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                // ensure arrays are empty
                if (grid[x,y] != null) Destroy(grid[x,y]);
                if (occupants[x,y] != null) Destroy(occupants[x,y]);
                grid[x,y] = null;
                occupants[x,y] = null;
                gridData[x,y] = 0;
            }
        }

        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                boardSpawner.SpawnBlockAt(x, y);
                gridData[x, y] = GenerateCellValueByProbability();
                if (gridData[x,y] != 0)
                {
                    boardSpawner.SpawnOccupant(gridData[x,y], x, y);
                }
            }
        }
    }

    void LevelUp()
    {
        level++;
        Debug.Log("Level Up! " + level);
        Direction dirToIncrease = (Direction)((level - 1) % 4);
        addCount[dirToIncrease] = Mathf.Min(3, addCount[dirToIncrease] + 1);
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
        if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Add);
        Debug.Log("=== GAME OVER ===");
    }

    // Public wrapper so DeleteManager can notify BoardManager of player-destroy event
    public void GameOverPublic()
    {
        GameOver();
    }

    // Forwarding wrapper to ItemManager
    public void ConsumeItemAt(int x,int y)
    {
        if (itemManager != null) itemManager.ConsumeItemAt(x,y);
        else
        {
            int cell = gridData[x,y];
            if (!(cell == ITEM_A || cell == ITEM_B || cell == ITEM_C)) return;
            int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
            boardSpawner.RemoveOccupantAt(x,y);
            gridData[x,y] = 0;
        }
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
        if (grid[newX,newY] != null) playerGO.transform.SetParent(grid[newX,newY].transform);
        playerGO.transform.localPosition = Vector3.up * 0.5f;
        playerObj = playerGO;
    }

    // ------------------ 新しく公開する（PlayerController 用）メソッド群 ------------------

    // Getters used by PlayerController
    public int GetPlayerX() => playerX;
    public int GetPlayerY() => playerY;
    public GameObject GetPlayerObject() => playerObj;
    public int GetCoreSize() => coreSize;
    public int GetFullSize() => fullSize;
    public int GetGridDataAt(int x, int y) => gridData[x,y];
    public void SetGridDataAt(int x, int y, int value) => gridData[x,y] = value;

    // Item accessors (forward to ItemManager)
    public int GetItemCount(int idx) { if (itemManager != null) return itemManager.GetItemCount(idx); return 0; }
    public void IncrementItemCount(int idx) { if (itemManager != null) itemManager.Debug_SetCount(idx, GetItemCount(idx) + 1); }

    // pending mode / item start wrappers (expose existing logic safely)
    public int GetPendingMode() => itemManager != null ? itemManager.GetPendingMode() : 0;
    public void StartUseItemPublic(int itemIndex) { if (itemManager != null) itemManager.StartUseItem(itemIndex); else StartUseItem(itemIndex); }
    public bool HandlePendingDirectionInputPublic() { if (itemManager != null) return itemManager.HandlePendingDirectionInput(); return false; }

    // allow PlayerController to remove occupant visuals via BoardSpawner
    public void RemoveOccupantAtPublic(int x,int y) => boardSpawner.RemoveOccupantAt(x,y);

    // Allow PlayerController to set player pos after moving
    public void SetPlayerPosition(int x,int y) { playerX = x; playerY = y; }

    // ------------------ 元の StartUseItem / HandlePendingDirectionInput を内部で維持 (fallback) ------------------

    void StartUseItem(int itemIndex)
    {
        // fallback: local implementation (kept minimal)
        if (itemIndex < 0 || itemIndex > 2) return;
        Debug.Log("[BoardManager3] StartUseItem fallback called. Please assign ItemManager to itemManagerBehaviour.");
    }

    bool HandlePendingDirectionInput()
    {
        Debug.Log("[BoardManager3] HandlePendingDirectionInput fallback called. Please assign ItemManager to itemManagerBehaviour.");
        return false;
    }

    bool DestroyObstacleAt(int x,int y)
    {
        if (x < 1 || y < 1 || x > coreSize || y > coreSize) return false;
        if (gridData[x,y] != OBSTACLE) return false;
        boardSpawner.RemoveOccupantAt(x,y);
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
                    boardSpawner.RemoveOccupantAt(x,y);
                    gridData[x,y] = 0;
                }
            }
        }
        Debug.Log("All obstacles cleared.");
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    int GenerateCellValueByProbability()
    {
        float obstacleProb = Mathf.Clamp01(obstacleBaseProb + (level - 1) * obstacleIncreasePerLevel);
        float r = UnityEngine.Random.value;
        if (r < itemProbEach) return ITEM_A;
        if (r < itemProbEach * 2f) return ITEM_B;
        if (r < itemProbEach * 3f) return ITEM_C;
        if (r < itemProbEach * 3f + obstacleProb) return OBSTACLE;
        return 0;
    }

    // ---------------- ここから追加: スライド後に BoardManager の内部状態を grid に合わせる ----------------

    private void ResyncFromGrid()
    {
        if (gridStorage == null) return;

        // 1) PLAYER の再検出
        int foundX = -1, foundY = -1;
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (gridData[x,y] == PLAYER)
                {
                    foundX = x; foundY = y;
                }
            }
        }

        if (foundX != -1)
        {
            playerX = foundX;
            playerY = foundY;
            if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
            {
                playerObj = occupants[playerX, playerY];
            }
        }
        else
        {
            Debug.LogWarning("[BoardManager3] ResyncFromGrid: no PLAYER found in gridData after slide.");
            playerObj = null;
        }

        // 2) Occupants の親子付けとローカル位置を GridObjects に合わせる
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                var block = grid[x,y];
                var occ = occupants[x,y];
                if (block != null && occ != null)
                {
                    if (occ.transform.parent != block.transform)
                    {
                        occ.transform.SetParent(block.transform);
                    }
                    occ.transform.localPosition = Vector3.up * 0.5f;
                }
                if (block != null && gridStorage != null)
                {
                    block.transform.position = gridStorage.WorldPosition(x, y);
                }
            }
        }
    }
}
