using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // -----------------------------
    // 定義 / フィールド
    // -----------------------------
    public static BoardManager Instance { get; private set; }

    public GameObject blockPrefab;
    public GameObject itemPrefab;
    public GameObject obstaclePrefab;
    public GameObject playerPrefab;

    public int coreSize = 7;      // 1..7 がコア領域
    private int fullSize = 9;     // 0..8 を使う（外枠含む）

    public GameObject[,] grid;    // 土台ブロック参照（index は 0..8）
    private GameObject[,] occupants; // ブロック上のオブジェクト（item/obstacle/player）
    public int[,] gridData;       // 0=empty,1=item,2=obstacle,3=player

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

    // プレイヤーの現在位置（配列インデックス）
    private int playerX;
    private int playerY;
    private GameObject playerObj;

    // アニメ設定
    [SerializeField] private float slideDuration = 0.28f;

    // -----------------------------
    // Unity ライフサイクル
    // -----------------------------
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 配列初期化
        grid = new GameObject[fullSize, fullSize];
        occupants = new GameObject[fullSize, fullSize];
        gridData = new int[fullSize, fullSize];

        InitBoard();

        // プレイヤーを中央に配置（1..7 の中心）
        int center = coreSize / 2 + 1; // -> 4
        playerX = center;
        playerY = center;
        gridData[playerX, playerY] = 3;
        PlaceObjectOnBlock(playerX, playerY);
        playerObj = occupants[playerX, playerY];
    }

    void Update()
    {
        // フェーズの進行はキーと入力で行う
        if (Input.GetKeyDown(KeyCode.A) && phase == GamePhase.Add)
        {
            AddBlocks();
            phase = GamePhase.PlayerMove;
            return;
        }

        if (phase == GamePhase.PlayerMove)
        {
            HandlePlayerInput();
            return;
        }

        // Sでスライド開始（コルーチンでアニメ→配列更新）
        if (Input.GetKeyDown(KeyCode.S) && phase == GamePhase.Slide)
        {
            StartCoroutine(SlideBlocksCoroutine(slideDuration));
            return;
        }

        if (Input.GetKeyDown(KeyCode.D) && phase == GamePhase.Remove)
        {
            RemoveBlocks();
            LevelUp();
            phase = GamePhase.Add;
            return;
        }
    }

    // -----------------------------
    // 初期化関連
    // -----------------------------
    void InitBoard()
    {
        // コア領域 1..coreSize にブロックを置き、ランダムにitem/obstacle を配置
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                SpawnBlock(x, y);
                gridData[x, y] = Random.Range(0, 3); // 0=empty,1=item,2=obstacle
                PlaceObjectOnBlock(x, y);
            }
        }

        // 外枠（0 と fullSize-1）は空のまま
    }

    void SpawnBlock(int x, int y)
    {
        Vector3 pos = new Vector3(x, 0, y);
        grid[x, y] = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
    }

    // -----------------------------
    // 入力 / プレイヤー処理（BoardManager内に統合）
    // -----------------------------
    void HandlePlayerInput()
    {
        int newX = playerX;
        int newY = playerY;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newY++;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newY--;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newX--;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newX++;
        else return;

        // 範囲チェック（コア領域内のみ許可）
        if (newX < 1 || newY < 1 || newX > coreSize || newY > coreSize) return;

        int cell = gridData[newX, newY];
        if (cell == 2) return; // 障害物は不可

        if (cell == 1)
        {
            // アイテム取得
            ConsumeItemAt(newX, newY);
            Debug.Log("アイテムを取得しました！");
        }

        // 移動実行（配列とoccupantsを更新, reparent）
        MovePlayerTo(playerX, playerY, newX, newY, playerObj);

        // playerX/Y更新
        playerX = newX;
        playerY = newY;

        // 移動したらスライドフェーズへ
        phase = GamePhase.Slide;
    }

    // -----------------------------
    // レベル / ブロック追加
    // -----------------------------
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
        gridData[x, y] = Random.Range(0, 3); // 0=empty,1=item,2=obstacle
        PlaceObjectOnBlock(x, y);
        insertedBlocks.Add((x, y));

        Debug.Log($"Added {dir} block at ({x},{y})");
    }

    // -----------------------------
    // helper: 指定の IEnumerator を実行して完了フラグを立てる
    // -----------------------------
    private IEnumerator RunAndFlag(IEnumerator routine, List<bool> doneFlags, int index)
    {
        yield return StartCoroutine(routine);
        doneFlags[index] = true;
    }

    // -----------------------------
    // SlideBlocksCoroutine の同辺同時版
    // -----------------------------
    private IEnumerator SlideBlocksCoroutine(float duration)
    {
        // group insertedBlocks by direction, but keep the edge order: Left -> Up -> Right -> Down
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

        // process edges in clockwise order starting from Left
        Direction[] order = new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

        foreach (var dir in order)
        {
            var list = groups[dir];
            if (list.Count == 0) continue;

            // For each entry in this edge, create a coroutine (SlideRowAnimated or SlideColumnAnimated)
            var doneFlags = new List<bool>();
            var started = new List<Coroutine>();

            for (int i = 0; i < list.Count; i++)
            {
                doneFlags.Add(false);
                var (x, y) = list[i];

                IEnumerator routine;
                if (dir == Direction.Left || dir == Direction.Right)
                {
                    // row slide: y holds the row index
                    int row = y;
                    routine = SlideRowAnimated(row, dir, duration);
                }
                else
                {
                    // column slide: x holds the column index
                    int col = x;
                    routine = SlideColumnAnimated(col, dir, duration);
                }

                // start wrapper that will set doneFlags[i] = true when finished
                started.Add(StartCoroutine(RunAndFlag(routine, doneFlags, i)));
            }

            // wait until all doneFlags are true
            bool allDone = false;
            while (!allDone)
            {
                allDone = true;
                for (int k = 0; k < doneFlags.Count; k++)
                {
                    if (!doneFlags[k]) { allDone = false; break; }
                }
                if (!allDone) yield return null;
            }

            // small pause between edge groups for見た目 (optional)
            yield return new WaitForSeconds(0.03f);
        }

        // 全部終了 -> Remove フェーズへ
        phase = GamePhase.Remove;

        yield break;
    }


    // ブロック（親オブジェクト）を滑らかに移動させるヘルパー
    private IEnumerator MoveTransformOverTime(Transform t, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / duration);
            // イージングにしたければ Mathf.SmoothStep(0,1,t01) 等
            t.position = Vector3.Lerp(from, to, t01);
            yield return null;
        }
        t.position = to;
    }

    // 行アニメ + 配列更新
    private IEnumerator SlideRowAnimated(int row, Direction dir, float duration)
    {
        // 1) collect movers (存在するブロックだけ)
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();

        if (dir == Direction.Left)
        {
            // 左が追加されるケース：各 x のブロックは x -> x+1 の位置に移動
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
        else // Right
        {
            // 右が追加されるケース：各 x のブロックは x -> x-1 の位置に移動
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

        // 2) run animations in parallel
        var routines = new List<Coroutine>();
        foreach (var m in movers)
        {
            routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        }
        // wait for all to finish
        foreach (var c in routines) yield return c;

        // 3) array update (same logic as before), no transforms moved here (already positioned)
        if (dir == Direction.Left)
        {
            for (int x = fullSize - 1; x >= 1; x--)
            {
                grid[x, row] = grid[x - 1, row];
                occupants[x, row] = occupants[x - 1, row];
                gridData[x, row] = gridData[x - 1, row];

                // playerXY 更新
                if (gridData[x, row] == 3)
                {
                    playerX = x;
                    playerY = row;
                }
            }

            grid[0, row] = null;
            occupants[0, row] = null;
            gridData[0, row] = 0;
        }
        else // Right
        {
            for (int x = 0; x <= fullSize - 2; x++)
            {
                grid[x, row] = grid[x + 1, row];
                occupants[x, row] = occupants[x + 1, row];
                gridData[x, row] = gridData[x + 1, row];

                if (gridData[x, row] == 3)
                {
                    playerX = x;
                    playerY = row;
                }
            }

            grid[fullSize - 1, row] = null;
            occupants[fullSize - 1, row] = null;
            gridData[fullSize - 1, row] = 0;
        }

        // update playerObj reference
        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
        {
            playerObj = occupants[playerX, playerY];
        }

        yield break;
    }

    // 列アニメ + 配列更新
    private IEnumerator SlideColumnAnimated(int col, Direction dir, float duration)
    {
        var movers = new List<(GameObject go, Vector3 from, Vector3 to)>();

        if (dir == Direction.Up)
        {
            // 上から追加 -> 下へ移動（y -> y-1）
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
        else // Down
        {
            // 下から追加 -> 上へ移動 (y -> y+1)
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
            routines.Add(StartCoroutine(MoveTransformOverTime(m.go.transform, m.from, m.to, duration)));
        }
        foreach (var c in routines) yield return c;

        // 配列更新
        if (dir == Direction.Up)
        {
            for (int y = 1; y <= fullSize - 1; y++)
            {
                grid[col, y - 1] = grid[col, y];
                occupants[col, y - 1] = occupants[col, y];
                gridData[col, y - 1] = gridData[col, y];

                if (gridData[col, y - 1] == 3)
                {
                    playerX = col;
                    playerY = y - 1;
                }
            }
            grid[col, fullSize - 1] = null;
            occupants[col, fullSize - 1] = null;
            gridData[col, fullSize - 1] = 0;
        }
        else
        {
            for (int y = fullSize - 2; y >= 0; y--)
            {
                grid[col, y + 1] = grid[col, y];
                occupants[col, y + 1] = occupants[col, y];
                gridData[col, y + 1] = gridData[col, y];

                if (gridData[col, y + 1] == 3)
                {
                    playerX = col;
                    playerY = y + 1;
                }
            }
            grid[col, 0] = null;
            occupants[col, 0] = null;
            gridData[col, 0] = 0;
        }

        // update playerObj reference
        if (playerX >= 0 && playerY >= 0 && playerX < fullSize && playerY < fullSize)
        {
            playerObj = occupants[playerX, playerY];
        }

        yield break;
    }

    // -----------------------------
    // 削除（押し出されたものを消す）
    // -----------------------------
    void RemoveBlocks()
    {
        foreach (var (x, y) in insertedBlocks)
        {
            if (x == 0)
            {
                int tx = fullSize - 1, ty = y;
                DestroyAt(tx, ty);
            }
            else if (x == coreSize + 1)
            {
                int tx = 0, ty = y;
                DestroyAt(tx, ty);
            }
            else if (y == 0)
            {
                int tx = x, ty = fullSize - 1;
                DestroyAt(tx, ty);
            }
            else if (y == coreSize + 1)
            {
                int tx = x, ty = 0;
                DestroyAt(tx, ty);
            }
        }
        insertedBlocks.Clear();
    }

    void DestroyAt(int x, int y)
    {
        if (grid[x, y] != null) Destroy(grid[x, y]);
        if (occupants[x, y] != null) Destroy(occupants[x, y]);
        grid[x, y] = null;
        occupants[x, y] = null;
        gridData[x, y] = 0;
    }

    // -----------------------------
    // 生成 / 消去 / 移動ユーティリティ
    // -----------------------------
    public void PlaceObjectOnBlock(int x, int y)
    {
        if (grid[x, y] == null) return;

        if (occupants[x, y] != null)
        {
            Destroy(occupants[x, y]);
            occupants[x, y] = null;
        }

        if (gridData[x, y] == 1)
            occupants[x, y] = Instantiate(itemPrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
        else if (gridData[x, y] == 2)
            occupants[x, y] = Instantiate(obstaclePrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
        else if (gridData[x, y] == 3)
            occupants[x, y] = Instantiate(playerPrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
    }

    public void ConsumeItemAt(int x, int y)
    {
        if (gridData[x, y] != 1) return;
        if (occupants[x, y] != null)
        {
            Destroy(occupants[x, y]);
            occupants[x, y] = null;
        }
        gridData[x, y] = 0;
    }

    // 注意: playerGO は通常 playerObj を渡す
    public void MovePlayerTo(int oldX, int oldY, int newX, int newY, GameObject playerGO)
    {
        if (occupants[newX, newY] != null && occupants[newX, newY] != playerGO)
        {
            Destroy(occupants[newX, newY]);
            occupants[newX, newY] = null;
        }

        if (occupants[oldX, oldY] == playerGO) occupants[oldX, oldY] = null;

        gridData[oldX, oldY] = 0;
        gridData[newX, newY] = 3;

        occupants[newX, newY] = playerGO;
        playerGO.transform.SetParent(grid[newX, newY].transform);
        playerGO.transform.localPosition = Vector3.up * 0.5f;

        // playerObj を最新に
        playerObj = playerGO;
    }

    // -----------------------------
    // ユーティリティ
    // -----------------------------
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
