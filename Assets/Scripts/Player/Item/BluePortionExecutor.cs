using UnityEngine;
using System.Collections;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // 青ポーションの実行クラス

    public class BluePortionExecutor : ItemExecutor
    {
        // 位置選択の処理
        // 青ポーションはプレイヤーしか選択できないため、ガイドを表示するだけ
        public override void PosSelect(Direction dir)
        {
            guideManager.Clear();
            guideManager.Show(playerData.playerX, playerData.playerY, CommandState.ItemSelect);
        }

        // アイテムが使用可能かのチェック
        // 青ポーションはMPが最大でないときに使用可能
        public override bool Check()
        {
            return playerData.currentMP < playerData.maxMP;
        }

        // アイテムの効果を実行するコルーチン
        public override IEnumerator ItemExecute()
        {
            guideManager.Clear();
            var pos = new Vector3(playerData.playerX, 0.5f, playerData.playerY);
            GameObject effect = Instantiate(itemDef.effectPrefab, pos, Quaternion.identity);
            Destroy(effect, 1.5f);

            yield return new WaitForSeconds(1.5f);

            playerData.RecoverMP(itemDef.power);
            playerData.itemList.RemoveAt(index);

            Debug.Log("青ポーションで" + itemDef.power + "MP回復した");
        }

    }
}
