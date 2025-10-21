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
    // 3種類のアイテムプレハブをセット（Inspectorで3個登録）
    public GameObject[] itemPrefabs = new GameObject[3];

    public int coreSize = 7;      // 1..7 がコア領域
    private int fullSize = 9;     // 0..8 を使う（外枠含む）

    public GameObject[,] grid;       // 土台ブロック参照（index は 0..8）
    private GameObject[,] occupants; // ブロック上のオブジェクト（item/obstacle/player）
    public int[,] gridData;          // マップデータ（値は下の定数参照）

    // マップ値（新しい割当）
    private const int PLAYER = 1;
    private const int OBSTACLE = 2;
    private const int ITEM_A = 3;
    private const int ITEM_B = 4;
    private const int ITEM_C = 5;

    public enum GamePhase { Add, PlayerMove, Slide, Remove }
    public GamePhase phase = GamePhase.Add;

    public enum Direction { Left, Up, Right, Down }

    // レベル&追加カウントは従来通り
    private int level = 1;
    private Dictionary<Direction, int> addCount = new Dictionary<Direction, int>()
    {
        { Direction.Left, 1 },
        { Direction.Up, 0 },
        { Direction.Right, 0 },
        { Direction.Down, 0 }
    };

    private List<(int x, int y)> insertedBlocks = new List<(int, int)>();

    // プレイヤー状態
    private int playerX;
    private int playerY;
    private GameObject playerObj;

    // 攻撃ターゲット（前ターンの位置）
    private int lastPlayerX;
    private int lastPlayerY;
    private bool attackPerformedThisSlide = false;

    // ゲームオーバー
    private bool isGameOver = false;

    // スライドアニメ設定
    [SerializeField] private float slideDuration = 0.28f;

    // --- 出現確率設定 ---
    [Header("Spawn Probabilities")]
    [Range(0f, 1f)] public float itemProbEach = 0.05f; // 各アイテムの個別確率（デフォルト5%）
    [Range(0f, 1f)] public float obstacleBaseProb = 0.20f; // 障害物ベース確率（デフォルト20%）
    [Range(0f, 1f)] public float obstacleIncreasePerLevel = 0.02f; // レベル毎の増加量（調整可）

    // プレイヤーが保持しているアイテム数（index 0 = ITEM_A, 1 = ITEM_B, 2 = ITEM_C）
    private int[] itemsCount = new int[3] { 0, 0, 0 };

    // ---------------- Unity ライフサイクル ----------------
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

        // プレイヤーを中央に配置
        int center = coreSize / 2 + 1; // -> 4
        playerX = center;
        playerY = center;
        gridData[playerX, playerY] = PLAYER;
        PlaceObjectOnBlock(playerX, playerY);
        playerObj = occupants[playerX, playerY];

        lastPlayerX = playerX;
        lastPlayerY = playerY;
        attackPerformedThisSlide = false;
    }

    void Update()
    {
        if (isGameOver) return;

        // Add phase
        if (Input.GetKeyDown(KeyCode.A) && phase == GamePhase.Add)
        {
            AddBlocks();
            phase = GamePhase.PlayerMove;

            // 記録（このターンに攻撃が当たる位置 = 現在のプレイヤー位置）
            lastPlayerX = playerX;
            lastPlayerY = playerY;
            attackPerformedThisSlide = false;
            return;
        }

        // PlayerMove phase: check item usage keys first (1/2/3)
        if (phase == GamePhase.PlayerMove)
        {
            // アイテム使用（1,2,3）
            if (Input.GetKeyDown(KeyCode.Alpha1)) { UseItem(0); return; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { UseItem(1); return; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { UseItem(2); return; }

            HandlePlayerInput();
            return;
        }

        // Slide phase: first perform attack (once)
        if (phase == GamePhase.Slide && !attackPerformedThisSlide)
        {
            PerformAttackAtLastPosition();
            attackPerformedThisSlide = true;
            if (isGameOver) return;
        }

        // S to start slide coroutine (this will start the group-based simultaneous slide)
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

    // ---------------- 初期化 ----------------
    void InitBoard()
    {
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                SpawnBlock(x, y);
                gridData[x, y] = GenerateCellValueByProbability();
                PlaceObjectOnBlock(x, y);
            }
        }
    }

    void SpawnBlock(int x, int y)
    {
        Vector3 pos = new Vector3(x, 0, y);
        grid[x, y] = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
    }

    // ---------------- プレイヤー入力 ----------------
    void HandlePlayerInput()
    {
        int newX = playerX;
        int newY = playerY;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newY++;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newY--;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newX--;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newX++;
        else if (Input.GetKeyDown(KeyCode.Return)) { /* stay */ }
        else return;

        // 範囲チェック
        if (newX < 1 || newY < 1 || newX > coreSize || newY > coreSize) return;

        int cell = gridData[newX, newY];
        if (cell == OBSTACLE) return; // 障害物は移動不可

        // アイテムを踏む（移動先がアイテムであり、移動したときのみ取得）
        if ((cell == ITEM_A || cell == ITEM_B || cell == ITEM_C) && !(newX == playerX && newY == playerY))
        {
            int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
            itemsCount[idx]++;
            Debug.Log($"Picked up item {(idx + 1)}. Now have {itemsCount[idx]}.");
            if (occupants[newX, newY] != null) { Destroy(occupants[newX, newY]); occupants[newX, newY] = null; }
            gridData[newX, newY] = 0;
        }

        // 移動（stay の場合は移動処理をスキップ）
        if (!(newX == playerX && newY == playerY))
        {
            MovePlayerTo(playerX, playerY, newX, newY, playerObj);
            playerX = newX;
            playerY = newY;
        }

        // 行動したらスライドフェーズへ
        phase = GamePhase.Slide;
    }

    // ---------------- レベル / 追加 ----------------
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
        switch (dir)
        {
            case Direction.Left: x = 0; y = idx; break;
            case Direction.Right: x = coreSize + 1; y = idx; break;
            case Direction.Up: x = idx; y = coreSize + 1; break;
            case Direction.Down: x = idx; y = 0; break;
        }

        SpawnBlock(x, y);
        gridData[x, y] = GenerateCellValueByProbability();
        PlaceObjectOnBlock(x, y);
        insertedBlocks.Add((x, y));

        Debug.Log($"Added {dir} block at ({x},{y}) value={gridData[x,y]}");
    }

    // ---------------- 同辺同時スライド処理 ----------------
    // RunAndFlag wrapper
    private IEnumerator RunAndFlag(IEnumerator routine, List<bool> doneFlags, int index)
    {
        if (isGameOver)
        {
            doneFlags[index] = true;
            yield break;
        }

        yield return StartCoroutine(routine);
        doneFlags[index] = true;
    }

    // Grouped SlideBlocksCoroutine: same edge entries run in parallel
    private IEnumerator SlideBlocksCoroutine(float duration)
    {
        // group insertedBlocks by direction, keep edge order Left->Up->Right->Down
        var groups = new Dictionary<Direction, List<(int x, int y)>>()
        {
            { Direction.Left, new List<(int,int)>() },
            { Direction.Up,    new List<(int,int)>() },
            { Direction.Right, new List<(int,int)>() },
            { Direction.Down,  new List<(int,int)>() }
        };

        foreach (var (x, y) in insertedBlocks)
        {
            if (x == 0) groups[Direction.Left].Add((x, y));
            else if (x == coreSize + 1) groups[Direction.Right].Add((x, y));
            else if (y == 0) groups[Direction.Down].Add((x, y));
            else if (y == coreSize + 1) groups[Direction.Up].Add((x, y));
        }

        Direction[] order = new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

        foreach (var dir in order)
        {
            if (isGameOver) yield break;

            var list = groups[dir];
            if (list == null || list.Count == 0) continue;

            // prepare flags
            var doneFlags = new List<bool>(new bool[list.Count]);

            for (int i = 0; i < list.Count; i++)
            {
                if (isGameOver)
                {
                    doneFlags[i] = true;
                    continue;
                }

                int localIndex = i; // avoid closure capture issues
                var (x, y) = list[localIndex];

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

            // wait until all doneFlags are true
            while (true)
            {
                if (isGameOver) break;

                bool allDone = true;
                for (int k = 0; k < doneFlags.Count; k++)
                {
                    if (!doneFlags[k])
                    {
                        allDone = false;
                        break;
                    }
                }
                if (allDone) break;
                yield return null;
            }

            // small gap for visual separation
            yield return new WaitForSeconds(0.03f);
        }

        // finished
        phase = GamePhase.Remove;
        yield break;
    }

    // ブロック（親オブジェクト）を滑らかに移動させるヘルパー
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

    // SlideRowAnimated / SlideColumnAnimated: visual animation then array update
    private IEnumerator SlideRowAnimated(int row, Direction dir, float duration)
    {
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Left)
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x, row] != null)
                {
                    Vector3 from = grid[x, row].transform.position;
                    Vector3 to = new Vector3(x + 1, 0, row);
                    movers.Add((grid[x, row], from, to));
                }
            }
        }
        else
        {
            for (int x = 0; x < fullSize; x++)
            {
                if (grid[x, row] != null)
                {
                    Vector3 from = grid[x, row].transform.position;
                    Vector3 to = new Vector3(x - 1, 0, row);
                    movers.Add((grid[x, row], from, to));
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

        // array update
        if (dir == Direction.Left)
        {
            for (int x = fullSize - 1; x >= 1; x--)
            {
                grid[x, row] = grid[x - 1, row];
                occupants[x, row] = occupants[x - 1, row];
                gridData[x, row] = gridData[x - 1, row];

                if (gridData[x, row] == PLAYER)
                {
                    playerX = x;
                    playerY = row;
                }
            }
            grid[0, row] = null; occupants[0, row] = null; gridData[0, row] = 0;
        }
        else
        {
            for (int x = 0; x <= fullSize - 2; x++)
            {
                grid[x, row] = grid[x + 1, row];
                occupants[x, row] = occupants[x + 1, row];
                gridData[x, row] = gridData[x + 1, row];

                if (gridData[x, row] == PLAYER)
                {
                    playerX = x;
                    playerY = row;
                }
            }
            grid[fullSize - 1, row] = null; occupants[fullSize - 1, row] = null; gridData[fullSize - 1, row] = 0;
        }

        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
            playerObj = occupants[playerX, playerY];

        yield break;
    }

    private IEnumerator SlideColumnAnimated(int col, Direction dir, float duration)
    {
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();
        if (dir == Direction.Up)
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col, y] != null)
                {
                    Vector3 from = grid[col, y].transform.position;
                    Vector3 to = new Vector3(col, 0, y - 1);
                    movers.Add((grid[col, y], from, to));
                }
            }
        }
        else
        {
            for (int y = 0; y < fullSize; y++)
            {
                if (grid[col, y] != null)
                {
                    Vector3 from = grid[col, y].transform.position;
                    Vector3 to = new Vector3(col, 0, y + 1);
                    movers.Add((grid[col, y], from, to));
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
                grid[col, y - 1] = grid[col, y];
                occupants[col, y - 1] = occupants[col, y];
                gridData[col, y - 1] = gridData[col, y];

                if (gridData[col, y - 1] == PLAYER)
                {
                    playerX = col;
                    playerY = y - 1;
                }
            }
            grid[col, fullSize - 1] = null; occupants[col, fullSize - 1] = null; gridData[col, fullSize - 1] = 0;
        }
        else
        {
            for (int y = fullSize - 2; y >= 0; y--)
            {
                grid[col, y + 1] = grid[col, y];
                occupants[col, y + 1] = occupants[col, y];
                gridData[col, y + 1] = gridData[col, y];

                if (gridData[col, y + 1] == PLAYER)
                {
                    playerX = col;
                    playerY = y + 1;
                }
            }
            grid[col, 0] = null; occupants[col, 0] = null; grid[col, 0] = null; gridData[col, 0] = 0;
        }

        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
            playerObj = occupants[playerX, playerY];

        yield break;
    }

    // ---------------- 攻撃 ----------------
    void PerformAttackAtLastPosition()
    {
        Debug.Log($"Attack at ({lastPlayerX},{lastPlayerY})");
        if (playerX == lastPlayerX && playerY == lastPlayerY)
        {
            Debug.Log("Player hit by attack! Game Over.");
            GameOver();
        }
    }

    // ---------------- 削除 ----------------
    void RemoveBlocks()
    {
        foreach (var (x, y) in insertedBlocks)
        {
            if (isGameOver) break;

            if (x == 0) { int tx = fullSize - 1, ty = y; DestroyAt(tx, ty); }
            else if (x == coreSize + 1) { int tx = 0, ty = y; DestroyAt(tx, ty); }
            else if (y == 0) { int tx = x, ty = fullSize - 1; DestroyAt(tx, ty); }
            else if (y == coreSize + 1) { int tx = x, ty = 0; DestroyAt(tx, ty); }
        }
        insertedBlocks.Clear();
    }

    void DestroyAt(int x, int y)
    {
        if (occupants[x, y] != null && occupants[x, y] == playerObj)
        {
            Debug.Log("Player pushed out of board! Game Over.");
            GameOver();
        }

        if (grid[x, y] != null) Destroy(grid[x, y]);
        if (occupants[x, y] != null) Destroy(occupants[x, y]);
        grid[x, y] = null;
        occupants[x, y] = null;
        gridData[x, y] = 0;
    }

    void GameOver()
    {
        isGameOver = true;
        phase = GamePhase.Add;
        Debug.Log("=== GAME OVER ===");
    }

    // ---------------- 生成 / 取得 / 移動 ----------------
    public void PlaceObjectOnBlock(int x, int y)
    {
        if (grid[x, y] == null) return;

        if (occupants[x, y] != null)
        {
            Destroy(occupants[x, y]);
            occupants[x, y] = null;
        }

        int cell = gridData[x, y];
        if (cell == 0) return;

        if (cell == OBSTACLE)
        {
            occupants[x, y] = Instantiate(obstaclePrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
        }
        else if (cell == PLAYER)
        {
            occupants[x, y] = Instantiate(playerPrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
        }
        else if (cell == ITEM_A || cell == ITEM_B || cell == ITEM_C)
        {
            int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
            if (itemPrefabs != null && itemPrefabs.Length > idx && itemPrefabs[idx] != null)
                occupants[x, y] = Instantiate(itemPrefabs[idx], grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
            else
                Debug.LogWarning($"itemPrefabs[{idx}] not set");
        }
    }

    public void ConsumeItemAt(int x, int y)
    {
        int cell = gridData[x, y];
        if (!(cell == ITEM_A || cell == ITEM_B || cell == ITEM_C)) return;

        int idx = (cell == ITEM_A) ? 0 : (cell == ITEM_B) ? 1 : 2;
        itemsCount[idx]++;
        if (occupants[x, y] != null) { Destroy(occupants[x, y]); occupants[x, y] = null; }
        gridData[x, y] = 0;
    }

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
        playerGO.transform.SetParent(grid[newX, newY].transform);
        playerGO.transform.localPosition = Vector3.up * 0.5f;

        playerObj = playerGO;
    }

    // ---------------- アイテム使用 ----------------
    void UseItem(int index)
    {
        if (index < 0 || index >= itemsCount.Length) return;
        if (itemsCount[index] <= 0)
        {
            Debug.Log($"No item {index + 1} to use.");
            return;
        }

        itemsCount[index]--;
        Debug.Log($"Used item {index + 1}. Remaining: {itemsCount[index]}");

        // TODO: 実際の効果をここに実装（例：前方の障害物破壊 / 攻撃無効化 / スコア倍率など）
    }

    // ---------------- ヘルパー ----------------
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // 出現確率に従ってセル値を生成
    int GenerateCellValueByProbability()
    {
        float obstacleProb = obstacleBaseProb + (level - 1) * obstacleIncreasePerLevel;
        obstacleProb = Mathf.Clamp01(obstacleProb);

        float itemTotal = itemProbEach * 3f;
        float r = Random.value;

        if (r < itemProbEach) return ITEM_A;
        if (r < itemProbEach * 2f) return ITEM_B;
        if (r < itemProbEach * 3f) return ITEM_C;
        if (r < itemProbEach * 3f + obstacleProb) return OBSTACLE;
        return 0;
    }
}
