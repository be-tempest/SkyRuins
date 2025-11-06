// BoardSpawner.cs
using UnityEngine;

/// <summary>
/// ブロック（床）と占有オブジェクト（障害物/プレイヤー/アイテム）の生成を担当するコンポーネント。
/// GridStorage を参照して GridObjects / Occupants を直接更新します。
/// </summary>
public class BoardSpawner : MonoBehaviour, IBoardSpawner
{
    [Header("Prefabs")]
    public GameObject blockPrefab;
    public GameObject obstaclePrefab;
    public GameObject playerPrefab;
    public GameObject[] itemPrefabs = new GameObject[3];

    [Header("References")]
    public GridStorage gridStorage; // シーンの GridStorage を割り当てる

    // cellType constants must match BoardManager usage
    private const int PLAYER = 1;
    private const int OBSTACLE = 2;
    private const int ITEM_A = 3;
    private const int ITEM_B = 4;
    private const int ITEM_C = 5;

    void Awake()
    {
        if (gridStorage == null)
        {
            Debug.LogError("[BoardSpawner] gridStorage is not assigned!");
        }
    }

    // 床を生成して GridObjects に登録
    public void SpawnBlockAt(int x, int y)
    {
        if (gridStorage == null) return;
        if (!gridStorage.IsValidIndex(x, y)) return;

        // 既存があれば破棄して上書き（安全性を確保）
        if (gridStorage.GridObjects[x, y] != null)
        {
            Destroy(gridStorage.GridObjects[x, y]);
            gridStorage.GridObjects[x, y] = null;
        }

        Vector3 pos = gridStorage.WorldPosition(x, y);
        var go = Instantiate(blockPrefab, pos, Quaternion.identity, this.transform);
        gridStorage.GridObjects[x, y] = go;
    }

    // cellType に応じた占有オブジェクトを生成して Occupants に登録（既存は置換）
    public GameObject SpawnOccupant(int cellType, int x, int y)
    {
        if (gridStorage == null) return null;
        if (!gridStorage.IsValidIndex(x, y)) return null;
        // 既存 occupant があれば破棄
        if (gridStorage.Occupants[x, y] != null)
        {
            Destroy(gridStorage.Occupants[x, y]);
            gridStorage.Occupants[x, y] = null;
        }

        GameObject created = null;
        Vector3 basePos = Vector3.zero;
        if (gridStorage.GridObjects[x, y] != null) basePos = gridStorage.GridObjects[x, y].transform.position;
        else basePos = gridStorage.WorldPosition(x, y);

        if (cellType == OBSTACLE)
        {
            created = Instantiate(obstaclePrefab, basePos + Vector3.up * 0.5f, Quaternion.identity, gridStorage.GridObjects[x,y]?.transform);
        }
        else if (cellType == PLAYER)
        {
            created = Instantiate(playerPrefab, basePos + Vector3.up * 0.5f, Quaternion.identity, gridStorage.GridObjects[x,y]?.transform);
        }
        else if (cellType == ITEM_A || cellType == ITEM_B || cellType == ITEM_C)
        {
            int idx = (cellType == ITEM_A) ? 0 : (cellType == ITEM_B) ? 1 : 2;
            if (itemPrefabs != null && itemPrefabs.Length > idx && itemPrefabs[idx] != null)
            {
                created = Instantiate(itemPrefabs[idx], basePos + Vector3.up * 0.5f, Quaternion.identity, gridStorage.GridObjects[x,y]?.transform);
            }
            else
            {
                Debug.LogWarning($"[BoardSpawner] itemPrefabs[{idx}] not set.");
                created = null;
            }
        }

        gridStorage.Occupants[x, y] = created;
        return created;
    }

    public void RemoveOccupantAt(int x, int y)
    {
        if (gridStorage == null) return;
        if (!gridStorage.IsValidIndex(x, y)) return;
        if (gridStorage.Occupants[x, y] != null)
        {
            Destroy(gridStorage.Occupants[x, y]);
            gridStorage.Occupants[x, y] = null;
        }
        // gridStorage.GridData は呼び出し元がクリアする想定（BoardManager で行う）
    }
}