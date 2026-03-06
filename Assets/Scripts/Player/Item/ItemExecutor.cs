using UnityEngine;
using System.Collections;
using SkyRuins.Board;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // アイテムの実行クラスの基底クラス
    public abstract class ItemExecutor : MonoBehaviour
    {
        [Header("References")]
        protected PlayerData playerData;
        protected BoardData boardData;
        protected GuideManager guideManager;
        protected ItemDefinition itemDef;

        protected int index; // アイテムのインデックス

        public abstract void PosSelect(Direction dir); // 位置選択の処理

        public abstract bool Check(); // アイテムが使用可能かのチェック

        public abstract IEnumerator ItemExecute(); // アイテムの効果を実行するコルーチン


        // アイテムの実行クラスを初期化
        public void Initialize(PlayerData p, Board.BoardData b, GuideManager g, ItemDefinition i, int idx)
        {
            playerData = p;
            boardData = b;
            guideManager = g;
            itemDef = i;
            index = idx;
            PosSelect(Direction.Up);
        }

        // ガイドを消してアイテムの実行クラスを破棄する関数
        public void Clear()
        {
            guideManager.Clear();
            Destroy(gameObject);
        }
    }
}
