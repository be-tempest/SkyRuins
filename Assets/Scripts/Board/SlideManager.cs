using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pool;

namespace Board
{
    public class SlideManager : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;
        [SerializeField] private Player.PlayerData playerData;

        [SerializeField] private float slideDuration = 0.40f;
        // public float slideDuration => _slideDuration;

        private int[,] preGridData;
        private PooledObject[,] preGridObjects;
        private PooledObject[,] preOccupants;

        public IEnumerator SlideBlocks(List<(int x, int y)>[] insertedBlocks)
        {
            bool isHorizontal = false;
            int sign = 0;
            List<Coroutine> routines = new List<Coroutine>();

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: isHorizontal = true; sign = 1; break;
                    case 1: isHorizontal = false; sign = -1; break;
                    case 2: isHorizontal = true; sign = -1; break;
                    case 3: isHorizontal = false; sign = 1; break;
                }

                preGridData = (int[,])boardData.gridData.Clone();
                preGridObjects = (PooledObject[,])boardData.gridObjects.Clone();
                preOccupants = (PooledObject[,])boardData.occupants.Clone();

                foreach ((int col, int row) in insertedBlocks[i])
                {
                    var moveList = new List<(PooledObject obj, Vector3 from, Vector3 to)>();

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

                    if (playerData.playerX == col) playerData.SetPlayerPos(playerData.playerX, playerData.playerY + sign);
                    if (playerData.playerY == row) playerData.SetPlayerPos(playerData.playerX + sign, playerData.playerY);

                    SetBoardData(0, null, null, col, row);
                    foreach (var m in moveList) routines.Add(StartCoroutine(MoveAnimated(m.obj.transform, m.from, m.to)));
                }

                foreach (var r in routines) yield return r;
                yield return new WaitForSeconds(0.03f);
            }
        }

        private void SetBoardData(int data, PooledObject obj, PooledObject occ, int x, int y)
        {
            boardData.SetGridData(data, x, y);
            boardData.SetGridObjects(obj, x, y);
            boardData.SetOccupants(occ, x, y);
        }

        private IEnumerator MoveAnimated(Transform t, Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t01 = Mathf.Clamp01(elapsed / slideDuration);
                t.position = Vector3.Lerp(from, to, t01);
                yield return null;
            }
            t.position = to;
        }
    }
}