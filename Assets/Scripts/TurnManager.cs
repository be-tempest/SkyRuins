using UnityEngine;
using System;
using System.Collections;
using TMPro;
using SkyRuins.Board;
using SkyRuins.Player;
using SkyRuins.Enemies;
using SkyRuins.UI;

namespace SkyRuins
{
    // ターンのフェーズを定義
    enum TurnPhase
    {
        BoardPhase,
        EnemyPhase,
        PlayerPhase
    }

    // インゲームのターン進行を管理
    // ターンはPlayer -> Board -> Enemy の順で進行
    // 1ターン目のブロックの追加のためBoardPhaseを先にする

    public class TurnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private GameObject bannerObject;
        [SerializeField] private TextMeshProUGUI bannerText;

        private int turnCount = 1; // ターン数
        private bool isGameOver = false; // ゲームオーバーフラグ
        private TurnPhase currentPhase; // 現在のフェーズ
        private Action nextTurn; // 次のフェーズ開始の関数用

        // ゲーム開始・初期化   
        private void Start()
        {
            AudioManager.Instance.PlayGameBGM();
            boardManager.InitBoard(GameOver);
            playerManager.InitPlayer();
            gameOverUI.Hide();
            currentPhase = TurnPhase.BoardPhase;
            BoardTurn();
        }

        // Boardターン開始
        private void BoardTurn()
        {
            Debug.Log($"--- Turn {turnCount} : Board Phase ---");
            boardManager.StartBoardTurn(turnCount, BoardTurnEnd);
        }

        // Boardターン終了
        private void BoardTurnEnd()
        {
            StartCoroutine(TurnTransition());
        }

        // Enemyターン開始
        private void EnemyTurn()
        {
            Debug.Log($"--- Turn {turnCount} : Enemy Phase ---");
            // 1ターン目は敵の行動なし
            if (turnCount == 1)
            {
                EnemyTurnEnd();
                return;
            }
            enemyManager.StartEnemyTurn(EnemyTurnEnd);
        }

        // Enemyターン終了
        private void EnemyTurnEnd()
        {
            StartCoroutine(TurnTransition());
        }

        // Playerターン開始
        private void PlayerTurn()
        {
            Debug.Log($"--- Turn {turnCount} : Player Phase ---");
            playerManager.StartPlayerTurn(PlayerTurnEnd);
        }

        // Playerターン終了
        private void PlayerTurnEnd()
        {
            turnCount++; // ターン数を増やす
            StartCoroutine(TurnTransition());
        }

        // ターン間の遷移処理・バナーテキストの更新
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

            // 1ターン目はPlayerPhaseのみ遷移でバナー表示
            if (currentPhase == TurnPhase.PlayerPhase || !(turnCount == 1))
            {
                yield return new WaitForSeconds(1f);
                if (isGameOver) yield break;
                yield return StartCoroutine(TurnBanner());
            }

            nextTurn();
        }

        // ターンバナー表示
        private IEnumerator TurnBanner()
        {
            bannerObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            bannerObject.SetActive(false);
            yield return null;
        }

        // ゲームオーバー処理
        private void GameOver()
        {
            Debug.Log("Game Over");
            isGameOver = true;
            gameOverUI.Show();
        }
    }
}
