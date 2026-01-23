using UnityEngine;

namespace Player
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class PlayerAnimation : MonoBehaviour
    {
        private Animator anim;

        void Start()
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

        public void PlayMove(bool isMoving)
        {
            Debug.Log("PlayMove 呼ばれた");
            anim.SetBool("isMove", isMoving);
        }

        public void PlayAttack()
        {
            Debug.Log("PlayAttack 呼ばれた");
            anim.SetTrigger("isAttack");
        }
    }
}
