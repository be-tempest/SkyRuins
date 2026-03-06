using UnityEngine;
using System;
using System.Collections;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // プレイヤーのアニメーションを管理するクラス

    public class PlayerAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData playerData;

        private Animator anim; // アニメーターコンポーネント
        private bool deathDone = false; // 死亡アニメーションが完了フラグ

        // 間に魔法のエフェクトを挟むためのイベント
        public event Action CastReady; // 魔法のアニメーションの前半が完了したときに呼び出されるイベント
        public event Action CastDone; // 魔法のアニメーションの後半が完了したときに呼び出されるイベント
        
        // 初期化関数
        public void Initialize(PlayerData p)
        {
            playerData = p;
            anim = GetComponent<Animator>();
            playerData.OnPlayerDamage += PlayDamage;
            playerData.OnPlayerDead += PlayDead;
        }

        // 魔法のアニメーションの前半が完了したときに呼び出される関数
        public void AE_CastReady()
        {
            CastReady?.Invoke();
        }

        // 魔法のアニメーションの後半が完了したときに呼び出される関数
        public void AE_CastDone()
        {
            CastDone?.Invoke();
        }

        // 死亡アニメーションが完了したときに呼び出される関数
        public void AE_PlayerDeathDone()
        {
            deathDone = true;
        }

        // 攻撃のSEを再生する関数
        public void AE_PlaySE_Attack()
        {
            AudioManager.Instance.PlaySE(SEType.Attack);
        }

        // SEを停止する関数
        public void AE_PlaySE_Stop()
        {
            AudioManager.Instance.StopSE();
        }

        // プレイヤーの向きを設定する関数
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

        // 移動アニメーションを再生する関数
        public void PlayMove(bool isMoving)
        {
            Debug.Log("PlayMove 呼ばれた");
            anim.SetBool("isMove", isMoving);
            AudioManager.Instance.PlaySE(SEType.Footstep);
        }

        // 攻撃アニメーションを再生する関数
        public void PlayAttack()
        {
            Debug.Log("PlayAttack 呼ばれた");
            anim.SetTrigger("isAttack");
        }

        // 魔法のアニメーションを再生する関数
        public void PlayMagic(int magicType, int phase)
        {
            Debug.Log("PlayMagic 呼ばれた");

            // 魔法に応じて3パターンのアニメーションを再生
            switch (magicType)
            {
                case 1:
                    anim.SetInteger("isMagic1", phase);
                    break;
                case 2:
                    anim.SetInteger("isMagic2", phase);
                    break;
                case 3:
                    anim.SetInteger("isMagic3", phase);
                    break;
            }
        }

        // ダメージアニメーションを再生する関数
        public void PlayDamage()
        {
            Debug.Log("PlayDamage 呼ばれた");
            anim.SetTrigger("isDamage");
        }

        // 死亡アニメーションを再生する関数
        public void PlayDead()
        {
            Debug.Log("PlayDead 呼ばれた");
            deathDone = false;
            anim.SetTrigger("isDead");
            StartCoroutine(WaitDeathDone());
        }

        // 死亡アニメーションの完了を待つコルーチン
        private IEnumerator WaitDeathDone()
        {
            yield return new WaitUntil(() => deathDone);
            Debug.Log("死亡アニメーション完了");
            yield return new WaitForSeconds(0.3f);
            playerData.RequestGameOver();
        }
    }
}
