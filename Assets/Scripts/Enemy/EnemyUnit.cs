using UnityEngine;
using System.Collections;

namespace Enemies
{
    public class EnemyUnit : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition enemyDefinition;
        public EnemyDefinition Definition => enemyDefinition;

        private EnemyRegistry enemyRegistry;
        public EnemyAnimation enemyAnimation;
        [SerializeField] private Board.BoardData boardData;


        [SerializeField] private int _currentHP;
        public int currentHP => _currentHP;

        public bool deathDone = false;

        private int currentX;
        private int currentY;

        public void Init(EnemyRegistry registry, Board.BoardData board)
        {
            _currentHP = enemyDefinition.maxHP;
            enemyRegistry = registry;
            enemyRegistry.Register(this);
            enemyAnimation = GetComponent<EnemyAnimation>();
            enemyAnimation.Initialize();
            boardData = board;
        }

        public void Dead()
        {
            enemyRegistry.Unregister(this);
            boardData.occupants[currentX, currentY]?.Release();
            boardData.SetGridData(0, currentX, currentY);
            boardData.SetOccupants(null, currentX, currentY);
        }

        public void TakeDamage(int damage, int posX, int posY)
        {
            _currentHP -= damage;
            if (_currentHP <= 0)
            {
                _currentHP = 0;
                deathDone = false;
                currentX = posX;
                currentY = posY;
                StartCoroutine(WaitDeathDone());
            }
            else
            {
                StartCoroutine(WaitDamageDone());
            }
            Debug.Log($"{name} HP {_currentHP}!");
        }

        public void AE_EnemyDeathDone()
        {
            deathDone = true;
        }

        private IEnumerator WaitDamageDone()
        {
            yield return new WaitForSeconds(0.5f);
            enemyAnimation.PlayDamage();
        }

        private IEnumerator WaitDeathDone()
        {
            yield return new WaitForSeconds(0.5f);
            enemyAnimation.PlayDead();
            yield return new WaitUntil(() => deathDone);
            yield return new WaitForSeconds(0.5f);
            Dead();
        }
    }
}
