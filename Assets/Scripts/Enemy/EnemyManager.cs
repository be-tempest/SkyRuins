using UnityEngine;
using System.Collections;
using System;

namespace Enemies
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private Board.BoardData boardData;
        [SerializeField] private Player.PlayerData playerData;
        [SerializeField] private EnemyRegistry enemyRegistry;

        private Action end;
        private Action over;

        public void StartEnemyTurn(Action turnEnd, Action gameOver)
        {
            end = turnEnd;
            over = gameOver;
            StartCoroutine(ActionEnemies());
        }

        public IEnumerator ActionEnemies()
        {
            foreach (var enemy in enemyRegistry.enemies)
            {
                foreach (var action in enemy.Definition.actions)
                {
                    yield return action.Execute(enemy, boardData, playerData);
                    // yield return new WaitForSeconds(0.5f);
                }
            }

            end?.Invoke();
        }
    }
}