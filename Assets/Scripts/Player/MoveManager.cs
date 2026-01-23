using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pool;

namespace Player
{
    public class MoveManager : MonoBehaviour
    {
        [SerializeField] private Board.BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;

        [SerializeField] private GameObject moveCurser;
        [SerializeField] private float moveDuration = 1.0f;

        private int movePosX = 0;
        private int movePosY = 0;
        private Direction moveDir = Direction.Up;
        

        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        public void ShowMoveGuide()
        {
            List<Vector2Int> movePos = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var pos in movePos)
            {
                int posX = playerData.playerX + pos.x;
                int posY = playerData.playerY + pos.y;
                if (boardData.IsInsideCore(posX, posY))
                {
                    int occupantNum = boardData.gridData[posX, posY];
                    if (!(occupantNum == boardData.obstacleNum || occupantNum == boardData.enemyNum))
                    {
                        guideManager.Show(posX, posY, CommandState.MoveSelect);
                    }
                }
            }
        }

        public void MovePosSelect(int perX, int perY, Direction dir)
        {
            int newX = playerData.playerX + perX;
            int newY = playerData.playerY + perY;
            if (boardData.IsInsideCore(newX, newY))
            {
                int occupantNum = boardData.gridData[newX, newY];
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

        public bool MovePosCheck()
        {
            return movePosX != 0 || movePosY != 0;
        }

        public IEnumerator PlayerMove()
        {
            playerAnimation.SetDirection(moveDir);
            playerAnimation.PlayMove(true);
            
            PooledObject playerObj = boardData.occupants[playerData.playerX, playerData.playerY];
            Vector3 from = playerObj.transform.position;
            Vector3 to = new Vector3(movePosX, 0.5f, movePosY);
            yield return MoveAnimated(playerObj.transform, from, to);

            playerAnimation.PlayMove(false);

            if (boardData.gridData[movePosX, movePosY] == boardData.itemNum)
            {
                var itemData = boardData.occupants[movePosX, movePosY].GetComponent<ItemData>();
                playerData.itemList.Add(itemData.itemDefinition);
                boardData.occupants[movePosX, movePosY].Release();
            }

            playerObj.transform.SetParent(boardData.gridObjects[movePosX, movePosY].transform);
            playerObj.transform.localPosition = Vector3.up * 0.5f;

            boardData.SetGridData(0, playerData.playerX, playerData.playerY);
            boardData.SetOccupants(null, playerData.playerX, playerData.playerY);

            playerData.SetPlayerPos(movePosX, movePosY);
            boardData.SetGridData(boardData.playerNum, movePosX, movePosY);
            boardData.SetOccupants(playerObj, movePosX, movePosY);

            Clear();

            yield return new WaitForSeconds(0.5f);
        }

        public void Clear()
        {
            movePosX = 0;
            movePosY = 0;
            moveCurser.SetActive(false);
            guideManager.Clear();
        }

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