using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddManager : MonoBehaviour
{
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[AddManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogError("[AddManager] boardSpawner not assigned!");
    }

    // 追加ブロック決定
    public List<(int x, int y)> AddBlocks(Dictionary<BoardManager3.Direction, int> addCount)
    {
        var inserted = new List<(int, int)>();
        if (gridStorage == null || boardSpawner == null)
        {
            Debug.LogError("[AddManager] Missing references.");
            return inserted;
        }

        var usedRows = new HashSet<int>();
        var usedCols = new HashSet<int>();

        var dirs = new BoardManager3.Direction[] {
            BoardManager3.Direction.Left,
            BoardManager3.Direction.Up,
            BoardManager3.Direction.Right,
            BoardManager3.Direction.Down
        };

        foreach (var dir in dirs)
        {
            int count = 0;
            if (addCount != null && addCount.ContainsKey(dir)) count = addCount[dir];
            if (count == 0) continue;

            // 追加位置抽選
            var candidates = new List<int>();
            for (int i = 1; i <= coreSize; i++) candidates.Add(i);
            Shuffle(candidates);

            int added = 0;
            foreach (var idx in candidates)
            {
                if (dir == BoardManager3.Direction.Left || dir == BoardManager3.Direction.Right)
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
                switch (dir)
                {
                    case BoardManager3.Direction.Left: x = 0; y = idx; break;
                    case BoardManager3.Direction.Right: x = coreSize + 1; y = idx; break;
                    case BoardManager3.Direction.Up: x = idx; y = coreSize + 1; break;
                    case BoardManager3.Direction.Down: x = idx; y = 0; break;
                }

                boardSpawner.SpawnBlockAt(x, y);
                inserted.Add((x, y));
                added++;
                if (added >= count) break;
            }
        }

        Debug.Log($"[AddManager] Inserted {inserted.Count} blocks.");
        return inserted;
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