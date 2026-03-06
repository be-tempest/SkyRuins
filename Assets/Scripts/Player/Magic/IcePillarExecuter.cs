using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Enemies;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // アイスピラーの実行クラス

    public class IcePillarExecuter : MagicExecuter
    {
        // 魔法の向きを選択する関数
        public override void DirSelect(Direction dir)
        {
            magicPos.Clear();
            guideManager.Clear();
            attackDir = dir;

            // 向きに応じて魔法の効果範囲の座標を回転させ、ガイドを表示する
            foreach (var pos in magicDef.magicRange)
            {
                Vector2Int rotatePos = RotatePos(pos, dir);
                int posX = rotatePos.x + playerData.playerX;
                int posY = rotatePos.y + playerData.playerY;

                if (boardData.IsInsideCore(posX, posY))
                {
                    guideManager.Show(posX, posY, CommandState.MagicSelect);
                    magicPos.Add(new Vector2Int(posX, posY));
                }
            }
        }

        // 魔法の効果を実行する関数
        public override IEnumerator MagicExecute()
        {
            var pos = new Vector3(playerData.playerX, 0.5f, playerData.playerY);
            var rot = GetRotation(attackDir);
            GameObject effect = Instantiate(magicDef.effectPrefab, pos, rot);  // 魔法のエフェクトを生成
            Destroy(effect, 1.5f);

            yield return new WaitForSeconds(1.5f);

            // 魔法の効果範囲内の敵にダメージを与える
            foreach (var targetPos in magicPos)
            {
                if (boardData.gridData[targetPos.x, targetPos.y] == boardData.enemyNum)
                {
                    var enemyUnit = boardData.occupants[targetPos.x, targetPos.y].GetComponent<EnemyUnit>();
                    enemyUnit.TakeDamage(magicDef.power, targetPos.x, targetPos.y);
                    Debug.Log("IcePillarで" + magicDef.power + "のダメージを与えた");
                }
            }
        }
    }
}
