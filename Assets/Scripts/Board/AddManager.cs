using UnityEngine;
using System.Collections.Generic;

namespace SkyRuins.Board
{
    // 新しいブロックの追加位置を管理

    public class AddManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;

        // 追加ブロックの位置を決定
        public List<(int x, int y)>[] AddBlocks(int[] addCount)
        {
            List<(int x, int y)>[] insertedBlocks = new List<(int, int)>[4]; // 4方向の追加ブロック位置リスト 0:左 1:上 2:右 3:下

            for (int i = 0; i < 4; i++)
            {
                insertedBlocks[i] = new List<(int, int)>();
            }

            HashSet<int> usedRows = new HashSet<int>(); // 追加された行のインデックスを記録
            HashSet<int> usedCols = new HashSet<int>(); // 追加された列のインデックスを記録

            for (int i = 0; i < 4; i++)
            {
                int count = addCount[i];
                if (count == 0) continue; // 追加なしはスキップ

                List<int> candidates = new List<int>(); // コア内の行/列インデックス候補
                for (int j = 1; j <= boardData.coreSize; j++) candidates.Add(j);
                Shuffle(candidates);

                int added = 0;
                foreach (var idx in candidates)
                {
                    // 行/列の重複を避ける
                    if (i == 0 || i == 2)
                    {
                        if (usedRows.Contains(idx)) continue;
                        usedRows.Add(idx);
                    }
                    else
                    {
                        if (usedCols.Contains(idx)) continue;
                        usedCols.Add(idx);
                    }

                    // 追加位置を決定
                    int x = 0, y = 0;
                    switch (i)
                    {
                        case 0: x = 0; y = idx; break;
                        case 1: x = idx; y = boardData.coreSize + 1; break;
                        case 2: x = boardData.coreSize + 1; y = idx; break;
                        case 3: x = idx; y = 0; break;
                    }

                    insertedBlocks[i].Add((x, y));
                    added++;
                    if (added >= count) break;
                }
            }

            return insertedBlocks;
        }

        // リストをシャッフルする関数
        void Shuffle(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
