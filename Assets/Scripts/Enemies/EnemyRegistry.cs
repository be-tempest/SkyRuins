using UnityEngine;
using System.Collections.Generic;

namespace SkyRuins.Enemies
{
    // 盤面上の全敵ユニットを管理するクラス

    public class EnemyRegistry : MonoBehaviour
    {
        private List<EnemyUnit> _enemies = new(); // 登録された敵ユニットのリスト
        public List<EnemyUnit> enemies => _enemies;

        // 敵ユニットを登録する関数
        public void Register(EnemyUnit enemy)
        {
            enemies.Add(enemy);
        }

        // 敵ユニットを登録解除する関数
        public void Unregister(EnemyUnit enemy)
        {
            enemies.Remove(enemy);
        }
    }
}
