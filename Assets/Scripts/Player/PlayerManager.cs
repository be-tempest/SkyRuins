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
        [SerializeField] private MagicManager magicManager;
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
        private bool isMagic = false;
        private bool isItem = false;

        public void InitPlayer()
        {
            moveManager.SetAnimation();
            attackManager.SetAnimation();
            magicManager.SetAnimation();
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
                case CommandState.MagicExecute:
                    magicManager.Clear();
                    break;
                case CommandState.ItemExecute:
                    itemManager.Clear();
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

        IEnumerator MagicCoroutine()
        {
            isMoving = true;
            yield return magicManager.PlayerMagic();
            isMoving = false;
            StartCoroutine(EndTurnDelay());
        }

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

        IEnumerator ItemCoroutine()
        {
            isItem = true;
            yield return itemManager.PlayerItem();
            isItem = false;
            StartCoroutine(EndTurnDelay());
        }
    }
}