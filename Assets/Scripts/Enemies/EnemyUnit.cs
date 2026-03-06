using UnityEngine;
using System.Collections;
using SkyRuins.Board;

namespace SkyRuins.Enemies
{
    // 敵ユニットクラス
    // 敵個体のHPや位置などの情報を管理する

    public class EnemyUnit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private EnemyDefinition enemyDefinition;
        public EnemyDefinition Definition => enemyDefinition;
        private EnemyRegistry enemyRegistry;
        public EnemyAnimation enemyAnimation;
        
        [SerializeField] private int _currentHP; // 現在のHP
        public int currentHP => _currentHP;

        public bool deathDone = false; // 死亡アニメーション完了フラグ

        private int currentX; // 現在のX座標
        private int currentY; // 現在のY座標

        // 敵ユニットの初期化
        public void Init(EnemyRegistry registry, Board.BoardData board)
        {
            _currentHP = enemyDefinition.maxHP;
            enemyRegistry = registry;
            enemyRegistry.Register(this);
            enemyAnimation = GetComponent<EnemyAnimation>();
            enemyAnimation.Initialize();
            boardData = board;
        }

        // 敵ユニットの死亡処理
        public void Dead()
        {
            enemyRegistry.Unregister(this);
            boardData.occupants[currentX, currentY]?.Release();
            boardData.SetGridData(0, currentX, currentY);
            boardData.SetOccupants(null, currentX, currentY);
        }

        // 敵ユニットがダメージを受ける処理
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

        // アニメーションイベントから呼び出される完了通知
        public void AE_EnemyDeathDone()
        {
            deathDone = true;
        }

        // ダメージアニメーション完了待ちコルーチン
        private IEnumerator WaitDamageDone()
        {
            yield return new WaitForSeconds(0.5f);
            enemyAnimation.PlayDamage();
        }

        // 死亡アニメーション完了待ちコルーチン
        private IEnumerator WaitDeathDone()
        {
            yield return new WaitForSeconds(0.5f);
            enemyAnimation.PlayDead();
            yield return new WaitUntil(() => deathDone); // アニメーションイベントで死亡完了フラグが立つのを待つ
            yield return new WaitForSeconds(0.5f);
            Dead();
        }
    }
}
