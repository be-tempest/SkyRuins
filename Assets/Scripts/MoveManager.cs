using UnityEngine;

public class MoveManager : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private PlayerData playerData;

    public bool MovePlayer(int perX, int perY)
    {
        if (boardData.IsInsideCore(perX, perY)) return false;

        int newX = playerData.playerX + perX;
        int newY = playerData.playerY + perY;
        GameObject playerObj = boardData.occupants[playerData.playerX, playerData.playerY];
        int occupantNum = boardData.gridData[newX, newY];

        if (occupantNum != 0)
        {
            if (occupantNum == boardData.obstacleNum) return false;
            int idx = (occupantNum == boardData.item1Num) ? 0 : (occupantNum == boardData.item2Num) ? 1 : 2;
            playerData.AddItemCount(idx);
            Destroy(boardData.occupants[newX, newY]);
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
