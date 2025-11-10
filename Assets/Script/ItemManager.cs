using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GridStorage gridStorage;
    public BoardSpawner boardSpawner;
    public BoardManager3 boardManager;

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    private int[] itemsCount = new int[3] { 0, 0, 0 };

    private int pendingMode = -1; // 0: 爆弾, 2: ジャンプブーツ

    private bool shieldActive = false;

    public int GetPendingMode() => pendingMode;
    public int GetItemCount(int idx) { if (idx < 0 || idx >= itemsCount.Length) return 0; return itemsCount[idx]; }
    public bool IsShieldActive() => shieldActive;

    public void SetCount(int idx, int count) { if (idx < 0 || idx >= itemsCount.Length) return; itemsCount[idx] = count; }

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[ItemManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogWarning("[ItemManager] boardSpawner not assigned!");
        if (boardManager == null) Debug.LogWarning("[ItemManager] boardManager not assigned!");
    }

    public void InitFromCounts(int[] initialCounts)
    {
        if (initialCounts == null) return;
        for (int i = 0; i < Mathf.Min(itemsCount.Length, initialCounts.Length); i++) itemsCount[i] = initialCounts[i];
    }

    // アイテム使用状態
    public void StartUseItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex > 2) return;
        if (pendingMode != -1)
        {
            Debug.Log("[ItemManager] Already selecting target for an item.");
            return;
        }

        if (itemIndex == 0)
        {
            if (itemsCount[0] >= 5)
            {
                itemsCount[0] -= 5;
                DestroyAllObstacles();
                Debug.Log("Used 5x Bomb: cleared all obstacles.");
                return;
            }
            if (itemsCount[0] >= 3)
            {
                itemsCount[0] -= 3;
                DestroyCrossAroundPlayer();
                Debug.Log("Used 3x Bomb: destroyed cross obstacles.");
                return;
            }
            if (itemsCount[0] >= 1)
            {
                pendingMode = 0;
                Debug.Log("Bomb: select direction with arrow key to destroy obstacle one tile away.");
                return;
            }
            Debug.Log("No bombs to use.");
        }
        else if (itemIndex == 1)
        {
            if (itemsCount[1] <= 0) { Debug.Log("No shield to use."); return; }
            itemsCount[1]--;
            shieldActive = true;
            Debug.Log("Used Shield: next attack will be negated if it would hit you.");
            return;
        }
        else if (itemIndex == 2)
        {
            if (itemsCount[2] <= 0) { Debug.Log("No jump boots to use."); return; }
            pendingMode = 2;
            Debug.Log("Jump Boots: select direction with arrow key to jump 2 tiles.");
            return;
        }
    }

    public bool HandlePendingDirectionInput()
    {
        int dx = 0, dy = 0;
        bool pressed = false;
        if (Input.GetKeyDown(KeyCode.UpArrow)) { dx = 0; dy = 1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { dx = 0; dy = -1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { dx = -1; dy = 0; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { dx = 1; dy = 0; pressed = true; }

        if (!pressed) return false;

        int playerX = boardManager.GetPlayerX();
        int playerY = boardManager.GetPlayerY();

        // 爆弾
        if (pendingMode == 0)
        {
            int tx = playerX + dx;
            int ty = playerY + dy;
            if (tx < 1 || ty < 1 || tx > coreSize || ty > coreSize)
            {
                Debug.Log("Target out of bounds. Bomb wasted.");
            }
            else
            {
                bool destroyed = DestroyObstacleAt(tx, ty);
                if (destroyed) Debug.Log($"Bomb destroyed obstacle at ({tx},{ty})");
                else Debug.Log($"Bomb targeted ({tx},{ty}) but no obstacle there.");
            }
            itemsCount[0] = Mathf.Max(0, itemsCount[0] - 1);
            pendingMode = -1;
            return true;
        }
        // ジャンプブーツ
        else if (pendingMode == 2)
        {
            int tx = playerX + dx * 2;
            int ty = playerY + dy * 2;
            if (tx < 1 || ty < 1 || tx > coreSize || ty > coreSize)
            {
                Debug.Log("Jump target out of bounds.");
                pendingMode = -1;
                return true;
            }
            if (gridStorage.GridData[tx,ty] == 2)
            {
                Debug.Log("Cannot jump: destination occupied by obstacle.");
                pendingMode = -1;
                return true;
            }

            itemsCount[2] = Mathf.Max(0, itemsCount[2] - 1);
            Debug.Log($"Jumped to ({tx},{ty}). Remaining boots: {itemsCount[2]}");

            int cell = gridStorage.GridData[tx,ty];
            if (cell == boardManager.ITEM_A || cell == boardManager.ITEM_B || cell == boardManager.ITEM_C)
            {
                ConsumeItemAt(tx, ty);
            }

            if (boardManager != null)
            {
                var playerObj = boardManager.GetPlayerObject();
                boardManager.MovePlayerTo(playerX, playerY, tx, ty, playerObj);
            }

            pendingMode = -1;

            if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);
            return true;
        }

        return true;
    }

    // 爆弾：単一破壊
    public bool DestroyObstacleAt(int x, int y)
    {
        if (gridStorage == null) return false;
        if (x < 1 || y < 1 || x > coreSize || y > coreSize) return false;
        if (gridStorage.GridData[x, y] != boardManager.OBSTACLE) return false;
        if (gridStorage.Occupants[x, y] != null)
        {
            gridStorage.ClearCell(x, y);
        }
        Debug.Log($"Destroyed obstacle at ({x},{y})");
        return true;
    }

    // 爆弾：十字破壊
    public void DestroyCrossAroundPlayer()
    {
        if (boardManager == null) return;
        int px = boardManager.GetPlayerX();
        int py = boardManager.GetPlayerY();
        DestroyObstacleAt(px, py + 1);
        DestroyObstacleAt(px, py - 1);
        DestroyObstacleAt(px - 1, py);
        DestroyObstacleAt(px + 1, py);
    }

    // 爆弾：全破壊
    public void DestroyAllObstacles()
    {
        if (gridStorage == null) return;
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                DestroyObstacleAt(x, y);
            }
        }
        Debug.Log("All obstacles cleared.");
    }

    // アイテム取得
    public void ConsumeItemAt(int x, int y)
    {
        if (gridStorage == null) return;
        int cell = gridStorage.GridData[x, y];
        if (!(cell == boardManager.ITEM_A || cell == boardManager.ITEM_B || cell == boardManager.ITEM_C)) return;
        int idx = (cell == boardManager.ITEM_A) ? 0 : (cell == boardManager.ITEM_B) ? 1 : 2;
        itemsCount[idx]++;
        if (gridStorage.Occupants[x, y] != null)
        {
            gridStorage.ClearCell(x, y);
        }
        Debug.Log($"Consumed item at ({x},{y}) -> now have {itemsCount[idx]} of item {idx}");
    }

    // シールド効果判定
    public bool TryConsumeShieldProtect(int playerX, int lastX, int lastY)
    {
        if (!shieldActive) return false;
        if (playerX == lastX && playerX >= 0 && playerX < fullSize && lastY >= 0 && lastY < fullSize && playerX == playerX)
        {
            shieldActive = false;
            Debug.Log("Shield protected the player from attack!");
            return true;
        }
        return false;
    }    
}