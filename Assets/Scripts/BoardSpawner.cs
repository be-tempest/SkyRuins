using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private PlayerData playerData;

    // 確率設定
    [Header("Probability")]
    [SerializeField] private float itemProb = 0.05f;
    [SerializeField] private float obstacleBaseProb = 0.20f;
    [SerializeField] private float increasePerLevel = 0.02f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 盤面生成
    public void InitBoard()
    {
        boardData.Init();

        for (int x = 1; x <= boardData.coreSize; x++)
        {
            for (int y = 1; y <= boardData.coreSize; y++)
            {
                SpawnBlock(x, y, 1);
            }
        }

        int center = (boardData.fullSize - 1) / 2;
        playerData.SetPlayer(center, center);
        boardData.SetGridData(boardData.playerNum, center, center);
        SpawnOccupant(boardData.playerNum, center, center);
        

    }

    // ブロック生成
    public void SpawnBlock(int x, int y, int level)
    {
        if (!boardData.IsValidIndex(x, y)) return;

        if (boardData.gridObjects[x, y] != null)
        {
            Destroy(boardData.gridObjects[x, y]);
            boardData.SetGridObjects(null, x, y);
        }

        Vector3 pos = new Vector3(x, 0f, y);
        var obj = Instantiate(boardData.blockPrefab, pos, Quaternion.identity, this.transform);
        
        boardData.SetGridObjects(obj, x, y);
        boardData.SetGridData(OccupantProbability(level), x, y);
        if (boardData.gridData[x, y] != 0)
        {
            SpawnOccupant(boardData.gridData[x, y], x, y);
        }
    }

    // 占有オブジェクト生成
    void SpawnOccupant(int occupantNum, int x, int y)
    {
        if (boardData.occupants[x, y] != null)
        {
            Destroy(boardData.occupants[x, y]);
            boardData.SetOccupants(null, x, y);
        }

        GameObject created = null;
        Vector3 pos =  new Vector3(x, 0f, y);

        if (occupantNum == boardData.obstacleNum)
        {
            created = Instantiate(boardData.obstaclePrefab, pos + Vector3.up * 0.5f, Quaternion.identity, boardData.gridObjects[x, y]?.transform);
        }
        else if (occupantNum == boardData.playerNum)
        {
            created = Instantiate(playerData.playerPrefab, pos + Vector3.up * 0.5f, Quaternion.identity, boardData.gridObjects[x, y]?.transform);
        }
        else if (occupantNum == boardData.item1Num || occupantNum == boardData.item2Num || occupantNum == boardData.item3Num)
        {
            int idx = (occupantNum == boardData.item1Num) ? 0 : (occupantNum == boardData.item2Num) ? 1 : 2;
            if (boardData.itemPrefabs != null && boardData.itemPrefabs.Length > idx && boardData.itemPrefabs[idx] != null)
            {
                created = Instantiate(boardData.itemPrefabs[idx], pos + Vector3.up * 0.5f, Quaternion.identity, boardData.gridObjects[x, y]?.transform);
            }
        }

        boardData.SetOccupants(created, x, y);
    }
    
    int OccupantProbability(int level)
    {
        float obstacleProb = Mathf.Clamp01(obstacleBaseProb + (level - 1) * increasePerLevel);
        float r = UnityEngine.Random.value;
        if (r < itemProb) return boardData.item1Num;
        if (r < itemProb * 2f) return boardData.item2Num;
        if (r < itemProb * 3f) return boardData.item3Num;
        if (r < itemProb * 3f + obstacleProb) return boardData.obstacleNum;
        return 0;
    }
}
