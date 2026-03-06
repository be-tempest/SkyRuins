using UnityEngine;
using System;
using System.Collections.Generic;

namespace SkyRuins.Player
{
    // プレイヤーのデータを管理するクラス

    public class PlayerData : MonoBehaviour
    {
        private PooledObject _playerObject; // プレイヤーのゲームオブジェクト
        public PooledObject playerObject => _playerObject;

        private int _playerX; // プレイヤーのX座標
        public int playerX => _playerX;

        private int _playerY; // プレイヤーのY座標
        public int playerY => _playerY;

        private int _maxHP = 100; // プレイヤーの最大HP
        public int maxHP => _maxHP;

        private int _currentHP; // プレイヤーの現在のHP
        public int currentHP => _currentHP;

        private int _maxMP = 100; // プレイヤーの最大MP
        public int maxMP => _maxMP;

        private int _currentMP; // プレイヤーの現在のMP
        public int currentMP => _currentMP;

        private int _attack = 5; // プレイヤーの攻撃力
        public int attack => _attack;

        public event Action OnInfoChanged; // プレイヤーの情報が変更されたときに呼び出されるイベント
        public event Action OnPlayerDamage; // プレイヤーがダメージを受けたときに呼び出されるイベント
        public event Action OnPlayerDead; // プレイヤーが死亡したときに呼び出されるイベント
        public event Action OnGameOver; // プレイヤーがゲームオーバーを要求したときに呼び出されるイベント

        public List<MagicDefinition> magicList = new(); // プレイヤーが持っている魔法のリスト
        public List<ItemDefinition> itemList = new(); // プレイヤーが持っているアイテムのリスト

        private void Awake()
        {
            _currentHP = _maxHP;
            _currentMP = _maxMP;
            OnInfoChanged?.Invoke();
        }

        // 盤面上のプレイヤーのゲームオブジェクトを設定する関数
        public void SetPlayerObject(PooledObject obj)
        {
            _playerObject = obj;
        }

        // プレイヤーの位置を設定する関数
        public void SetPlayerPos(int x, int y)
        {
            _playerX = x;
            _playerY = y;
        }

        // プレイヤーがダメージを受ける関数
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

        // プレイヤーがゲームオーバーを要求する関数
        public void RequestGameOver()
        {
            Debug.Log("Player requested Game Over.");
            OnGameOver?.Invoke();
        }

        // プレイヤーがHPを回復する関数
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

        // プレイヤーがMPを使用する関数
        public void UseMP(int mp)
        {
            _currentMP -= mp;
            OnInfoChanged?.Invoke();
            Debug.Log($"Player MP {_currentMP}!");
        }

        // プレイヤーがMPを回復する関数
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

        // プレイヤーが魔法を消費する関数
        public void UseItem(int index)
        {
            itemList.RemoveAt(index);
        }
    }
}
