using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Board;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // 魔法の実行クラスの基底クラス

    public abstract class MagicExecuter : MonoBehaviour
    {
        [Header("References")]
        protected BoardData boardData;
        protected PlayerData playerData;
        protected GuideManager guideManager;
        protected PlayerAnimation playerAnimation;
        protected MagicDefinition magicDef;

        protected List<Vector2Int> magicPos = new List<Vector2Int>(); // 魔法の効果範囲の座標リスト
        protected Direction attackDir = Direction.Up; // 攻撃の向き

        private bool castReady; // 魔法の前半アニメーションが完了したか
        private bool castDone; // 魔法の後半アニメーションが完了したか

        private void OnCastReady() => castReady = true; // 魔法の前半アニメーションが完了したときに呼ばれる関数
        private void OnCastDone() => castDone = true; // 魔法の後半アニメーションが完了したときに呼ばれる関数

        public abstract void DirSelect(Direction dir); // 魔法の向きを選択する関数

        public abstract IEnumerator MagicExecute(); // 魔法の効果を実行する関数

        // 初期化関数
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

        // 魔法のアニメーションを再生し、効果を実行する関数
        public IEnumerator MagicAnimation()
        {
            guideManager.Clear();
            playerData.UseMP(magicDef.costMP);
            castReady = false;
            castDone = false;
            playerAnimation.SetDirection(attackDir);

            // 魔法の前半アニメーションを再生
            playerAnimation.PlayMagic(magicDef.animationType, 1);
            yield return new WaitUntil(() => castReady);

            // 魔法の効果を実行
            AudioManager.Instance.PlaySE(magicDef.seType);
            yield return MagicExecute();

            // 魔法の後半アニメーションを再生
            playerAnimation.PlayMagic(magicDef.animationType, 2);
            yield return new WaitUntil(() => castDone);
            playerAnimation.PlayMagic(magicDef.animationType, 0);

            Clear();

            yield return new WaitForSeconds(0.5f);
        }

        // 魔法を使用できるかどうかをチェックする関数
        public bool Check()
        {
            // MPが足りない、または魔法の効果範囲がない場合は使用できない
            if (playerData.currentMP < magicDef.costMP || magicPos.Count == 0)
            {
                return false;
            }

            return true;
        }

        //  座標を攻撃の向きに応じて回転させる関数
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

        // 攻撃の向きに応じた回転を返す関数
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

        // 魔法の状態をリセットする関数
        public void Clear()
        {
            magicPos.Clear();
            guideManager.Clear();
            attackDir = Direction.Up;
        }
    }
}
