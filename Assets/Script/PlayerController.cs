using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private BoardManager3 board => BoardManager3.Instance;
    public ItemManager itemManager;

    void Update()
    {
        if (board == null) return;
        if (GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.CurrentPhase != GameStateManager.GamePhase.PlayerMove) return;

        // アイテム使用中
        if (itemManager.GetPendingMode() != -1)
        {
            itemManager.HandlePendingDirectionInput();
        }
        // コマンド選択
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { itemManager.StartUseItem(0); return; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { itemManager.StartUseItem(1); return; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { itemManager.StartUseItem(2); return; }

            HandleMovementInput();
            return;
        }
    }

    private void HandleMovementInput()
    {
        int newX = board.GetPlayerX();
        int newY = board.GetPlayerY();
        bool acted = false;

        if (Input.GetKeyDown(KeyCode.UpArrow)) { newY++; acted = true; }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { newY--; acted = true; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { newX--; acted = true; }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) { newX++; acted = true; }
        else if (Input.GetKeyDown(KeyCode.Return)) { acted = true; }

        if (!acted) return;

        if (newX < 1 || newY < 1 || newX > board.GetCoreSize() || newY > board.GetCoreSize()) return;

        int cell = board.GetGridDataAt(newX, newY);
        if (cell == board.OBSTACLE) return;

        if (!(newX == board.GetPlayerX() && newY == board.GetPlayerY()))
        {
            itemManager.ConsumeItemAt(newX, newY);
            board.MovePlayerTo(board.GetPlayerX(), board.GetPlayerY(), newX, newY, board.GetPlayerObject());
        }

        if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);
    }
}