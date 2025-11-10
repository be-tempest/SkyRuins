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

    [System.NonSerialized] public int PLAYER = 1;
    [System.NonSerialized] public int OBSTACLE = 2;
    [System.NonSerialized] public int ITEM_A = 3;
    [System.NonSerialized] public int ITEM_B = 4;
    [System.NonSerialized] public int ITEM_C = 5;

    // 方向
    public enum Direction { Left, Up, Right, Down }

    // レベル・追加ブロック
    private int level = 1;
    private List<(int x, int y)> insertedBlocks = new List<(int, int)>();
    private Dictionary<Direction, int> addCount = new Dictionary<Direction, int>()
    {
        { Direction.Left, 1 },
        { Direction.Up, 0 },
        { Direction.Right, 0 },
        { Direction.Down, 0 }
    };

    // プレイヤーデータ
    private GameObject playerObj;
    private int playerX;
    private int playerY;
    private int lastPlayerX;
    private int lastPlayerY;
    private bool attackPerformedThisSlide = false;
    private bool isGameOver = false;

    //　スライド時間
    private float slideDuration = 0.40f;

    // 確率設定
    [SerializeField] private float itemProbEach = 0.05f;
    [SerializeField] private float obstacleBaseProb = 0.20f;
    [SerializeField] private float obstacleIncreasePerLevel = 0.02f;

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

        InitBoard(); // 

        // プレイヤー初期設定
        int center = (fullSize - 1) / 2;
        playerX = center;
        playerY = center;
        lastPlayerX = playerX;
        lastPlayerY = playerY;
        gridData[playerX, playerY] = PLAYER;
        playerObj = boardSpawner.SpawnOccupant(PLAYER, playerX, playerY); //

        itemManager.boardManager = this;

        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[BoardManager] GameStateManager not found in scene. Please add one.");
        }
        else
        {
            // イベント登録
            GameStateManager.Instance.OnRequestAdd += OnAddRequested;
            GameStateManager.Instance.OnRequestSlide += OnSlideRequested;
            GameStateManager.Instance.OnRequestDelete += OnDeleteRequested;
            GameStateManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }
    }

    // 盤面生成
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

    // 占有オブジェクト確率
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


    // ---------------- イベント関連 ----------------
    // Addフェーズ
    void OnAddRequested()
    {
        if (isGameOver) return;

        if (addManager == null)
        {
            Debug.LogError("[BoardManager3] addManager not set - cannot AddBlocks.");
        }
        else
        {
            insertedBlocks = addManager.AddBlocks(addCount);

            foreach (var (x,y) in insertedBlocks)
            {
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

    // Slideフェーズ
    void OnSlideRequested()
    {
        if (isGameOver) return;
        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);
        StartCoroutine(RunSlide());
    }

    // Deleteフェーズ
    void OnDeleteRequested()
    {
        if (isGameOver) return;

        if (deleteManager == null)
        {
            Debug.LogError("[BoardManager3] deleteManager not set - cannot DeleteBlocks.");
        }
        else
        {
            deleteManager.DeleteBlocks(insertedBlocks);
        }

        insertedBlocks.Clear();
        LevelUp();
        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Add);
    }

    // フェーズ変更
    void OnPhaseChanged(GameStateManager.GamePhase newPhase)
    {
        if (newPhase == GameStateManager.GamePhase.Slide && !attackPerformedThisSlide)
        {
            PerformAttackAtLastPosition();
            attackPerformedThisSlide = true;
        }
    }

    // フェーズ解除
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

    // スライド実行コルーチン
    private IEnumerator RunSlide()
    {
        if (slideManager == null)
        {
            Debug.LogError("[BoardManager3] slideManager is null. Cannot slide.");
            yield break;
        }

        bool callbackCalled = false;
        SlideResult slideResult = null;

        // スライド実行
        yield return StartCoroutine(slideManager.SlideInsertedBlocksCoroutine(insertedBlocks, slideDuration, (result) =>
        {
            callbackCalled = true;
            slideResult = result;
        }));

        // コールバックなし
        if (!callbackCalled)
        {
            Debug.LogWarning("[BoardManager3] Slide completed but callback not called. Falling back to ResyncFromGrid().");
            ResyncFromGrid();
            GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
            yield break;
        }
        // スライド失敗
        if (slideResult == null || !slideResult.Success)
        {
            Debug.LogError("[BoardManager3] SlideResult null or failed. Falling back to ResyncFromGrid().");
            ResyncFromGrid();
            GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
            yield break;
        }

        // ？
        if (slideResult.PlayerPos.x >= 0 && slideResult.PlayerPos.y >= 0)
        {
            playerX = slideResult.PlayerPos.x;
            playerY = slideResult.PlayerPos.y;
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

        // セル変更を適用
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                var block = grid[x, y];
                var occ = occupants[x, y];
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

        GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Delete);
    }
    

    // ---------------- プレイヤー関連 ----------------
    public int GetPlayerX() => playerX;
    public int GetPlayerY() => playerY;
    public GameObject GetPlayerObject() => playerObj;
    public int GetCoreSize() => coreSize;
    public int GetGridDataAt(int x, int y) => gridData[x,y];
    public void SetGridDataAt(int x, int y, int value) => gridData[x, y] = value;

    // プレイヤー移動
    public void MovePlayerTo(int oldX, int oldY, int newX, int newY, GameObject playerGO)
    {
        if (occupants[newX, newY] != null && occupants[newX, newY] != playerGO)
        {
            Destroy(occupants[newX, newY]);
            occupants[newX, newY] = null;
        }
        if (occupants[oldX, oldY] == playerGO) occupants[oldX, oldY] = null;
        gridData[oldX, oldY] = 0;
        gridData[newX, newY] = PLAYER;
        occupants[newX, newY] = playerGO;
        if (grid[newX, newY] != null) playerGO.transform.SetParent(grid[newX, newY].transform);
        playerGO.transform.localPosition = Vector3.up * 0.5f;
        playerX = newX;
        playerY = newY;
    }

    // 攻撃判定    
    void PerformAttackAtLastPosition()
    {
        Debug.Log($"Attack at ({lastPlayerX},{lastPlayerY})");

        if (itemManager != null && itemManager.TryConsumeShieldProtect(playerX, lastPlayerX, lastPlayerY))
        {
            return;
        }

        if (playerX == lastPlayerX && playerY == lastPlayerY)
        {
            Debug.Log("Player hit by attack! Game Over.");
            GameOver();
        }
    }

    // レベルアップ
    void LevelUp()
    {
        level++;
        Debug.Log("Level Up! " + level);
        Direction dirToIncrease = (Direction)((level - 1) % 4);
        addCount[dirToIncrease] = Mathf.Min(3, addCount[dirToIncrease] + 1);
    }

    // ゲームオーバー
    public void GameOver()
    {
        isGameOver = true;
        if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Add);
        Debug.Log("=== GAME OVER ===");
    }


    // ---------------- その他 ----------------
    // リストシャッフル
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void ResyncFromGrid()
    {
        if (gridStorage == null) return;

        int foundX = -1, foundY = -1;
        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (gridData[x, y] == PLAYER)
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

        for (int x = 0; x < fullSize; x++)
        {
            for (int y = 0; y < fullSize; y++)
            {
                var block = grid[x, y];
                var occ = occupants[x, y];
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
