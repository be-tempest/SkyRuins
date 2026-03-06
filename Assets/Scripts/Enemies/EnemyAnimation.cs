using UnityEngine;
using SkyRuins.Common;

namespace SkyRuins.Enemies
{
    // 敵のアニメーションを管理するクラス

    public class EnemyAnimation : MonoBehaviour
    {
        private Animator anim; // アニメーターコンポーネント

        // 初期化関数
        public void Initialize()
        {
            anim = GetComponent<Animator>();
        }

        // 敵の向きを設定する関数
        public void SetDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.Down:
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Direction.Left:
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Right:
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
        }

        // 攻撃アニメーションを再生する関数
        public void PlayAttack()
        {
            Debug.Log("Enemy Attack Animation");
            anim.SetTrigger("isAttack");
        }

        // ダメージアニメーションを再生する関数
        public void PlayDamage()
        {
            anim.SetTrigger("isDamage");
        }

        // 死亡アニメーションを再生する関数
        public void PlayDead()
        {
            anim.SetTrigger("isDead");
        }
    }
}
