using UnityEngine;
using System.Collections.Generic;

namespace Board
{
    public class DeleteManager : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;

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

                    if (boardData.gridData[tx, ty] == 1) isGameOver = true;
                    if (boardData.gridData[tx, ty] == 3) 
                    {
                        var enemyUnit = boardData.occupants[tx, ty].GetComponent<Enemies.EnemyUnit>();
                        enemyUnit.Die();
                    }

                    boardData.gridObjects[tx, ty].Release();
                    boardData.occupants[tx, ty]?.Release();
                    boardData.SetGridData(0, tx, ty);
                    boardData.SetGridObjects(null, tx, ty);
                    boardData.SetOccupants(null, tx, ty);
                }
            }

            return isGameOver;
        }
    }
}