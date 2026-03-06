using UnityEngine;
using System.Collections.Generic;

namespace SkyRuins.Enemies
{
    [CreateAssetMenu(menuName = "Enemy/EnemyDefinition")]

    // 敵の基本情報と行動パターンを定義

    public class EnemyDefinition : ScriptableObject
    {
        public string name; // 敵の名前
        public int maxHP; // 敵の最大HP
        public int attack; // 敵の攻撃力

        public List<EnemyActionDefinition> actions; // 敵の行動パターン（攻撃や移動など）
    }
}