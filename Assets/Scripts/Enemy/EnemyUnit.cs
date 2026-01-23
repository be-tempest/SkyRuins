using UnityEngine;

namespace Enemies
{
    public class EnemyUnit : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition enemyDefinition;
        public EnemyDefinition Definition => enemyDefinition;

        private EnemyRegistry enemyRegistry;

        [SerializeField] private int _currentHP;
        public int currentHP => _currentHP;

        public void Init(EnemyRegistry registry)
        {
            _currentHP = enemyDefinition.maxHP;
            enemyRegistry = registry;
            enemyRegistry.Register(this);
        }

        public void Die()
        {
            enemyRegistry.Unregister(this);
        }

        // public IEnumerator Act()
        // {
        //     Debug.Log($"{gameObject.name} is acting.");
        //     yield return null;
        // }

        public void TakeDamage(int damage)
        {
            _currentHP -= damage;
            if (_currentHP <= 0)
            {
                _currentHP = 0;
                // GameOver通知など
            }
            Debug.Log($"{name} HP {_currentHP}!");
        }
    }
}
