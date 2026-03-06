using UnityEngine;
using System.Collections;
using System;
using SkyRuins.Board;
using SkyRuins.Player;

namespace SkyRuins.Enemies
{
    // 敵の行動を管理するクラス

    public class EnemyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private EnemyRegistry enemyRegistry;

        private Action end; // ターン終了コールバック

        // Enemyターン開始
        public void StartEnemyTurn(Action turnEnd)
        {
            end = turnEnd;
            StartCoroutine(ActionEnemies());
        }

        // 敵の行動を順番に実行
        public IEnumerator ActionEnemies()
        {
            foreach (var enemy in enemyRegistry.enemies)
            {
                // 複数の行動を持つ敵用
                foreach (var action in enemy.Definition.actions)
                {
                    yield return action.Execute(enemy, boardData, playerData);
                }
            }

            end?.Invoke();
        }
    }
}