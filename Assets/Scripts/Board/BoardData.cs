using UnityEngine;

namespace SkyRuins.Board
{
    // 盤面の情報を管理
    // 盤面サイズ、データ配列、更新関数などを定義

    public class BoardData : MonoBehaviour
    {
        // 盤面サイズ (追加位置も含む)
        [SerializeField] private int _fullSize = 9;
        public int fullSize => _fullSize;

        // コアサイズ（有効な盤面サイズ）
        [SerializeField] private int _coreSize = 7;
        public int coreSize => _coreSize;

        // 盤面データ配列
        private int[,] _gridData;
        public int[,] gridData => _gridData;

        // 盤面ブロックオブジェクト配列
        private PooledObject[,] _gridObjects;
        public PooledObject[,] gridObjects => _gridObjects;

        // 盤面占有オブジェクト配列
        private PooledObject[,] _occupants;
        public PooledObject[,] occupants => _occupants;

        // GridData 情報
        // 0:空 1:プレイヤー 2:障害物 3:敵 4:アイテム
        private int _playerNum = 1;
        public int playerNum => _playerNum;

        private int _obstacleNum = 2;
        public int obstacleNum => _obstacleNum;

        private int _enemyNum = 3;
        public int enemyNum => _enemyNum;

        private int _itemNum = 4;
        public int itemNum => _itemNum;

        // 盤面情報の初期化
        public void Init()
        {
            // fullSizeがcoreSize + 2とならない場合は自動調整
            int expected = _coreSize + 2;

            if (_fullSize != expected)
            {
                _fullSize = expected;
                Debug.Log($"[GridStorage] fullSize adjusted to coreSize + 2 = {fullSize}");
            }

            _gridObjects = new PooledObject[_fullSize, _fullSize];
            _occupants = new PooledObject[_fullSize, _fullSize];
            _gridData = new int[_fullSize, _fullSize];
        }

        // 盤面内かチェック
        public bool IsValidIndex(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _fullSize && y < _fullSize;
        }

        // コア内かチェック
        public bool IsInsideCore(int x, int y)
        {
            return x >= 1 && y >= 1 && x <= _coreSize && y <= _coreSize;
        }

        // 盤面データの更新関数
        public void SetGridData(int num, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _gridData[x, y] = num;
        }

        // 盤面オブジェクトの更新関数
        public void SetGridObjects(PooledObject obj, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _gridObjects[x, y] = obj;
        }

        // 盤面占有オブジェクトの更新関数
        public void SetOccupants(PooledObject obj, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _occupants[x, y] = obj;
        }

        // ターゲットの敵の位置を返す関数
        public Vector2Int FindEnemyPos(GameObject target)
        {
            for (int x = 0; x < _fullSize; x++)
            {
                for (int y = 0; y < _fullSize; y++)
                {
                    if (_occupants[x, y] != null && _occupants[x, y].gameObject == target)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            Debug.LogWarning($"[BoardData] Target {target.name} not found on board.");
            return new Vector2Int(99, 99); // 見つからない場合は異常値を返す
        }
    }
}
