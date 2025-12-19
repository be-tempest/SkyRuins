using UnityEngine;
using Data;
using Pool;

namespace Player
{
    public class MoveManager : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;

        public bool MovePlayer(int perX, int perY)
        {
            int newX = playerData.playerX + perX;
            int newY = playerData.playerY + perY;
            if (!boardData.IsInsideCore(newX, newY)) return false;

            PooledObject playerObj = boardData.occupants[playerData.playerX, playerData.playerY];
            int occupantNum = boardData.gridData[newX, newY];

            if (occupantNum != 0)
            {
                if (occupantNum == boardData.obstacleNum || occupantNum == boardData.enemyNum) return false;
                if (occupantNum == boardData.itemNum)
                {
                    var itemData = boardData.occupants[newX, newY].GetComponent<ItemData>();
                    playerData.AddItemCount(itemData.itemID);
                    boardData.occupants[newX, newY].Release();
                }
            }

            playerObj.transform.SetParent(boardData.gridObjects[newX, newY].transform);
            playerObj.transform.localPosition = Vector3.up * 0.5f;

            boardData.SetGridData(0, playerData.playerX, playerData.playerY);
            boardData.SetOccupants(null, playerData.playerX, playerData.playerY);

            playerData.SetPlayer(newX, newY);
            boardData.SetGridData(boardData.playerNum, newX, newY);
            boardData.SetOccupants(playerObj, newX, newY);

            return true;
        }
    }
}