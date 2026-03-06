using UnityEngine;

namespace SkyRuins.Player
{
    // アイテムの定義をオブジェクトに持たせるクラス

    public class ItemData : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ItemDefinition _itemDefinition;
        public ItemDefinition itemDefinition => _itemDefinition;
    }
}

