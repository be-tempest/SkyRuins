using UnityEngine;
using System.Collections.Generic;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    [CreateAssetMenu(fileName = "MagicDefinition", menuName = "Scriptable Objects/MagicDefinition")]

    // 魔法の定義を管理するクラス

    public class MagicDefinition : ScriptableObject, ISelectableData
    {
        public int id; // 魔法のID
        public string name; // 魔法の名前
        public Sprite icon; // 魔法のアイコン
        public string explanation; // 魔法の説明

        public string DisplayName => name;
        public Sprite Icon => icon;
        public string Explanation => explanation;

        public int costMP; // 魔法のMPコスト
        public int power; // 魔法の威力
        public List<Vector2Int> magicRange; // 魔法の範囲
        public int animationType; // 魔法のアニメーションタイプ
        public MagicExecuter executerPrefab; // 魔法の実行クラスのプレハブ
        public GameObject effectPrefab; // 魔法の効果エフェクトのプレハブ
        public SEType seType; // 魔法の効果音タイプ
    }
}
