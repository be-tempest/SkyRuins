// DeleteManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Board の外枠ブロックを削除する責務を持つコンポーネント（Delete フェイズ用）。
/// - GridStorage / BoardSpawner を参照して grid/occupants/gridData を更新する
/// - プレイヤーが押し出されたかの判定は BoardManager3 に委ね、必要なら BoardManager3.GameOverPublic() を呼ぶ
/// </summary>
public class DeleteManager : MonoBehaviour
{
    [Header("References (assign same instances as BoardManager)")]
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;
    public BoardManager3 boardManager; // BoardManager3 の参照（同じシーンのインスタンスを割り当ててください）

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[DeleteManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogWarning("[DeleteManager] boardSpawner not assigned (optional but recommended)");
        if (boardManager == null) Debug.LogWarning("[DeleteManager] boardManager not assigned (optional but recommended)");
    }

    /// <summary>
    /// insertedBlocks: BoardManager が保持している「追加された外枠ブロック座標」のリスト
    /// このメソッドは同スレッド（コルーチン内）で呼んでください（ゲームループの安全性のため）。
    /// </summary>
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

        foreach (var (x,y) in insertedBlocks)
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

    private void DestroyAt(int x,int y, GameObject[,] grid, GameObject[,] occupants, int[,] gridData)
    {
        // bounds safety
        if (x < 0 || y < 0 || x >= fullSize || y >= fullSize) return;

        // If occupant is the player, tell BoardManager to handle game over
        var occ = occupants[x,y];
        if (occ != null && boardManager != null && occ == boardManager.GetPlayerObject())
        {
            Debug.Log("[DeleteManager] Player would be destroyed by Delete -> notify BoardManager GameOver.");
            boardManager.GameOverPublic();
            // Note: we still continue to destroy the objects so board is cleaned up
        }

        if (grid[x,y] != null)
        {
            UnityEngine.Object.Destroy(grid[x,y]);
        }
        if (occupants[x,y] != null)
        {
            UnityEngine.Object.Destroy(occupants[x,y]);
        }

        grid[x,y] = null;
        occupants[x,y] = null;
        gridData[x,y] = 0;
    }
}