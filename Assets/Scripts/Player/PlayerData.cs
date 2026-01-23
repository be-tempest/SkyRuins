using UnityEngine;
using System.Collections.Generic;
using Pool;

namespace Player
{
    public class PlayerData : MonoBehaviour
    {
        private PooledObject _playerObject;
        public PooledObject playerObject => _playerObject;

        private int _playerX;
        public int playerX => _playerX;

        private int _playerY;
        public int playerY => _playerY;

        private int _maxHP = 10;
        public int maxHP => _maxHP;

        private int _currentHP;
        public int currentHP => _currentHP;

        private int _attack = 5;
        public int attack => _attack;

        private bool _isShild;
        public bool isShild => _isShild;

        private int[] _itemsCount = new int[3];
        public int[] itemsCount => _itemsCount;

        public List<ItemDefinition> itemList = new();

        private void Awake()
        {
            _currentHP = _maxHP;
        }

        public void SetPlayerObject(PooledObject obj)
        {
            _playerObject = obj;
        }

        public void SetPlayerPos(int x, int y)
        {
            _playerX = x;
            _playerY = y;
        }

        public void TakeDamage(int damage)
        {
            _currentHP -= damage;
            if (_currentHP <= 0)
            {
                _currentHP = 0;
                // GameOver通知など
            }
            Debug.Log($"Player HP {_currentHP}!");
        }

        // public void SetShild(bool act)
        // {
        //     _isShild = act;
        // }

        // public void AddItemCount(int idx)
        // {
        //     _itemsCount[idx]++;
        // }
    }
}
