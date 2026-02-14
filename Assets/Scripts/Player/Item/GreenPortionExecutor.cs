using UnityEngine;
using System.Collections;

namespace Player
{
    public class GreenPortionExecutor : ItemExecutor
    {
        public override void PosSelect(Direction dir)
        {
            guideManager.Clear();
            guideManager.Show(playerData.playerX, playerData.playerY, CommandState.ItemSelect);
        }

        public override bool Check()
        {
            return playerData.currentHP < playerData.maxHP;
        }

        public override IEnumerator ItemExecute()
        {
            guideManager.Clear();
            var pos = new Vector3(playerData.playerX, 0.5f, playerData.playerY);
            GameObject effect = Instantiate(itemDef.effectPrefab, pos, Quaternion.identity);
            Destroy(effect, 1.5f);

            yield return new WaitForSeconds(1.5f);

            playerData.Heal(itemDef.power);
            playerData.itemList.RemoveAt(index);
            
            Debug.Log("緑ポーションで" + itemDef.power + "HP回復した");
        }

    }
}
