using UnityEngine;
using Board;
using Player;
using Enemies;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemyManager enemyManager;

    private int turnCount = 1;
    bool isGameOver = false;

    private void Start()
    {
        boardManager.InitBoard();
        playerManager.InitPlayer();
        BoardTurn();
    }

    private void BoardTurn()
    {
        if (isGameOver) return;
        boardManager.StartBoardTurn(turnCount, BoardTurnEnd, GameOver);
    }

    private void BoardTurnEnd()
    {
        EnemyTurn();
    }

    private void EnemyTurn()
    {
        if (isGameOver) return;
        if (turnCount == 1)
        {
            PlayerTurn();
            return;
        }
        enemyManager.StartEnemyTurn(EnemyTurnEnd, GameOver);
    }

    private void EnemyTurnEnd()
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
