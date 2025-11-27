using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private PlayerData playerData;

    public bool UseBomb(int perX, int perY)
    {
        int useX = playerData.playerX + perX;
        int useY = playerData.playerY + perY;

        if (boardData.gridData[useX, useY] != boardData.obstacleNum) return false;

        Destroy(boardData.occupants[useX, useY]);
        boardData.SetGridData(0, useX, useY);
        boardData.SetOccupants(null, useX, useY);

        return true;
    }

    public void UseShild()
    {
        playerData.SetShild(true);
    }

    public void UseBoots()
    {

    }
}
