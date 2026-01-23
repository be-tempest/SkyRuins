using UnityEngine;
using System.Collections.Generic;

namespace Enemies
{
    public class EnemyRegistry : MonoBehaviour
    {
        private List<EnemyUnit> _enemies = new();
        public List<EnemyUnit> enemies => _enemies;

        public void Register(EnemyUnit enemy)
        {
            enemies.Add(enemy);
        }

        public void Unregister(EnemyUnit enemy)
        {
            enemies.Remove(enemy);
        }
    }
}
