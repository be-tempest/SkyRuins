using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject itemPrefab;
    public GameObject obstaclePrefab;

    public int coreSize = 7; // 実際の盤面
    private int fullSize = 9; // 外枠込み (0～8)
    private GameObject[,] grid;
    private int[,] gridData;

    public enum GamePhase { Add, PlayerMove, Slide, Remove }
    public GamePhase phase = GamePhase.Add;

    private int insertRow = -1; // どの行に追加したか

    void Start()
    {
        grid = new GameObject[fullSize, fullSize];
        gridData = new int[fullSize, fullSize];
        InitBoard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && phase == GamePhase.Add)
        {
            AddBlockLeft();
            phase = GamePhase.PlayerMove;
        }
        else if (Input.GetKeyDown(KeyCode.P) && phase == GamePhase.PlayerMove)
        {
            Debug.Log("Player Move");
            phase = GamePhase.Slide;
        }
        else if (Input.GetKeyDown(KeyCode.S) && phase == GamePhase.Slide)
        {
            SlideRow(insertRow);
            phase = GamePhase.Remove;
        }
        else if (Input.GetKeyDown(KeyCode.R) && phase == GamePhase.Remove)
        {
            RemoveRightmost(insertRow);
            phase = GamePhase.Add;
        }
    }

    void InitBoard()
    {
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                SpawnBlock(x, y);
                gridData[x, y] = Random.Range(0, 3); // アイテム/障害物
                PlaceObjectOnBlock(x, y);
            }
        }
    }

    void SpawnBlock(int x, int y)
    {
        Vector3 pos = new Vector3(x, 0, y);
        grid[x, y] = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
    }

    // 外枠(0列)に追加
    void AddBlockLeft()
    {
        insertRow = Random.Range(1, coreSize + 1); // 1～7 の中から選ぶ
        SpawnBlock(0, insertRow);
        gridData[0, insertRow] = Random.Range(0, 3);
        PlaceObjectOnBlock(0, insertRow);

        Debug.Log($"Added block at (0,{insertRow})");
    }

    void SlideRow(int row)
    {
        for (int x = coreSize; x >= 0; x--)
        {
            grid[x + 1, row] = grid[x, row];
            gridData[x + 1, row] = gridData[x, row];

            if (grid[x + 1, row] != null)
            {
                grid[x + 1, row].transform.position = new Vector3(x + 1, 0, row);
            }
        }
        Debug.Log($"Slid row {row}");
    }

    void RemoveRightmost(int row)
    {
        int x = coreSize + 1; // 8列
        if (grid[x, row] != null)
        {
            Destroy(grid[x, row]);
            grid[x, row] = null;
            gridData[x, row] = 0;
        }
        Debug.Log($"Removed block at ({x},{row})");
    }

    void PlaceObjectOnBlock(int x, int y)
    {
        if (grid[x, y] == null) return;

        foreach (Transform child in grid[x, y].transform)
            Destroy(child.gameObject);

        if (gridData[x, y] == 1)
            Instantiate(itemPrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
        else if (gridData[x, y] == 2)
            Instantiate(obstaclePrefab, grid[x, y].transform.position + Vector3.up * 0.5f, Quaternion.identity, grid[x, y].transform);
    }
}
