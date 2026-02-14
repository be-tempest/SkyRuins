using UnityEngine;

namespace Enemies
{
    public class EnemyAnimation : MonoBehaviour
    {
        private Animator anim;

        public void Initialize()
        {
            anim = GetComponent<Animator>();
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

        public void PlayAttack()
        {
            Debug.Log("Enemy Attack Animation");
            anim.SetTrigger("isAttack");
        }

        public void PlayDamage()
        {
            anim.SetTrigger("isDamage");
        }

        public void PlayDead()
        {
            anim.SetTrigger("isDead");
        }
    }
}
