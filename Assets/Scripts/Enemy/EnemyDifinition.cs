using UnityEngine;
using System.Collections.Generic;

namespace Enemies
{
    [CreateAssetMenu(menuName = "Enemy/EnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string name;
        public int maxHP;
        public int attack;

        public List<EnemyActionDefinition> actions;
    }
}