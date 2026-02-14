using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Player
{
    public abstract class MagicExecuter : MonoBehaviour
    {
        protected PlayerData playerData;
        protected Board.BoardData boardData;
        protected GuideManager guideManager;
        protected PlayerAnimation playerAnimation;
        protected MagicDefinition magicDef;

        protected List<Vector2Int> magicPos = new List<Vector2Int>();
        protected Direction attackDir = Direction.Up;

        private bool castReady;
        private bool castDone;

        private void OnCastReady() => castReady = true;
        private void OnCastDone() => castDone = true;

        public abstract void DirSelect(Direction dir);

        public abstract IEnumerator MagicExecute();

        public void Initialize(PlayerData p, Board.BoardData b, GuideManager g, PlayerAnimation a, MagicDefinition m)
        {
            playerData = p;
            boardData = b;
            guideManager = g;
            playerAnimation = a;
            magicDef = m;

            playerAnimation.CastReady += OnCastReady;
            playerAnimation.CastDone += OnCastDone;
        }

        public IEnumerator MagicAnimation()
        {
            guideManager.Clear();
            playerData.UseMP(magicDef.costMP);
            castReady = false;
            castDone = false;
            playerAnimation.SetDirection(attackDir);

            playerAnimation.PlayMagic(magicDef.animationType, 1);
            yield return new WaitUntil(() => castReady);

            AudioManager.Instance.PlaySE(magicDef.seType);
            yield return MagicExecute();

            playerAnimation.PlayMagic(magicDef.animationType, 2);
            yield return new WaitUntil(() => castDone);
            playerAnimation.PlayMagic(magicDef.animationType, 0);

            Clear();

            yield return new WaitForSeconds(0.5f);
        }

        public bool Check()
        {
            if (playerData.currentMP < magicDef.costMP || magicPos.Count == 0)
            {
                return false;
            }

            return true;
        }

        public Vector2Int RotatePos(Vector2Int p, Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return p;

                case Direction.Right:
                    return new Vector2Int(p.y, -p.x);

                case Direction.Down:
                    return new Vector2Int(-p.x, -p.y);

                case Direction.Left:
                    return new Vector2Int(-p.y, p.x);
            }

            return p;
        }

        public Quaternion GetRotation(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return Quaternion.identity;
                
                case Direction.Down:
                    return Quaternion.Euler(0, 180f, 0f);

                case Direction.Left:
                    return Quaternion.Euler(0, -90f, 0f);

                case Direction.Right:
                    return Quaternion.Euler(0, 90f, 0f);

                default:
                    return Quaternion.identity;
            }
        }

        public void Clear()
        {
            magicPos.Clear();
            guideManager.Clear();
            attackDir = Direction.Up;
            // Destroy(gameObject);
        }

        
    }
}
