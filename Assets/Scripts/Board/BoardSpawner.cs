using UnityEngine;
using System;
using Pool;


namespace Board
{
    public class BoardSpawner : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;
        [SerializeField] private Player.PlayerData playerData;
        [SerializeField] private Enemies.EnemyRegistry enemyRegistry;

        // 確率設定
        [Header("Probability")]
        [SerializeField] private float enemyProb = 0.20f;
        [SerializeField] private float itemProb = 0.05f;
        [SerializeField] private float obstacleBaseProb = 0.20f;
        [SerializeField] private float increasePerLevel = 0.02f;

        [Header("ObjectPool")]
        [SerializeField] private ObjectPool[] blockPools = new ObjectPool[4];
        [SerializeField] private ObjectPool playerPool;
        [SerializeField] private ObjectPool obstaclePool;
        [SerializeField] private ObjectPool[] enemyPools;
        [SerializeField] private ObjectPool[] itemPools;

        // 盤面生成
        public void InitBoard(Action gameOver)
        {
            playerData.OnGameOver += gameOver;
            boardData.Init();

            for (int x = 1; x <= boardData.coreSize; x++)
            {
                for (int y = 1; y <= boardData.coreSize; y++)
                {
                    SpawnBlock(x, y, 1);
                }
            }

            int center = (boardData.fullSize - 1) / 2;
            playerData.SetPlayerPos(center, center);
            boardData.SetGridData(boardData.playerNum, center, center);
            SpawnOccupant(boardData.playerNum, center, center);
        }

        // ブロック生成
        public void SpawnBlock(int x, int y, int level)
        {
            if (!boardData.IsValidIndex(x, y)) return;

            if (boardData.gridObjects[x, y] != null)
            {
                boardData.gridObjects[x, y].Release();
            }

            int index = UnityEngine.Random.Range(0, blockPools.Length);
            PooledObject obj = blockPools[index].GetPooledObject();

            obj.transform.position = new Vector3(x, 0f, y);
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
                boardData.occupants[x, y].Release();
            }

            PooledObject created = null;

            if (occupantNum == boardData.playerNum)
            {
                created = playerPool.GetPooledObject();
                playerData.SetPlayerObject(created);
                var playerAnim = created.GetComponent<Player.PlayerAnimation>();
                playerAnim.Initialize(playerData);
            }
            else if (occupantNum == boardData.obstacleNum)
            {
                created = obstaclePool.GetPooledObject();
            }
            else if (occupantNum == boardData.enemyNum)
            {
                int index = UnityEngine.Random.Range(0, enemyPools.Length);
                created = enemyPools[index].GetPooledObject();
                var enemyUnit = created.GetComponent<Enemies.EnemyUnit>();
                enemyUnit.Init(enemyRegistry, boardData);
            }
            else
            {
                int index = UnityEngine.Random.Range(0, itemPools.Length);
                created = itemPools[index].GetPooledObject();
            }

            created.transform.position = new Vector3(x, 0.5f, y);
            created.transform.SetParent(boardData.gridObjects[x, y]?.transform);
            boardData.SetOccupants(created, x, y);
        }

        int OccupantProbability(int level)
        {
            float obstacleProb = Mathf.Clamp01(obstacleBaseProb + (level - 1) * increasePerLevel);
            float r = UnityEngine.Random.value;
            if (r < obstacleProb) return boardData.obstacleNum;
            if (r < obstacleProb + enemyProb) return boardData.enemyNum;
            if (r < obstacleProb + enemyProb + itemProb) return boardData.itemNum;
            return 0;
        }
    }
}
