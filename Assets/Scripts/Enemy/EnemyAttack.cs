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
            Vector2Int enemyPos = boardData.FindEnemyPos(enemy.gameObject);
            Vector2Int playerPos = new Vector2Int(playerData.playerX, playerData.playerY);

            foreach (Direction dir in AllDirections)
            {
                foreach (var range in attackRanges)
                {
                    Vector2Int attackPos = RotatePos(range, dir);
                    if (enemyPos + attackPos == playerPos)
                    {
                        Debug.Log($"{enemy.name} attacks Player!");
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