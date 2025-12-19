using UnityEngine;

namespace Data
{
    public class PlayerData : MonoBehaviour
    {
        private int _playerX;
        public int playerX => _playerX;

        private int _playerY;
        public int playerY => _playerY;

        private bool _isShild;
        public bool isShild => _isShild;

        private int[] _itemsCount = new int[3];
        public int[] itemsCount => _itemsCount;

        public void SetPlayer(int x, int y)
        {
            _playerX = x;
            _playerY = y;
        }

        public void SetShild(bool act)
        {
            _isShild = act;
        }

        public void AddItemCount(int idx)
        {
            _itemsCount[idx]++;
        }
    }
}
