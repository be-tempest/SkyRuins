using UnityEngine;
using System;
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

        private int _maxHP = 100;
        public int maxHP => _maxHP;

        private int _currentHP;
        public int currentHP => _currentHP;

        private int _maxMP = 100;
        public int maxMP => _maxMP;

        private int _currentMP;
        public int currentMP => _currentMP;

        private int _attack = 5;
        public int attack => _attack;

        public event Action OnInfoChanged; 
        public event Action OnPlayerDamage;
        public event Action OnPlayerDead;
        public event Action OnGameOver;

        public List<MagicDefinition> magicList = new();
        public List<ItemDefinition> itemList = new();

        private void Awake()
        {
            _currentHP = _maxHP;
            _currentMP = _maxMP;
            OnInfoChanged?.Invoke();
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
                OnPlayerDead?.Invoke();
            }
            else
            {
                OnPlayerDamage?.Invoke();
            }

            OnInfoChanged?.Invoke();
            Debug.Log($"Player HP {_currentHP}!");
        }

        public void RequestGameOver()
        {
            Debug.Log("Player requested Game Over.");
            OnGameOver?.Invoke();
        }

        public void Heal(int heal)
        {
            _currentHP += heal;
            if (_currentHP > _maxHP)
            {
                _currentHP = _maxHP;
            }
            OnInfoChanged?.Invoke();
            Debug.Log($"Player HP {_currentHP}!");
        }

        public void UseMP(int mp)
        {
            _currentMP -= mp;
            OnInfoChanged?.Invoke();
            Debug.Log($"Player MP {_currentMP}!");
        }

        public void RecoverMP(int mp)
        {
            _currentMP += mp;
            if (_currentMP > _maxMP)
            {
                _currentMP = _maxMP;
            }
            OnInfoChanged?.Invoke();
            Debug.Log($"Player MP {_currentMP}!");
        }

        public void UseItem(int index)
        {
            itemList.RemoveAt(index);
        }
    }
}
