using System;
using System.Collections;
using UnityEngine;
using SkyRuins.UI;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // プレイヤーの行動を管理するクラス
    // 入力に応じて移動、攻撃、魔法、アイテム使用などの処理を担当

    public class PlayerManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private MoveManager moveManager;
        [SerializeField] private AttackManager attackManager;
        [SerializeField] private MagicManager magicManager;
        [SerializeField] private ItemManager itemManager;

        // コマンド状態とUIを紐づける構造体
        [System.Serializable]
        public struct CommandStruct
        {
            public CommandState commandState; // コマンドの状態
            public CommandUI commandUI; // CommandUIの参照
            public GameObject commandPanel; // コマンドのUIパネル
        }

        [SerializeField] private CommandStruct[] commands; // コマンド状態とUIの配列

        private Action end; // ターン終了コールバック
        private CommandState currentState = CommandState.None; // 現在のコマンド状態
        private CommandUI currentUI = null; // 現在のコマンドUI

        private bool isMoving = false; // 移動中フラグ
        private bool isMagic = false; // 魔法使用中フラグ
        private bool isItem = false; // アイテム使用中フラグ

        // プレイヤーの初期化関数
        public void InitPlayer()
        {
            // 各マネージャーのアニメーション設定
            moveManager.SetAnimation();
            attackManager.SetAnimation();
            magicManager.SetAnimation();
        }

        // プレイヤーターンの開始関数
        public void StartPlayerTurn(Action turnEnd)
        {
            end = turnEnd;
            ChangeState(CommandState.MainSelect);
            Debug.Log("Player Turn START");
        }

        // ターン終了の遅延処理
        private IEnumerator EndTurnDelay()
        {
            yield return new WaitForSeconds(0.2f);
            EndTurn();
        }

        // ターン終了関数
        public void EndTurn()
        {
            Debug.Log("Player Turn END");
            ChangeState(CommandState.None);
            end?.Invoke();
        }

        void Update()
        {
            // 現在のコマンド状態に応じて処理を分岐
            switch (currentState)
            {
                case CommandState.MainSelect:
                    MainSelect();
                    break;

                case CommandState.MoveSelect:
                    MoveSelect();
                    break;

                case CommandState.AttackSelect:
                    AttackSelect();
                    break;

                case CommandState.MagicSelect:
                    MagicSelect();
                    break;

                case CommandState.MagicExecute:
                    MagicExecute();
                    break;

                case CommandState.ItemSelect:
                    ItemSelect();
                    break;

                case CommandState.ItemExecute:
                    ItemExecute();
                    break;
            }
        }

        // コマンド状態を変更する関数
        void ChangeState(CommandState newState)
        {
            OnExitState(currentState);

            currentState = newState;

            // 対応するUIだけON
            // それ以外はOFF
            foreach (var command in commands)
            {
                if (command.commandState == currentState)
                {
                    command.commandPanel.SetActive(true);
                    currentUI = command.commandUI;
                }
                else
                {
                    command.commandPanel.SetActive(false);
                }
            }

            currentUI.Refresh(currentState);
            OnEnterState(currentState);
        }

        // コマンド状態に入るときの処理
        void OnEnterState(CommandState state)
        {
            switch (state)
            {
                case CommandState.MoveSelect:
                    moveManager.ShowMoveGuide();
                    break;
            }
        }

        // コマンド状態から出るときの処理
        void OnExitState(CommandState state)
        {
            switch (state)
            {
                case CommandState.MoveSelect:
                    moveManager.Clear();
                    break;
                case CommandState.AttackSelect:
                    attackManager.Clear();
                    break;
                case CommandState.MagicExecute:
                    magicManager.Clear();
                    break;
                case CommandState.ItemExecute:
                    itemManager.Clear();
                    break;
            }
        }

        // メインコマンド選択の処理
        void MainSelect()
        {
            var input = inputManager.GetInput();
            bool decided = currentUI.SelectCommand(input);

            if (!decided) return;

            switch (currentUI.index)
            {
                case 0:
                    ChangeState(CommandState.MoveSelect);
                    break;

                case 1:
                    ChangeState(CommandState.AttackSelect);
                    break;

                case 2:
                    ChangeState(CommandState.MagicSelect);
                    break;

                case 3:
                    ChangeState(CommandState.ItemSelect);
                    break;

                case 4:
                    StartCoroutine(EndTurnDelay());
                    break;
            }
        }

        // 移動方向選択の処理
        void MoveSelect()
        {
            var input = inputManager.GetInput();

            switch (input)
            {
                case InputCommand.Up:
                    moveManager.MovePosSelect(0, 1, Direction.Up);
                    break;

                case InputCommand.Down:
                    moveManager.MovePosSelect(0, -1, Direction.Down);
                    break;

                case InputCommand.Left:
                    moveManager.MovePosSelect(-1, 0, Direction.Left);
                    break;

                case InputCommand.Right:
                    moveManager.MovePosSelect(1, 0, Direction.Right);
                    break;

                case InputCommand.Decide:
                    if (!isMoving && moveManager.MovePosCheck())
                    {
                        StartCoroutine(MoveCoroutine());
                    }
                    break;

                case InputCommand.Cancel:
                    ChangeState(CommandState.MainSelect);
                    break;
            }
        }

        // 移動処理のコルーチン
        IEnumerator MoveCoroutine()
        {
            isMoving = true;
            yield return moveManager.PlayerMove();
            isMoving = false;
            StartCoroutine(EndTurnDelay());
        }

        // 攻撃コマンド選択の処理
        void AttackSelect()
        {
            bool attackFlag = false;
            var input = inputManager.GetInput();

            switch (input)
            {
                case InputCommand.Up:
                    attackManager.AttackPosSelect(0, 1, Direction.Up);
                    break;

                case InputCommand.Down:
                    attackManager.AttackPosSelect(0, -1, Direction.Down);
                    break;

                case InputCommand.Left:
                    attackManager.AttackPosSelect(-1, 0, Direction.Left);
                    break;

                case InputCommand.Right:
                    attackManager.AttackPosSelect(1, 0, Direction.Right);
                    break;

                case InputCommand.Decide:
                    attackFlag = attackManager.PlayerAttack();
                    break;

                case InputCommand.Cancel:
                    ChangeState(CommandState.MainSelect);
                    break;
            }

            if (attackFlag)
            {
                StartCoroutine(EndTurnDelay());
            }
        }

        // 魔法コマンド選択の処理
        void MagicSelect()
        {
            var input = inputManager.GetInput();

            if (input == InputCommand.Cancel)
            {
                ChangeState(CommandState.MainSelect);
                return;
            }

            bool decided = currentUI.SelectCommand(input);

            if (!decided) return;

            magicManager.MagicSetup(currentUI.index);
            ChangeState(CommandState.MagicExecute);
        }

        //　魔法方向選択の処理
        void MagicExecute()
        {
            bool magicFlag = false;
            var input = inputManager.GetInput();

            switch (input)
            {
                case InputCommand.Up:
                    magicManager.MagicPosSelect(Direction.Up);
                    break;

                case InputCommand.Down:
                    magicManager.MagicPosSelect(Direction.Down);
                    break;

                case InputCommand.Left:
                    magicManager.MagicPosSelect(Direction.Left);
                    break;

                case InputCommand.Right:
                    magicManager.MagicPosSelect(Direction.Right);
                    break;

                case InputCommand.Decide:
                    if (!isMagic && magicManager.MagicCheck())
                    {
                        StartCoroutine(MagicCoroutine());
                    }
                    break;

                case InputCommand.Cancel:
                    ChangeState(CommandState.MagicSelect);
                    break;
            }
        }

        // 魔法処理のコルーチン
        IEnumerator MagicCoroutine()
        {
            isMoving = true;
            yield return magicManager.PlayerMagic();
            isMoving = false;
            StartCoroutine(EndTurnDelay());
        }

        // アイテムコマンド選択の処理
        void ItemSelect()
        {
            var input = inputManager.GetInput();

            if (input == InputCommand.Cancel)
            {
                ChangeState(CommandState.MainSelect);
                return;
            }

            bool decided = currentUI.SelectCommand(input);

            if (!decided) return;

            itemManager.ItemSetup(currentUI.index);
            ChangeState(CommandState.ItemExecute);
        }

        // アイテム方向選択の処理
        void ItemExecute()
        {
            bool itemFlag = false;
            var input = inputManager.GetInput();

            switch (input)
            {
                case InputCommand.Up:
                    itemManager.ItemPosSelect(Direction.Up);
                    break;

                case InputCommand.Down:
                    itemManager.ItemPosSelect(Direction.Down);
                    break;

                case InputCommand.Left:
                    itemManager.ItemPosSelect(Direction.Left);
                    break;

                case InputCommand.Right:
                    itemManager.ItemPosSelect(Direction.Right);
                    break;

                case InputCommand.Decide:
                    if (!isMagic && itemManager.ItemCheck())
                    {
                        StartCoroutine(ItemCoroutine());
                    }
                    break;

                case InputCommand.Cancel:
                    ChangeState(CommandState.ItemSelect);
                    break;
            }
        }

        // アイテム処理のコルーチン
        IEnumerator ItemCoroutine()
        {
            isItem = true;
            yield return itemManager.PlayerItem();
            isItem = false;
            StartCoroutine(EndTurnDelay());
        }
    }
}