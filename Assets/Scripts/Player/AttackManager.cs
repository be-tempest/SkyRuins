using UnityEngine;
using Enemies;
using Pool;

namespace Player
{
    public class AttackManager : MonoBehaviour
    {
        [SerializeField] private Board.BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;

        private int attackPosX = 0;
        private int attackPosY = 0;
        private Direction attackDir = Direction.Up;

        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        public void AttackPosSelect(int perX, int perY, Direction dir)
        {
            int posX = playerData.playerX + perX;
            int posY = playerData.playerY + perY;
            if (boardData.IsInsideCore(posX, posY))
            {
                guideManager.Clear();
                attackPosX = posX;
                attackPosY = posY;
                attackDir = dir;
                guideManager.Show(posX, posY, CommandState.AttackSelect);
            }
        }

        public bool PlayerAttack()
        {
            if (attackPosX == 0 && attackPosY == 0) return false;

            playerAnimation.SetDirection(attackDir);
            playerAnimation.PlayAttack();

            if (boardData.gridData[attackPosX, attackPosY] == boardData.enemyNum)
            {
                var enemyUnit = boardData.occupants[attackPosX, attackPosY].GetComponent<EnemyUnit>();
                enemyUnit.TakeDamage(playerData.attack, attackPosX, attackPosY);
                // if (enemyUnit.currentHP <= 0)
                // {
                //     boardData.occupants[attackPosX, attackPosY].Release();
                //     boardData.SetGridData(0, attackPosX, attackPosY);
                //     boardData.SetOccupants(null, attackPosX, attackPosY);
                // }
            }

            Clear();

            return true;
        }

        public void Clear()
        {
            attackPosX = 0;
            attackPosY = 0;
            guideManager.Clear();
        }
    }
}