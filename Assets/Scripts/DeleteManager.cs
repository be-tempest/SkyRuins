using UnityEngine;
using System.Collections.Generic;

public class DeleteManager : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private PlayerData playerData;

    public bool DeleteBlocks(List<(int x, int y)>[] insertedBlocks)
    {
        bool isGameOver = false;

        for (int i = 0; i < 4; i++)
        {
            foreach ((int x, int y) in insertedBlocks[i])
            {
                int tx = 0, ty = 0;
                switch (i)
                {
                    case 0: tx = boardData.fullSize - 1; ty = y; break;
                    case 1: tx = x; ty = 0; break;
                    case 2: tx = 0; ty = y; break;
                    case 3: tx = x; ty = boardData.fullSize - 1; break;
                }

                Destroy(boardData.gridObjects[tx, ty]);
                boardData.SetGridData(0, tx, ty);
                boardData.SetGridObjects(null, tx, ty);
                boardData.SetOccupants(null, tx, ty);
                if (playerData.playerX == tx && playerData.playerY == ty) isGameOver = true;
            }
        }

        return isGameOver;
    }
}
