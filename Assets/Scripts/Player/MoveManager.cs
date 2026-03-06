using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Board;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // プレイヤーの移動を管理するクラス

    public class MoveManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;
        [SerializeField] private GameObject moveCurser; // 移動先を示すカーソルオブジェクト
        [SerializeField] private float moveDuration = 1.0f; // 移動アニメーションの時間

        private int movePosX = 0; // 移動先のX座標
        private int movePosY = 0; // 移動先のY座標
        private Direction moveDir = Direction.Up; // 移動方向


        // アニメーションのセットアップ関数
        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        // 移動可能なマスを表示する関数
        public void ShowMoveGuide()
        {
            // 周囲4マスの座標
            List<Vector2Int> movePos = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            // プレイヤーの周囲4マスをチェックして移動可能なマスにガイドを表示
            foreach (var pos in movePos)
            {
                int posX = playerData.playerX + pos.x;
                int posY = playerData.playerY + pos.y;
                if (boardData.IsInsideCore(posX, posY))
                {
                    int occupantNum = boardData.gridData[posX, posY];
                    // 障害物や敵がいないマスにガイドを表示
                    if (!(occupantNum == boardData.obstacleNum || occupantNum == boardData.enemyNum))
                    {
                        guideManager.Show(posX, posY, CommandState.MoveSelect);
                    }
                }
            }
        }

        // 移動先を選択する関数
        public void MovePosSelect(int perX, int perY, Direction dir)
        {
            int newX = playerData.playerX + perX;
            int newY = playerData.playerY + perY;
            if (boardData.IsInsideCore(newX, newY))
            {
                int occupantNum = boardData.gridData[newX, newY];
                // 障害物や敵がいないマスを選択した場合、移動先としてカーソルを表示
                if (!(occupantNum == boardData.obstacleNum || occupantNum == boardData.enemyNum))
                {
                    moveCurser.SetActive(true);
                    movePosX = newX;
                    movePosY = newY;
                    moveDir = dir;
                    moveCurser.transform.position = new Vector3(newX, 0.55f, newY);
                }
            }
        }

        // 移動先が選択されているかをチェックする関数
        public bool MovePosCheck()
        {
            return movePosX != 0 || movePosY != 0;
        }

        // プレイヤーを移動させる関数
        public IEnumerator PlayerMove()
        {
            moveCurser.SetActive(false);
            guideManager.Clear();

            playerAnimation.SetDirection(moveDir);
            playerAnimation.PlayMove(true);

            // 移動開始位置と移動終了位置を取得
            PooledObject playerObj = boardData.occupants[playerData.playerX, playerData.playerY];
            Vector3 from = playerObj.transform.position;
            Vector3 to = new Vector3(movePosX, 0.5f, movePosY);

            yield return MoveAnimated(playerObj.transform, from, to);

            playerAnimation.PlayMove(false);

            // アイテムがあるマスに移動した場合、アイテムを取得してマスを空にする
            if (boardData.gridData[movePosX, movePosY] == boardData.itemNum)
            {
                AudioManager.Instance.PlaySE(SEType.Item);
                var itemData = boardData.occupants[movePosX, movePosY].GetComponent<ItemData>();
                playerData.itemList.Add(itemData.itemDefinition);
                boardData.occupants[movePosX, movePosY].Release();
            }

            playerObj.transform.SetParent(boardData.gridObjects[movePosX, movePosY].transform);
            playerObj.transform.localPosition = Vector3.up * 0.5f;

            // データを更新
            boardData.SetGridData(0, playerData.playerX, playerData.playerY);
            boardData.SetOccupants(null, playerData.playerX, playerData.playerY);

            playerData.SetPlayerPos(movePosX, movePosY);
            boardData.SetGridData(boardData.playerNum, movePosX, movePosY);
            boardData.SetOccupants(playerObj, movePosX, movePosY);

            Clear();

            yield return new WaitForSeconds(0.5f);
        }

        // クリア関数
        public void Clear()
        {
            movePosX = 0;
            movePosY = 0;
            moveCurser.SetActive(false);
            guideManager.Clear();
        }

        // 移動アニメーションを実行するコルーチン
        private IEnumerator MoveAnimated(Transform t, Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t01 = Mathf.Clamp01(elapsed / moveDuration);
                t.position = Vector3.Lerp(from, to, t01);
                yield return null;
            }
            t.position = to;
        }
    }
}