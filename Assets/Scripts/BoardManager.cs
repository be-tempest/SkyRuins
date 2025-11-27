using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private BoardSpawner boardSpawner;
    [SerializeField] private AddManager addManager;
    [SerializeField] private SlideManager slideManager;
    [SerializeField] private DeleteManager deleteManager;

    private Action end;
    private Action over;

    private int[] addCount = new int[4];
    private List<(int x, int y)>[] insertedBlocks = new List<(int, int)>[4];

    public void InitBoard()
    {
        // 初期生成
        boardSpawner.InitBoard();
    }

    public void StartBoardTurn(int turnCount, Action turnEnd, Action gameOver)
    {
        end = turnEnd;
        over = gameOver;
        StartCoroutine(BoardTurnRoutine(turnCount));
    }

    private IEnumerator BoardTurnRoutine(int turnCount)
    {
        LevelUp(turnCount);

        if (turnCount == 1)
        {
            yield return StartCoroutine(AddPhase(turnCount));
        }
        else
        {
            yield return StartCoroutine(SlidePhase());
            yield return StartCoroutine(DeletePhase());
            yield return StartCoroutine(AddPhase(turnCount));
        }

        end?.Invoke();
    }

    private IEnumerator AddPhase(int turnCount)
    {
        // AddManager に任せる
        for (int i = 0; i < 4; i++)
        {
            insertedBlocks[i] = new List<(int, int)>();
        }

        insertedBlocks = addManager.AddBlocks(addCount);

        for (int i = 0; i < 4; i++)
        {
            foreach ((int x, int y) in insertedBlocks[i])
            {
                boardSpawner.SpawnBlock(x, y, turnCount);
            }
        }

        yield return null;

    }

    private IEnumerator SlidePhase()
    {
        // SlideManager に任せる
        yield return StartCoroutine(slideManager.SlideBlocks(insertedBlocks));
        //yield return null;
    }

    private IEnumerator DeletePhase()
    {
        // DeleteManager に任せる
        bool isGameOver = deleteManager.DeleteBlocks(insertedBlocks);
        if (isGameOver) over?.Invoke();
        yield return null;
    }
    
    // レベルアップ
    void LevelUp(int turnCount)
    {
        int idx = (turnCount - 1) % 4;
        addCount[idx] = Mathf.Min(3, addCount[idx] + 1);
    }
}
