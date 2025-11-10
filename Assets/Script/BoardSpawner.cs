using UnityEngine;

public class BoardSpawner : MonoBehaviour, IBoardSpawner
{
    public BoardManager3 boardManager;
    public GridStorage gridStorage;

    [Header("Prefabs")]
    public GameObject blockPrefab;
    public GameObject obstaclePrefab;
    public GameObject playerPrefab;
    public GameObject[] itemPrefabs = new GameObject[3];  
    
    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[BoardSpawner] gridStorage not assigned!");
        if (boardManager == null) Debug.LogWarning("[BoardSpawner] boardManager not assigned!");
    }

    // ブロック生成
    public void SpawnBlockAt(int x, int y)
    {
        if (gridStorage == null) return;
        if (!gridStorage.IsValidIndex(x, y)) return;

        if (gridStorage.GridObjects[x, y] != null)
        {
            Destroy(gridStorage.GridObjects[x, y]);
            gridStorage.GridObjects[x, y] = null;
        }

        Vector3 pos = gridStorage.WorldPosition(x, y);
        var go = Instantiate(blockPrefab, pos, Quaternion.identity, this.transform);
        gridStorage.GridObjects[x, y] = go;
    }

    // 占有オブジェクト生成
    public GameObject SpawnOccupant(int cellType, int x, int y)
    {
        if (gridStorage == null) return null;
        if (!gridStorage.IsValidIndex(x, y)) return null;
        
        if (gridStorage.Occupants[x, y] != null)
        {
            Destroy(gridStorage.Occupants[x, y]);
            gridStorage.Occupants[x, y] = null;
        }

        GameObject created = null;
        Vector3 basePos = gridStorage.WorldPosition(x, y);

        if (cellType == boardManager.OBSTACLE)
        {
            created = Instantiate(obstaclePrefab, basePos + Vector3.up * 0.5f, Quaternion.identity, gridStorage.GridObjects[x,y]?.transform);
        }
        else if (cellType == boardManager.PLAYER)
        {
            created = Instantiate(playerPrefab, basePos + Vector3.up * 0.5f, Quaternion.identity, gridStorage.GridObjects[x,y]?.transform);
        }
        else if (cellType == boardManager.ITEM_A || cellType == boardManager.ITEM_B || cellType == boardManager.ITEM_C)
        {
            int idx = (cellType == boardManager.ITEM_A) ? 0 : (cellType == boardManager.ITEM_B) ? 1 : 2;
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
}