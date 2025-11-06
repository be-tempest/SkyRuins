// ItemManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムの所持数／使用／ターゲット選択などを管理するコンポーネント。
/// BoardManager3 と GridStorage / BoardSpawner を参照して、実際の grid/occupants/gridData を操作します。
/// </summary>
public class ItemManager : MonoBehaviour
{
    [Header("References (assign same instances as BoardManager)")]
    public GridStorage gridStorage;   // 必須: GridObjects / Occupants / GridData を参照
    public BoardSpawner boardSpawner; // optional (for removing occupant visuals)
    public BoardManager3 boardManager; // optional, to call MovePlayerTo, SetPlayerPosition, SetPhase if needed

    private int coreSize => gridStorage != null ? gridStorage.CoreSize : 7;
    private int fullSize => gridStorage != null ? gridStorage.FullSize : 9;

    // itemsCount[0] = Bomb, [1] = Shield, [2] = JumpBoots
    private int[] itemsCount = new int[3] { 0, 0, 0 };

    // pending selection state
    private int pendingItem = -1; // 0=bomb directional, 2=jump boots directional
    private int pendingMode = 0;  // 0=none,1=bomb select,2=jump select

    private bool shieldActive = false;

    void Awake()
    {
        if (gridStorage == null) Debug.LogError("[ItemManager] gridStorage not assigned!");
        if (boardSpawner == null) Debug.LogWarning("[ItemManager] boardSpawner not assigned (optional but recommended)");
    }

    // ---------- Public API used by BoardManager / PlayerController ----------

    public void InitFromCounts(int[] initialCounts)
    {
        if (initialCounts == null) return;
        for (int i = 0; i < Mathf.Min(itemsCount.Length, initialCounts.Length); i++) itemsCount[i] = initialCounts[i];
    }

    // Called when player presses item key (1/2/3). Mirrors old StartUseItem logic.
    public void StartUseItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex > 2) return;
        if (pendingMode != 0)
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
                pendingItem = 0;
                pendingMode = 1;
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
            pendingItem = 2;
            pendingMode = 2;
            Debug.Log("Jump Boots: select direction with arrow key to jump 2 tiles.");
            return;
        }
    }

    // Called each frame when expecting a directional input for pending item.
    // Returns true if a pending action was processed (so caller can early-return).
    public bool HandlePendingDirectionInput()
    {
        int dx = 0, dy = 0;
        bool pressed = false;
        if (Input.GetKeyDown(KeyCode.UpArrow)) { dx = 0; dy = 1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { dx = 0; dy = -1; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { dx = -1; dy = 0; pressed = true; }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { dx = 1; dy = 0; pressed = true; }

        if (!pressed) return false;

        // get player pos from boardManager (if available) otherwise we cannot act
        int playerX = boardManager != null ? boardManager.GetPlayerX() : -1;
        int playerY = boardManager != null ? boardManager.GetPlayerY() : -1;

        if (pendingMode == 1 && pendingItem == 0)
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
            pendingItem = -1; pendingMode = 0;
            return true;
        }
        else if (pendingMode == 2 && pendingItem == 2)
        {
            int tx = playerX + dx * 2;
            int ty = playerY + dy * 2;
            if (tx < 1 || ty < 1 || tx > coreSize || ty > coreSize)
            {
                Debug.Log("Jump target out of bounds.");
                pendingItem = -1; pendingMode = 0;
                return true;
            }
            var gridData = gridStorage.GridData;
            if (gridData[tx,ty] == 2) // OBSTACLE assumed 2 by convention; better to query BoardManager if necessary
            {
                Debug.Log("Cannot jump: destination occupied by obstacle.");
                pendingItem = -1; pendingMode = 0;
                return true;
            }

            itemsCount[2] = Mathf.Max(0, itemsCount[2] - 1);
            Debug.Log($"Jumped to ({tx},{ty}). Remaining boots: {itemsCount[2]}");

            // consume item on destination if any
            int cell = gridData[tx,ty];
            if (cell == boardManager.ITEM_A || cell == boardManager.ITEM_B || cell == boardManager.ITEM_C)
            {
                ConsumeItemAt(tx, ty);
            }

            // move via boardManager
            if (boardManager != null)
            {
                var playerObj = boardManager.GetPlayerObject();
                boardManager.MovePlayerTo(playerX, playerY, tx, ty, playerObj);
                boardManager.SetPlayerPosition(tx, ty);
            }

            pendingItem = -1; pendingMode = 0;

            // Jump now counts as player movement -> advance to Slide phase
            if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);

            return true;
        }

        pendingItem = -1; pendingMode = 0;
        return true;
    }

    // ---------- Item effect helpers (operate on gridStorage) ----------

    public bool DestroyObstacleAt(int x,int y)
    {
        if (gridStorage == null) return false;
        if (x < 1 || y < 1 || x > coreSize || y > coreSize) return false;
        var gridData = gridStorage.GridData;
        if (gridData[x,y] != boardManager.OBSTACLE) return false;
        if (gridStorage.Occupants[x,y] != null)
        {
            UnityEngine.Object.Destroy(gridStorage.Occupants[x,y]);
            gridStorage.Occupants[x,y] = null;
        }
        gridData[x,y] = 0;
        Debug.Log($"Destroyed obstacle at ({x},{y})");
        return true;
    }

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

    public void DestroyAllObstacles()
    {
        if (gridStorage == null) return;
        var gridData = gridStorage.GridData;
        for (int x = 1; x <= coreSize; x++)
        {
            for (int y = 1; y <= coreSize; y++)
            {
                if (gridData[x,y] == boardManager.OBSTACLE)
                {
                    if (gridStorage.Occupants[x,y] != null)
                    {
                        UnityEngine.Object.Destroy(gridStorage.Occupants[x,y]);
                        gridStorage.Occupants[x,y] = null;
                    }
                    gridData[x,y] = 0;
                }
            }
        }
        Debug.Log("All obstacles cleared.");
    }

    // Called when picking up an item on the board
    public void ConsumeItemAt(int x,int y)
    {
        if (gridStorage == null) return;
        var gridData = gridStorage.GridData;
        int cell = gridData[x,y];
        if (!(cell == boardManager.ITEM_A || cell == boardManager.ITEM_B || cell == boardManager.ITEM_C)) return;
        int idx = (cell == boardManager.ITEM_A) ? 0 : (cell == boardManager.ITEM_B) ? 1 : 2;
        itemsCount[idx]++;
        if (gridStorage.Occupants[x,y] != null)
        {
            UnityEngine.Object.Destroy(gridStorage.Occupants[x,y]);
            gridStorage.Occupants[x,y] = null;
        }
        gridData[x,y] = 0;
        Debug.Log($"Consumed item at ({x},{y}) -> now have {itemsCount[idx]} of item {idx}");
    }

    // ---------- Shield handling called by BoardManager during attacks ----------
    // Returns true if shield prevented the attack and consumed the shield
    public bool TryConsumeShieldProtect(int playerX, int lastX, int lastY)
    {
        if (!shieldActive) return false;
        if (playerX == lastX && playerX >= 0 && playerX < fullSize && lastY >= 0 && lastY < fullSize && playerX == playerX)
        {
            // player is at last pos and shield active -> consume and negate
            shieldActive = false;
            Debug.Log("Shield protected the player from attack!");
            return true;
        }
        return false;
    }

    // ---------- Accessors ----------
    public int GetItemCount(int idx) { if (idx < 0 || idx >= itemsCount.Length) return 0; return itemsCount[idx]; }
    public int GetPendingMode() => pendingMode;
    public bool IsShieldActive() => shieldActive;

    // For debug / editor
    public void Debug_SetCount(int idx, int count) { if (idx < 0 || idx >= itemsCount.Length) return; itemsCount[idx] = count; }
}