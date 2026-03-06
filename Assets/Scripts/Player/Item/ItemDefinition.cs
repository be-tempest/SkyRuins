using UnityEngine;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Scriptable Objects/ItemDefinition")]

    // アイテムの定義を管理するクラス

    public class ItemDefinition : ScriptableObject, ISelectableData
    {
        public int id; // アイテムのID
        public string name; // アイテムの名前
        public Sprite icon; // アイテムのアイコン
        public string explanation; // アイテムの説明

        public string DisplayName => name;
        public Sprite Icon => icon;
        public string Explanation => explanation;

        public int power; // アイテムの効果量
        public ItemExecutor executerPrefab; // アイテムの実行クラスのプレハブ
        public GameObject effectPrefab; // アイテム使用時のエフェクトのプレハブ
    }
}