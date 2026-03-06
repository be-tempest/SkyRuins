using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkyRuins.Board;
using SkyRuins.Enemies;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // 魔法の実行を管理するクラス

    public class MagicManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;

        private MagicExecuter currentMagic; // 現在選択されている魔法の実行クラス

        // アニメーションのセットアップ関数
        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        // 魔法のセットアップ関数
        public void MagicSetup(int magicIndex)
        {
            // 選択された魔法を使用
            var magicDef = playerData.magicList[magicIndex];
            currentMagic = Instantiate(magicDef.executerPrefab, transform);
            currentMagic.Initialize(playerData, boardData, guideManager, playerAnimation, magicDef);
        }

        // 魔法の方向選択を呼ぶ関数
        public void MagicPosSelect(Direction dir)
        {
            currentMagic.DirSelect(dir);
        }

        // 魔法のチェックを呼ぶ関数
        public bool MagicCheck()
        {
            return currentMagic.Check();
        }

        // 魔法の実行を呼ぶ関数
        public IEnumerator PlayerMagic()
        {
            yield return currentMagic.MagicAnimation();
        }

        // 魔法のクリアを呼ぶ関数
        public void Clear()
        {
            currentMagic.Clear();
        }
    }
}