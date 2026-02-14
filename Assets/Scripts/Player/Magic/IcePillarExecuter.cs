using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enemies;

namespace Player
{
    public class IcePillarExecuter : MagicExecuter
    {
        public override void DirSelect(Direction dir)
        {
            magicPos.Clear();
            guideManager.Clear();
            attackDir = dir;

            foreach (var pos in magicDef.magicRange)
            {
                Vector2Int rotatePos = RotatePos(pos, dir);
                int posX = rotatePos.x + playerData.playerX;
                int posY = rotatePos.y + playerData.playerY;

                if (boardData.IsInsideCore(posX, posY))
                {
                    guideManager.Show(posX, posY, CommandState.MagicSelect);
                    magicPos.Add(new Vector2Int(posX, posY));
                }
            }
        }

        public override IEnumerator MagicExecute()
        {
            // var pos = new Vector3(magicPos[0].x, 0.5f, magicPos[0].y);
            var pos = new Vector3(playerData.playerX, 0.5f, playerData.playerY);
            var rot = GetRotation(attackDir);
            GameObject effect = Instantiate(magicDef.effectPrefab, pos, rot);
            Destroy(effect, 1.5f);

            yield return new WaitForSeconds(1.5f);

            foreach (var targetPos in magicPos)
            {
                if (boardData.gridData[targetPos.x, targetPos.y] == boardData.enemyNum)
                {
                    var enemyUnit = boardData.occupants[targetPos.x, targetPos.y].GetComponent<EnemyUnit>();
                    enemyUnit.TakeDamage(magicDef.power, targetPos.x, targetPos.y);
                    Debug.Log("IcePillarで" + magicDef.power + "のダメージを与えた");
                    // if (enemyUnit.currentHP <= 0)
                    // {
                    //     boardData.occupants[targetPos.x, targetPos.y].Release();
                    //     boardData.SetGridData(0, targetPos.x, targetPos.y);
                    //     boardData.SetOccupants(null, targetPos.x, targetPos.y);
                    // }
                }
            }
        }
    }
}
