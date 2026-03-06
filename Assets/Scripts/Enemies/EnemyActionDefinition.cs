using UnityEngine;
using System.Collections;
using SkyRuins.Common;

namespace SkyRuins.Enemies
{
    // 敵の行動パターンを定義する抽象クラス

    public abstract class EnemyActionDefinition : ScriptableObject
    {
        // 全方向の定義
        protected static readonly Direction[] AllDirections =
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right
        };

        // 敵の行動を実行する抽象関数
        public abstract IEnumerator Execute(EnemyUnit enemy, Board.BoardData boardData, Player.PlayerData playerData);

        // 座標を方向に応じて回転させる関数
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
    }
}