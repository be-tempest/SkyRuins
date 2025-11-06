// PlayerController.cs
using System;
using UnityEngine;

/// <summary>
/// PlayerMove フェーズの入力処理を BoardManager から切り出したコンポーネント。
/// - 矢印キーでプレイヤー移動（BoardManager.MovePlayerTo を呼ぶ）
/// - 1/2/3 でアイテム使用開始（BoardManager の StartUseItem を利用）
/// - pending (アイテムの方向選択) の処理は BoardManager.HandlePendingDirectionInputPublic を呼ぶ
/// </summary>
public class PlayerController : MonoBehaviour
{
    // BoardManager3 の Instance を直接使います（既存のシングルトン）。
    private BoardManager3 board => BoardManager3.Instance;

    void Update()
    {
        // Safety
        if (board == null) return;
        if (GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.CurrentPhase != GameStateManager.GamePhase.PlayerMove) return;

        // まず pending モードの処理（if true, then it handled input and we early-return）
        if (board.GetPendingMode() != 0)
        {
            // BoardManager が内部で pending input を処理する（移動やアイテム使用の確定など）
            if (board.HandlePendingDirectionInputPublic()) return;
        }
        else
        {
            // アイテムキー
            if (Input.GetKeyDown(KeyCode.Alpha1)) { board.StartUseItemPublic(0); return; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { board.StartUseItemPublic(1); return; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { board.StartUseItemPublic(2); return; }

            // 通常の移動入力
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
        else if (Input.GetKeyDown(KeyCode.Return)) { acted = true; } // stay

        if (!acted) return;

        if (newX < 1 || newY < 1 || newX > board.GetCoreSize() || newY > board.GetCoreSize()) return;

        int cell = board.GetGridDataAt(newX, newY);
        if (cell == board.OBSTACLE) return;

        // アイテム拾得判定
        if ((cell == board.ITEM_A || cell == board.ITEM_B || cell == board.ITEM_C) && !(newX == board.GetPlayerX() && newY == board.GetPlayerY()))
        {
            int idx = (cell == board.ITEM_A) ? 0 : (cell == board.ITEM_B) ? 1 : 2;
            board.IncrementItemCount(idx);
            Debug.Log($"Picked up item {idx+1}. Now have {board.GetItemCount(idx)}");
            board.RemoveOccupantAtPublic(newX, newY);
            board.SetGridDataAt(newX, newY, 0);
        }

        if (!(newX == board.GetPlayerX() && newY == board.GetPlayerY()))
        {
            // MovePlayerTo は BoardManager に実装済み
            board.MovePlayerTo(board.GetPlayerX(), board.GetPlayerY(), newX, newY, board.GetPlayerObject());
            board.SetPlayerPosition(newX, newY);
        }

        // 移動後は Slide へ
        if (GameStateManager.Instance != null) GameStateManager.Instance.SetPhase(GameStateManager.GamePhase.Slide);
    }
}