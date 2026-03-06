using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Player;

namespace SkyRuins.Board
{
    // ブロックのスライド処理を管理

    public class SlideManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;

        [Header("スライド時間")]
        [SerializeField] private float slideDuration = 0.40f;

        private int[,] preGridData; // 盤面データのコピー
        private PooledObject[,] preGridObjects; // 盤面ブロックオブジェクトのコピー
        private PooledObject[,] preOccupants; // 盤面占有オブジェクトのコピー

        // ブロックのスライド処理
        public IEnumerator SlideBlocks(List<(int x, int y)>[] insertedBlocks)
        {
            bool isHorizontal = false; // 行方向スライドか列方向スライドか
            int sign = 0; // スライド方向の符号（1 or -1）
            List<Coroutine> routines = new List<Coroutine>(); // スライドアニメーションのコルーチンリスト

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: isHorizontal = true; sign = 1; break; // 左から右
                    case 1: isHorizontal = false; sign = -1; break; // 上から下
                    case 2: isHorizontal = true; sign = -1; break; // 右から左
                    case 3: isHorizontal = false; sign = 1; break; // 下から上
                }

                // スライド前の盤面データをコピー
                preGridData = (int[,])boardData.gridData.Clone();
                preGridObjects = (PooledObject[,])boardData.gridObjects.Clone();
                preOccupants = (PooledObject[,])boardData.occupants.Clone();

                foreach ((int col, int row) in insertedBlocks[i])
                {
                    // 追加ブロック位置から各ブロックの移動元と移動先の座標を計算
                    // moveListにスライド対象のブロックと移動元・移動先座標を追加
                    // BoardDataを更新
                    var moveList = new List<(PooledObject obj, Vector3 from, Vector3 to)>(); // スライド対象のブロックと移動元・移動先座標のリスト

                    if (isHorizontal)
                    {
                        for (int x = 0; x < boardData.fullSize; x++)
                        {
                            if (boardData.gridObjects[x, row] != null)
                            {
                                Vector3 from = boardData.gridObjects[x, row].transform.position;
                                Vector3 to = from + new Vector3(sign, 0, 0);
                                moveList.Add((boardData.gridObjects[x, row], from, to));
                            }
                        }
                        for (int x = 0; x < boardData.fullSize; x++)
                        {
                            if (preGridObjects[x, row] != null)
                            {
                                SetBoardData(preGridData[x, row], preGridObjects[x, row], preOccupants[x, row], x + sign, row);
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < boardData.fullSize; y++)
                        {
                            if (boardData.gridObjects[col, y] != null)
                            {
                                Vector3 from = boardData.gridObjects[col, y].transform.position;
                                Vector3 to = from + new Vector3(0, 0, sign);
                                moveList.Add((boardData.gridObjects[col, y], from, to));
                            }
                        }
                        for (int y = 0; y < boardData.fullSize; y++)
                        {
                            if (preGridObjects[col, y] != null)
                            {
                                SetBoardData(preGridData[col, y], preGridObjects[col, y], preOccupants[col, y], col, y + sign);
                            }
                        }
                    }

                    SetBoardData(0, null, null, col, row); // 追加ブロック位置は空にする

                    // プレイヤーがスライドする行/列にいる場合はプレイヤー位置も更新
                    if (playerData.playerX == col) playerData.SetPlayerPos(playerData.playerX, playerData.playerY + sign);
                    if (playerData.playerY == row) playerData.SetPlayerPos(playerData.playerX + sign, playerData.playerY);

                    foreach (var m in moveList) routines.Add(StartCoroutine(MoveAnimated(m.obj.transform, m.from, m.to))); // スライドアニメーションのコルーチンをリストに追加
                }

                foreach (var r in routines) yield return r; // 同方向のスライドアニメーションを同時に開始
                yield return new WaitForSeconds(0.5f);
            }
        }

        // BoardDataの更新をまとめて行う関数
        private void SetBoardData(int data, PooledObject obj, PooledObject occ, int x, int y)
        {
            boardData.SetGridData(data, x, y);
            boardData.SetGridObjects(obj, x, y);
            boardData.SetOccupants(occ, x, y);
        }

        // スライドアニメーションのコルーチン
        private IEnumerator MoveAnimated(Transform t, Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            AudioManager.Instance.PlaySE(SEType.Slide);
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t01 = Mathf.Clamp01(elapsed / slideDuration);
                t.position = Vector3.Lerp(from, to, t01);
                yield return null;
            }
            t.position = to;
            AudioManager.Instance.StopSE();
        }
    }
}