using System;
using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UI;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        private Action end;
        private Action over;

        [SerializeField] private InputManager inputManager;
        [SerializeField] private MoveManager moveManager;
        [SerializeField] private AttackManager attackManager;
        [SerializeField] private ItemManager itemManager;

        [System.Serializable]
        public struct CommandStruct
        {
            public CommandState commandState;
            public CommandUI commandUI;
            public GameObject commandPanel;
        }

        [SerializeField] private CommandStruct[] commands;

        private CommandState currentState = CommandState.None;
        private CommandUI currentUI = null;

        private bool isMoving = false;

        public void InitPlayer()
        {
            moveManager.SetAnimation();
            attackManager.SetAnimation();
        }

        public void StartPlayerTurn(Action turnEnd, Action gameOver)
        {
            end = turnEnd;
            over = gameOver;
            ChangeState(CommandState.MainSelect);
            Debug.Log("Player Turn START");
        }

        private IEnumerator EndTurnDelay()
        {
            yield return new WaitForSeconds(0.2f);
            EndTurn();
        }

        public void EndTurn()
        {
            Debug.Log("Player Turn END");
            ChangeState(CommandState.None);
            end?.Invoke();
        }

        void Update()
        {
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
                    break;

                case CommandState.ItemSelect:
                    ItemSelect();
                    break;

                case CommandState.ItemUse:
                    break;
            }
        }

        void ChangeState(CommandState newState)
        {
            OnExitState(currentState);

            currentState = newState;

            // 対応するUIだけON
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

        void OnEnterState(CommandState state)
        {
            switch (state)
            {
                case CommandState.MoveSelect:
                    moveManager.ShowMoveGuide();
                    break;
            }
        }

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
            }
        }

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

        void MoveSelect()
        {
            // bool moveFlag = false;
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
                    // moveFlag = moveManager.MovePosCheck();
                    if (!isMoving && moveManager.MovePosCheck())
                    {
                        StartCoroutine(MoveCoroutine());
                    }
                    break;

                case InputCommand.Cancel:
                    ChangeState(CommandState.MainSelect);
                    break;
            }

            // if (moveFlag)
            // {
            //     StartCoroutine(EndTurnDelay());
            // }
        }

        IEnumerator MoveCoroutine()
        {
            isMoving = true;
            yield return moveManager.PlayerMove();
            isMoving = false;
            StartCoroutine(EndTurnDelay());
        }

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

        void ItemSelect()
        {
            var input = inputManager.GetInput();
            bool decided = currentUI.SelectCommand(input);

            if (input == InputCommand.Cancel)
            {
                ChangeState(CommandState.MainSelect);
                return;
            }
        }

        // void ItemSelectPanel()
        // {
        //     mainPanel.SetActive(false);
        //     itemPanel.SetActive(true);

        //     if (Input.GetKeyDown(KeyCode.UpArrow)) itemUI.MoveUp();
        //     if (Input.GetKeyDown(KeyCode.DownArrow)) itemUI.MoveDown();

        //     if (Input.GetKeyDown(KeyCode.Z))
        //     {
        //         if (itemUI.index == 1)
        //         {
        //             itemManager.UseShild();
        //             Debug.Log("Use Shild!");
        //             ChangeState(CommandState.MainSelect);
        //         }
        //         else
        //         {
        //             ChangeState(CommandState.ItemUse);
        //         }
        //     }

        //     if (Input.GetKeyDown(KeyCode.X))
        //     {
        //         ChangeState(CommandState.MainSelect);
        //     }
        // }

        // void UseItem()
        // {
        //     mainPanel.SetActive(false);
        //     itemPanel.SetActive(false);

        //     int perX = 0, perY = 0;
        //     bool inputFlag = false;
        //     bool useFlag = false;

        //     if (Input.GetKeyDown(KeyCode.UpArrow)) perY = 1; inputFlag = true;
        //     if (Input.GetKeyDown(KeyCode.DownArrow)) perY = -1; inputFlag = true;
        //     if (Input.GetKeyDown(KeyCode.LeftArrow)) perX = -1; inputFlag = true;
        //     if (Input.GetKeyDown(KeyCode.RightArrow)) perX = 1; inputFlag = true;

        //     if (inputFlag)
        //     {
        //         useFlag = itemManager.UseBomb(perX, perY);
        //         if (useFlag)
        //         {
        //             ChangeState(CommandState.MainSelect);
        //         }
        //     }

        //     if (Input.GetKeyDown(KeyCode.X))
        //     {
        //         ChangeState(CommandState.ItemSelect);
        //     }
        // }
    }
}