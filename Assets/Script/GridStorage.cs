// GridStorage.cs
using UnityEngine;

/// <summary>
/// Grid / Occupants / GridData を一元管理するコンポーネント。
/// BoardManager などから参照して使います。
/// Attach this script to an empty GameObject in the Scene and set coreSize/fullSize there,
/// or leave defaults (coreSize=7, fullSize=9).
/// </summary>
public class GridStorage : MonoBehaviour, IGridStorage
{
    [Header("Grid Size (coreSize = playable area width)")]
    [Tooltip("coreSize: playable core (例: 7)。fullSize は外枠を含む (例: 9)。")]
    public int coreSize = 7;

    // fullSize は coreSize + 2 (左右外枠/上下外枠)
    public int FullSize => fullSize;
    public int CoreSize => coreSize;

    [SerializeField]
    private int fullSize = 9; // デフォルトは coreSize + 2。Awake で整合チェックします。

    // データ配列
    public GameObject[,] GridObjects { get; set; }
    public GameObject[,] Occupants { get; set; }
    public int[,] GridData { get; set; }

    void Awake()
    {
        // 整合性: coreSize に合わせて fullSize を自動セット（もし不整合があれば修正）
        int expected = coreSize + 2;
        if (fullSize != expected)
        {
            fullSize = expected;
            Debug.Log($"[GridStorage] fullSize adjusted to coreSize + 2 = {fullSize}");
        }

        GridObjects = new GameObject[fullSize, fullSize];
        Occupants = new GameObject[fullSize, fullSize];
        GridData = new int[fullSize, fullSize];
    }

    // index 範囲チェック（0..fullSize-1）
    public bool IsValidIndex(int x, int y)
    {
        return x >= 0 && y >= 0 && x < fullSize && y < fullSize;
    }

    // core 内（1..coreSize）
    public bool IsInsideCore(int x, int y)
    {
        return x >= 1 && y >= 1 && x <= coreSize && y <= coreSize;
    }

    // ワールド座標変換（グリッドインデックス -> ワールド）
    // ここは既存の座標系に合わせているので、必要なら修正してください。
    public Vector3 WorldPosition(int x, int y)
    {
        return new Vector3(x, 0f, y);
    }

    public void SetCell(int x, int y, int value, GameObject obj = null)
    {
        if (!IsValidIndex(x, y)) return;
        GridData[x, y] = value;
        Occupants[x, y] = obj;
        // GridObjects は床 (block) など別管理で SpawnBlock がセットする想定
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
