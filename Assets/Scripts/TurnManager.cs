using UnityEngine;
using System;
using System.Collections;
using TMPro;
using Board;
using Player;
using Enemies;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private GameOverUI gameOverUI;

    [SerializeField] private int turnCount = 1;
    bool isGameOver = false;

    private TurnPhase currentPhase;

    private Action nextTurn;

    [SerializeField] private GameObject bannerObject;
    [SerializeField] private TextMeshProUGUI bannerText;

    enum TurnPhase
    {
        BoardPhase,
        EnemyPhase,
        PlayerPhase
    }

    private void Start()
    {
        AudioManager.Instance.PlayGameBGM();
        boardManager.InitBoard(GameOver);
        playerManager.InitPlayer();
        gameOverUI.Hide();
        currentPhase = TurnPhase.BoardPhase;
        BoardTurn();
    }

    private void BoardTurn()
    {
        Debug.Log($"--- Turn {turnCount} : Board Phase ---");
        if (isGameOver) return;
        boardManager.StartBoardTurn(turnCount, BoardTurnEnd, GameOver);
    }

    private void BoardTurnEnd()
    {
        StartCoroutine(TurnTransition());
    }

    private void EnemyTurn()
    {
        Debug.Log($"--- Turn {turnCount} : Enemy Phase ---");
        if (isGameOver) return;
        if (turnCount == 1)
        {
            EnemyTurnEnd();
            return;
        }
        enemyManager.StartEnemyTurn(EnemyTurnEnd, GameOver);
    }

    private void EnemyTurnEnd()
    {
        StartCoroutine(TurnTransition());
    }

    private void PlayerTurn()
    {
        Debug.Log($"--- Turn {turnCount} : Player Phase ---");
        if (isGameOver) return;
        playerManager.StartPlayerTurn(PlayerTurnEnd, GameOver);
    }

    private void PlayerTurnEnd()
    {
        turnCount++;
        StartCoroutine(TurnTransition());
    }

    private IEnumerator TurnTransition()
    {
        switch (currentPhase)
        {
            case TurnPhase.BoardPhase:
                currentPhase = TurnPhase.EnemyPhase;
                nextTurn = EnemyTurn;
                bannerText.text = "Enemy Turn";
                break;

            case TurnPhase.EnemyPhase:
                currentPhase = TurnPhase.PlayerPhase;
                nextTurn = PlayerTurn;
                bannerText.text = "Player Turn";
                break;

            case TurnPhase.PlayerPhase:
                currentPhase = TurnPhase.BoardPhase;
                nextTurn = BoardTurn;
                bannerText.text = "Board Turn";
                break;
        }

        if (currentPhase == TurnPhase.PlayerPhase || !(turnCount == 1))
        {
            yield return new WaitForSeconds(1f);
            if (isGameOver) yield break;
            yield return StartCoroutine(TurnBanner());
        }                    

        nextTurn();
    }

    private IEnumerator TurnBanner()
    {
        bannerObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        bannerObject.SetActive(false);
        yield return null;
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over");
        gameOverUI.Show();
    }
}
