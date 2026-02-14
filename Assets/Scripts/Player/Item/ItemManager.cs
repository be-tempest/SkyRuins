using UnityEngine;
using System.Collections;

namespace Player
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private Board.BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;

        private ItemExecutor currentItem;

        public void ItemSetup(int itemIndex)
        {
            var itemDef = playerData.itemList[itemIndex];
            currentItem = Instantiate(itemDef.executerPrefab, transform);
            currentItem.Initialize(playerData, boardData, guideManager, itemDef, itemIndex);
        }

        public void ItemPosSelect(Direction dir)
        {
            currentItem.PosSelect(dir);
        }

        public bool ItemCheck()
        {
            return currentItem.Check();
        }   

        public IEnumerator PlayerItem()
        {
            yield return currentItem.ItemExecute();
        }

        public void Clear()
        {
            currentItem.Clear();
        }
    }
}