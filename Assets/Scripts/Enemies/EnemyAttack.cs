using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Common;

namespace SkyRuins.Enemies
{
    [CreateAssetMenu(menuName = "Enemy/Action/Attack")]
    
    // 敵の攻撃行動を定義するクラス

    public class EnemyAttack : EnemyActionDefinition
    {
        public List<Vector2Int> attackRanges; // 攻撃範囲

        // 敵の攻撃行動を実行する関数
        public override IEnumerator Execute(EnemyUnit enemy, Board.BoardData boardData, Player.PlayerData playerData)
        {
            Vector2Int enemyPos = boardData.FindEnemyPos(enemy.gameObject); // 敵の現在位置を取得
            Vector2Int playerPos = new Vector2Int(playerData.playerX, playerData.playerY); // プレイヤーの現在位置を取得

            // 全方向に対して攻撃範囲を回転させ、プレイヤーが攻撃範囲内にいるかをチェック
            foreach (Direction dir in AllDirections)
            {
                foreach (var range in attackRanges)
                {
                    Vector2Int attackPos = RotatePos(range, dir);
                    if (enemyPos + attackPos == playerPos)
                    {
                        enemy.enemyAnimation.SetDirection(dir);
                        enemy.enemyAnimation.PlayAttack();
                        var playerAnimation = playerData.playerObject.GetComponent<Player.PlayerAnimation>();
                        playerAnimation.SetDirection(OppositeDirection(dir));
                        yield return new WaitForSeconds(0.5f);
                        AudioManager.Instance.PlaySE(SEType.Attack);
                        playerData.TakeDamage(enemy.Definition.attack);
                        yield return new WaitForSeconds(1.0f);
                    }
                }
            }

            yield return null;
        }

        // 方向を反転させる関数
        private Direction OppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return Direction.Down;

                case Direction.Down:
                    return Direction.Up;

                case Direction.Left:
                    return Direction.Right;

                case Direction.Right:
                    return Direction.Left;

                default:
                    return dir;
            }
        }
    }
}