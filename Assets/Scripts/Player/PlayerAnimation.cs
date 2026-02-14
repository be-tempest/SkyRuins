using UnityEngine;
using System;
using System.Collections;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;

        private Animator anim;
        public event Action CastReady;
        public event Action CastDone;

        private bool deathDone = false;

        public void Initialize(PlayerData p)
        {
            playerData = p;
            anim = GetComponent<Animator>();
            playerData.OnPlayerDamage += PlayDamage;
            playerData.OnPlayerDead += PlayDead;
        }

        public void AE_CastReady()
        {
            CastReady?.Invoke();
        }

        public void AE_CastDone()
        {
            CastDone?.Invoke();
        }

        public void AE_PlayerDeathDone()
        {
            deathDone = true;
        }

        public void AE_PlaySE_Attack()
        {
            AudioManager.Instance.PlaySE(SEType.Attack);
        }

        public void AE_PlaySE_Stop()
        {
            AudioManager.Instance.StopSE();
        }

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

        public void PlayMove(bool isMoving)
        {
            Debug.Log("PlayMove 呼ばれた");
            anim.SetBool("isMove", isMoving);
            AudioManager.Instance.PlaySE(SEType.Footstep);
        }

        public void PlayAttack()
        {
            Debug.Log("PlayAttack 呼ばれた");
            anim.SetTrigger("isAttack");
        }

        public void PlayMagic(int magicType, int phase)
        {
            Debug.Log("PlayMagic 呼ばれた");
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

        public void PlayDamage()
        {
            Debug.Log("PlayDamage 呼ばれた");
            anim.SetTrigger("isDamage");
        }

        public void PlayDead()
        {
            Debug.Log("PlayDead 呼ばれた");
            deathDone = false;
            anim.SetTrigger("isDead");
            StartCoroutine(WaitDeathDone());
        }

        private IEnumerator WaitDeathDone()
        {
            yield return new WaitUntil(() => deathDone);
            Debug.Log("死亡アニメーション完了");
            yield return new WaitForSeconds(0.3f);
            playerData.RequestGameOver();
        }
    }
}
