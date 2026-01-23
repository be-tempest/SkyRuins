using UnityEngine;
using System.Collections;

namespace Enemies
{
    public abstract class EnemyActionDefinition : ScriptableObject
    {
        public abstract IEnumerator Execute(EnemyUnit enemy, Board.BoardData boardData, Player.PlayerData playerData);
    }
}