using UnityEngine;
using Pool;

namespace Data
{
    public class BoardData : MonoBehaviour
    {
        // 盤面サイズ
        [SerializeField] private int _fullSize = 9;
        public int fullSize => _fullSize;

        [SerializeField] private int _coreSize = 7;
        public int coreSize => _coreSize;

        // 盤面データ配列
        private int[,] _gridData;
        public int[,] gridData => _gridData;

        private PooledObject[,] _gridObjects;
        public PooledObject[,] gridObjects => _gridObjects;

        private PooledObject[,] _occupants;
        public PooledObject[,] occupants => _occupants;

        // GridData 情報
        private int _playerNum = 1;
        public int playerNum => _playerNum;

        private int _obstacleNum = 2;
        public int obstacleNum => _obstacleNum;

        private int _enemyNum = 3;
        public int enemyNum => _enemyNum;

        private int _itemNum = 4;
        public int itemNum => _itemNum;

        public void Init()
        {
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

        // 範囲チェック
        public bool IsValidIndex(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _fullSize && y < _fullSize;
        }

        public bool IsInsideCore(int x, int y)
        {
            return x >= 1 && y >= 1 && x <= _coreSize && y <= _coreSize;
        }

        public void SetGridData(int num, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _gridData[x, y] = num;
        }

        public void SetGridObjects(PooledObject obj, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _gridObjects[x, y] = obj;
        }

        public void SetOccupants(PooledObject obj, int x, int y)
        {
            if (!IsValidIndex(x, y)) return;
            _occupants[x, y] = obj;
        }
    }
}
