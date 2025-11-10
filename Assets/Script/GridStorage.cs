using UnityEngine;

public class GridStorage : MonoBehaviour, IGridStorage
{
    // 盤面サイズ
    public int FullSize = 9;
    public int CoreSize = 7;

    // 盤面データ配列
    public GameObject[,] GridObjects { get; set; }
    public GameObject[,] Occupants { get; set; }
    public int[,] GridData { get; set; }

    void Awake()
    {
        int expected = CoreSize + 2;
        if (FullSize != expected)
        {
            FullSize = expected;
            Debug.Log($"[GridStorage] fullSize adjusted to coreSize + 2 = {FullSize}");
        }

        GridObjects = new GameObject[FullSize, FullSize];
        Occupants = new GameObject[FullSize, FullSize];
        GridData = new int[FullSize, FullSize];
    }

    // 範囲チェック
    public bool IsValidIndex(int x, int y)
    {
        return x >= 0 && y >= 0 && x < FullSize && y < FullSize;
    }

    public bool IsInsideCore(int x, int y)
    {
        return x >= 1 && y >= 1 && x <= CoreSize && y <= CoreSize;
    }

    public Vector3 WorldPosition(int x, int y)
    {
        return new Vector3(x, 0f, y);
    }

    public void SetCell(int x, int y, int value, GameObject obj = null)
    {
        if (!IsValidIndex(x, y)) return;
        GridData[x, y] = value;
        Occupants[x, y] = obj;
    }

    public int GetCell(int x, int y)
    {
        if (!IsValidIndex(x, y)) return -1;
        return GridData[x, y];
    }

    public void ClearCell(int x, int y)
    {
        if (!IsValidIndex(x, y)) return;
        GridData[x, y] = 0;
        if (Occupants[x, y] != null)
        {
            Destroy(Occupants[x, y]);
            Occupants[x, y] = null;
        }
    }
}
