using UnityEngine;
using System.Collections.Generic;
using Data;

namespace Board
{
    public class AddManager : MonoBehaviour
    {
        [SerializeField] private BoardData boardData;

        public List<(int x, int y)>[] AddBlocks(int[] addCount)
        {
            List<(int x, int y)>[] insertedBlocks = new List<(int, int)>[4];

            for (int i = 0; i < 4; i++)
            {
                insertedBlocks[i] = new List<(int, int)>();
            }

            HashSet<int> usedRows = new HashSet<int>();
            HashSet<int> usedCols = new HashSet<int>();

            for (int i = 0; i < 4; i++)
            {
                int count = addCount[i];
                if (count == 0) continue;

                // 追加位置抽選
                List<int> candidates = new List<int>();
                for (int j = 1; j <= boardData.coreSize; j++) candidates.Add(j);
                Shuffle(candidates);

                int added = 0;
                foreach (var idx in candidates)
                {
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
