using UnityEngine;
using System.Collections;

namespace Player
{
    public abstract class ItemExecutor : MonoBehaviour
    {
        protected PlayerData playerData;
        protected Board.BoardData boardData;
        protected GuideManager guideManager;
        protected ItemDefinition itemDef;

        protected int index;

        public abstract void PosSelect(Direction dir);

        public abstract bool Check();

        public abstract IEnumerator ItemExecute();

        public void Initialize(PlayerData p, Board.BoardData b, GuideManager g, ItemDefinition i, int idx)
        {
            playerData = p;
            boardData = b;
            guideManager = g;
            itemDef = i;
            index = idx;
            PosSelect(Direction.Up);
        }

        public void Clear()
        {
            guideManager.Clear();
            Destroy(gameObject);
        }
    }
}
