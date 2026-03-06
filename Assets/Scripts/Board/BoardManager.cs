using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyRuins.Board
{
    // Boardターンの進行を管理
    // Slide -> Delete -> Add の順で進行
    // 1ターン目はブロック追加のためAddフェーズのみ

    public class BoardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardSpawner boardSpawner;
        [SerializeField] private AddManager addManager;
        [SerializeField] private SlideManager slideManager;
        [SerializeField] private DeleteManager deleteManager;

        private int[] addCount = new int[4]; // 4方向の追加ブロック数 0:左 1:上 2:右 3:下
        private List<(int x, int y)>[] insertedBlocks = new List<(int, int)>[4]; // 4方向の追加ブロック位置リスト 0:左 1:上 2:右 3:下
        private Action end; // ターン終了コールバック

        // 盤面初期生成
        public void InitBoard(Action gameOver)
        {
            boardSpawner.InitBoard(gameOver);
        }

        // Boardターン開始
        public void StartBoardTurn(int turnCount, Action turnEnd)
        {
            end = turnEnd;
            StartCoroutine(BoardTurnRoutine(turnCount));
        }

        //　Boardターン進行コルーチン
        private IEnumerator BoardTurnRoutine(int turnCount)
        {
            LevelUp(turnCount);

            // 1ターン目はAddフェーズのみ
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

        // Addフェーズ：ブロックの追加と位置の記録
        private IEnumerator AddPhase(int turnCount)
        {
            for (int i = 0; i < 4; i++)
            {
                insertedBlocks[i] = new List<(int, int)>();
            }

            insertedBlocks = addManager.AddBlocks(addCount); // 追加ブロックの位置を取得

            for (int i = 0; i < 4; i++)
            {
                foreach ((int x, int y) in insertedBlocks[i])
                {
                    boardSpawner.SpawnBlock(x, y, turnCount); // ブロックを生成
                }
            }

            yield return null;

        }

        // Slideフェーズ：ブロックのスライド
        private IEnumerator SlidePhase()
        {
            yield return StartCoroutine(slideManager.SlideBlocks(insertedBlocks));
        }

        // Deleteフェーズ：盤面外に出たブロックの削除
        private IEnumerator DeletePhase()
        {
            deleteManager.DeleteBlocks(insertedBlocks);
            yield return null;
        }

        // レベルアップ：ターン数に応じて追加ブロック数を増加
        void LevelUp(int turnCount)
        {
            int idx = (turnCount - 1) % 4;
            addCount[idx] = Mathf.Min(3, addCount[idx] + 1);
        }
    }
}
