using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Enemies
{
    [CreateAssetMenu(menuName = "Enemy/Action/Attack")]
    public class EnemyAttack : EnemyActionDefinition
    {
        public List<Vector2Int> attackRanges;
        
        public override IEnumerator Execute(EnemyUnit enemy, Board.BoardData boardData, Player.PlayerData playerData)
        {
            // ここで前後左右を調べる
            // プレイヤーがいれば攻撃
            Vector2Int enemyPos = boardData.FindEnemyPos(enemy.gameObject);
            Vector2Int playerPos = new Vector2Int(playerData.playerX, playerData.playerY);

            foreach (var range in attackRanges)
            {
                if (enemyPos + range == playerPos)
                {
                    // 攻撃処理
                    Debug.Log($"{enemy.name} attacks Player!");
                    playerData.TakeDamage(enemy.Definition.attack);
                }
            }

            yield return null;
        }
    }
}