using UnityEngine;
using System.Collections;
using SkyRuins.Board;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // アイテムの実行を管理するクラス

    public class ItemManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;

        private ItemExecutor currentItem; // 現在使用中のアイテムの実行クラス

        // アイテムのセットアップ
        public void ItemSetup(int itemIndex)
        {
            // 選択されたアイテムの定義を使用
            var itemDef = playerData.itemList[itemIndex];
            currentItem = Instantiate(itemDef.executerPrefab, transform);
            currentItem.Initialize(playerData, boardData, guideManager, itemDef, itemIndex);
        }

        // アイテムの位置選択を呼ぶ関数
        public void ItemPosSelect(Direction dir)
        {
            currentItem.PosSelect(dir);
        }

        // アイテムのチェックを呼ぶ関数
        public bool ItemCheck()
        {
            return currentItem.Check();
        }

        // アイテムの実行を呼ぶ関数
        public IEnumerator PlayerItem()
        {
            yield return currentItem.ItemExecute();
        }

        // アイテムのクリアを呼ぶ関数
        public void Clear()
        {
            currentItem.Clear();
        }
    }
}