using UnityEngine;
using System.Collections.Generic;
using SkyRuins.Player;

namespace SkyRuins.Board
{
    // スライド後のブロックの削除を管理

    public class DeleteManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;

        // ブロックの削除
        public void DeleteBlocks(List<(int x, int y)>[] insertedBlocks)
        {
            for (int i = 0; i < 4; i++)
            {
                foreach ((int x, int y) in insertedBlocks[i])
                {
                    // 追加ブロック位置からスライドにより盤外に出たブロックの位置を計算
                    int tx = 0, ty = 0;
                    switch (i)
                    {
                        case 0: tx = boardData.fullSize - 1; ty = y; break;
                        case 1: tx = x; ty = 0; break;
                        case 2: tx = 0; ty = y; break;
                        case 3: tx = x; ty = boardData.fullSize - 1; break;
                    }

                    // 盤外にプレイヤーがいる場合はゲームオーバー
                    if (boardData.gridData[tx, ty] == boardData.playerNum)
                    {
                        playerData.RequestGameOver();
                    }

                    // 盤外に敵がいる場合は敵の死亡処理を呼び出す
                        if (boardData.gridData[tx, ty] == boardData.enemyNum)
                        {
                            var enemyUnit = boardData.occupants[tx, ty].GetComponent<Enemies.EnemyUnit>();
                            enemyUnit.Dead();
                        }

                    // ブロック・占有オブジェクトをリリースして盤面データを空にする
                    boardData.gridObjects[tx, ty].Release();
                    boardData.occupants[tx, ty]?.Release();
                    boardData.SetGridData(0, tx, ty);
                    boardData.SetGridObjects(null, tx, ty);
                    boardData.SetOccupants(null, tx, ty);
                }
            }

            return;
        }
    }
}