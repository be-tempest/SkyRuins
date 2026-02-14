using UnityEngine;
using System.Collections;

namespace Enemies
{
    public abstract class EnemyActionDefinition : ScriptableObject
    {
        protected static readonly Direction[] AllDirections =
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right
        };

        public abstract IEnumerator Execute(EnemyUnit enemy, Board.BoardData boardData, Player.PlayerData playerData);

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