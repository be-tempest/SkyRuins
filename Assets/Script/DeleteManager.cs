using System;
using System.Collections.Generic;
using UnityEngine;

public class DeleteManager : MonoBehaviour
{
    [Header("References (assign same instances as BoardManager)")]
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;
    public BoardManager3 boardManager;

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[DeleteManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogWarning("[DeleteManager] boardSpawner not assigned (optional but recommended)");
        if (boardManager == null) Debug.LogWarning("[DeleteManager] boardManager not assigned (optional but recommended)");
    }

    // 盤外ブロック削除
    public void DeleteBlocks(List<(int x, int y)> insertedBlocks)
    {
        if (gridStorage == null)
        {
            Debug.LogError("[DeleteManager] Cannot DeleteBlocks: gridStorage is null");
            return;
        }

        var grid = gridStorage.GridObjects;
        var occupants = gridStorage.Occupants;
        var gridData = gridStorage.GridData;

        foreach (var (x, y) in insertedBlocks)
        {
            if (x == 0)
            {
                int tx = fullSize - 1;
                int ty = y;
                DestroyAt(tx, ty, grid, occupants, gridData);
            }
            else if (x == coreSize + 1)
            {
                int tx = 0;
                int ty = y;
                DestroyAt(tx, ty, grid, occupants, gridData);
            }
            else if (y == 0)
            {
                int tx = x;
                int ty = fullSize - 1;
                DestroyAt(tx, ty, grid, occupants, gridData);
            }
            else if (y == coreSize + 1)
            {
                int tx = x;
                int ty = 0;
                DestroyAt(tx, ty, grid, occupants, gridData);
            }
            else
            {
                Debug.LogWarning($"[DeleteManager] Unexpected insertedBlock coord ({x},{y}) - ignoring");
            }
        }
    }

    // ブロックごと削除
    private void DestroyAt(int x, int y, GameObject[,] grid, GameObject[,] occupants, int[,] gridData)
    {
        if (x < 0 || y < 0 || x >= fullSize || y >= fullSize) return;

        var occ = occupants[x, y];
        if (occ != null && boardManager != null && occ == boardManager.GetPlayerObject())
        {
            Debug.Log("[DeleteManager] Player would be destroyed by Delete -> notify BoardManager GameOver.");
            boardManager.GameOver();
        }

        if (grid[x, y] != null)
        {
            Destroy(grid[x, y]);
            grid[x, y] = null;
        }

        gridStorage.ClearCell(x, y);
    }
}