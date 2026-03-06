using UnityEngine;
using SkyRuins.Board;
using SkyRuins.Enemies;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // プレイヤーの攻撃を管理するクラス

    public class AttackManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;

        private int attackPosX = 0; // 攻撃位置のX座標
        private int attackPosY = 0; // 攻撃位置のY座標
        private Direction attackDir = Direction.Up; // 攻撃の向き

        // アニメーションの設定関数
        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        // 攻撃位置を選択する関数
        public void AttackPosSelect(int perX, int perY, Direction dir)
        {
            int posX = playerData.playerX + perX;
            int posY = playerData.playerY + perY;

            // 攻撃位置がコアの範囲内であれば、攻撃位置として設定
            if (boardData.IsInsideCore(posX, posY))
            {
                guideManager.Clear();
                attackPosX = posX;
                attackPosY = posY;
                attackDir = dir;
                guideManager.Show(posX, posY, CommandState.AttackSelect);
            }
        }

        // プレイヤーの攻撃を実行する関数
        public bool PlayerAttack()
        {
            if (attackPosX == 0 && attackPosY == 0) return false;

            playerAnimation.SetDirection(attackDir);
            playerAnimation.PlayAttack();

            // 攻撃位置に敵がいる場合、ダメージを与える
            if (boardData.gridData[attackPosX, attackPosY] == boardData.enemyNum)
            {
                var enemyUnit = boardData.occupants[attackPosX, attackPosY].GetComponent<EnemyUnit>();
                enemyUnit.TakeDamage(playerData.attack, attackPosX, attackPosY);
            }

            Clear();

            return true;
        }

        // 攻撃位置とガイドをクリアする関数
        public void Clear()
        {
            attackPosX = 0;
            attackPosY = 0;
            guideManager.Clear();
        }
    }
}