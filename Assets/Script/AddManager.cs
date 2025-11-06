// AddManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add（外枠ブロック追加）のロジックを切り出したコンポーネント。
/// - BoardManager3 から addCount を受け取り、追加する外枠ブロックの座標リストを返す。
/// - 実際に Block を Spawn し、GridStorage.GridObjects を更新する（gridData は BoardManager が決める）。
/// - BoardManager3.Direction を参照しているので BoardManager3 の Direction を public にしてください。
/// </summary>
public class AddManager : MonoBehaviour
{
    [Header("References (assign same instances as BoardManager)")]
    public GridStorage gridStorage;   // 必須
    public BoardSpawner boardSpawner; // 必須

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[AddManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogError("[AddManager] boardSpawner not assigned!");
    }

    /// <summary>
    /// addCount: BoardManager3.Direction => count の辞書
    /// 戻り値: 追加された外枠ブロック座標のリスト (x,y)
    /// 副作用: boardSpawner.SpawnBlockAt(x,y) を呼んで GridObjects に実オブジェクトを配置する
    /// </summary>
    public List<(int x, int y)> AddBlocks(Dictionary<BoardManager3.Direction, int> addCount)
    {
        var inserted = new List<(int, int)>();
        if (gridStorage == null || boardSpawner == null)
        {
            Debug.LogError("[AddManager] Missing references - cannot AddBlocks.");
            return inserted;
        }

        var usedRows = new HashSet<int>();
        var usedCols = new HashSet<int>();

        // directions の順番は BoardManager と合わせる（Left, Up, Right, Down）
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

            // 候補は 1..coreSize
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

                // Spawn block at border and ensure GridStorage reflects it.
                // Assumes boardSpawner.SpawnBlockAt will create the GameObject and parent it appropriately.
                boardSpawner.SpawnBlockAt(x, y);

                // If boardSpawner does not itself set GridStorage.GridObjects, set it here defensively:
                // (SpawnBlockAt in your project previously populated gridStorage.GridObjects;
                //  if not, uncomment the following line and adjust to returned GameObject.)
                // gridStorage.GridObjects[x, y] = theSpawnedBlock; 

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