using System;
using System.Collections;
//using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    private Action end;
    private Action over;

    public enum CommandState
    {
        None,
        MainSelect,
        MoveSelect,
        ItemSelect,
        ItemUse
    }

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private CommandUIGroup mainUI;

    [SerializeField] private GameObject itemPanel;
    [SerializeField] private CommandUIGroup itemUI;

    [SerializeField] private MoveManager moveManager;
    [SerializeField] private ItemManager itemManager;

    private CommandState currentState = CommandState.None;

    void Update()
    {
        switch (currentState)
        {
            case CommandState.MainSelect:
                MainSelectPanel();
                break;

            case CommandState.MoveSelect:
                MoveSelect();
                break;

            case CommandState.ItemSelect:
                ItemSelectPanel();
                break;
            
            case CommandState.ItemUse:
                UseItem();
                break;
        }
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

    void ChangeState(CommandState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case CommandState.MainSelect:
                mainPanel.SetActive(true);
                itemPanel.SetActive(false);
                break;

            case CommandState.MoveSelect:
                mainPanel.SetActive(false);
                itemPanel.SetActive(false);
                break;

            case CommandState.ItemSelect:
                mainPanel.SetActive(false);
                itemPanel.SetActive(true);
                break;

            case CommandState.None:
                mainPanel.SetActive(false);
                itemPanel.SetActive(false);
                break;
        }
    }

    void MainSelectPanel()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) mainUI.MoveUp();
        if (Input.GetKeyDown(KeyCode.DownArrow)) mainUI.MoveDown();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (mainUI.index == 0)
            {
                ChangeState(CommandState.MoveSelect);
            }
            else if (mainUI.index == 1)
            {
                ChangeState(CommandState.ItemSelect);
            }
        }
    }

    void MoveSelect()
    {
        mainPanel.SetActive(false);
        itemPanel.SetActive(false);

        bool moveFlag = false;
        
        if (Input.GetKeyDown(KeyCode.UpArrow)) moveFlag = moveManager.MovePlayer(0, 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) moveFlag = moveManager.MovePlayer(0, -1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) moveFlag = moveManager.MovePlayer(-1, 0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) moveFlag = moveManager.MovePlayer(1, 0);    
        if (Input.GetKeyDown(KeyCode.Return)) moveFlag = moveManager.MovePlayer(0, 0);

        if (moveFlag)
        {
            StartCoroutine(EndTurnDelay());
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ChangeState(CommandState.MainSelect);
        }
    }

    void ItemSelectPanel()
    {
        mainPanel.SetActive(false);
        itemPanel.SetActive(true);
        
        if (Input.GetKeyDown(KeyCode.UpArrow)) itemUI.MoveUp();
        if (Input.GetKeyDown(KeyCode.DownArrow)) itemUI.MoveDown();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (itemUI.index == 1)
            {
                itemManager.UseShild();
                Debug.Log("Use Shild!");
                ChangeState(CommandState.MainSelect);
            }
            else
            {
                ChangeState(CommandState.ItemUse);
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ChangeState(CommandState.MainSelect);
        }
    }

    void UseItem()
    {
        mainPanel.SetActive(false);
        itemPanel.SetActive(false);

        int perX = 0, perY = 0;
        bool inputFlag = false;
        bool useFlag = false;

        if (Input.GetKeyDown(KeyCode.UpArrow)) perY = 1; inputFlag = true;
        if (Input.GetKeyDown(KeyCode.DownArrow)) perY = -1; inputFlag = true;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) perX = -1; inputFlag = true;
        if (Input.GetKeyDown(KeyCode.RightArrow)) perX = 1; inputFlag = true;

        if (inputFlag)
        {
            useFlag = itemManager.UseBomb(perX, perY);
            if (useFlag)
            {
                ChangeState(CommandState.MainSelect);
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ChangeState(CommandState.ItemSelect);
        }
    }

    
}
