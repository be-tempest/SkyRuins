using UnityEngine;
using Data;

namespace Player
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;

        public bool UseBomb(int perX, int perY)
        {
            int useX = playerData.playerX + perX;
            int useY = playerData.playerY + perY;

            if (!boardData.IsInsideCore(useX, useY)) return false;
            if (boardData.gridData[useX, useY] != boardData.obstacleNum) return false;

            boardData.occupants[useX, useY].Release();
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
}