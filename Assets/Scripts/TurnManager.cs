using UnityEngine;
using Board;
using Player;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private BoardManager boardManager;

    private int turnCount = 1;
    bool isGameOver = false;

    private void Start()
    {
        boardManager.InitBoard();
        BoardTurn();
    }

    private void BoardTurn()
    {
        if (isGameOver) return;
        boardManager.StartBoardTurn(turnCount, BoardTurnEnd, GameOver);
    }

    private void BoardTurnEnd()
    {
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        if (isGameOver) return;
        playerManager.StartPlayerTurn(PlayerTurnEnd, GameOver);
    }

    private void PlayerTurnEnd()
    {
        turnCount++;
        BoardTurn();
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over");
    }
}
