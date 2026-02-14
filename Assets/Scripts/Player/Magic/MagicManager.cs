using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Pool;

namespace Player
{
    public class MagicManager : MonoBehaviour
    {
        [SerializeField] private Board.BoardData boardData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private PlayerAnimation playerAnimation;

        private MagicExecuter currentMagic;

        public void SetAnimation()
        {
            playerAnimation = playerData.playerObject.GetComponent<PlayerAnimation>();
        }

        public void MagicSetup(int magicIndex)
        {
            var magicDef = playerData.magicList[magicIndex];
            currentMagic = Instantiate(magicDef.executerPrefab, transform);
            currentMagic.Initialize(playerData, boardData, guideManager, playerAnimation, magicDef);
        }

        public void MagicPosSelect(Direction dir)
        {
            currentMagic.DirSelect(dir);
        }

        public bool MagicCheck()
        {
            return currentMagic.Check();
        }   

        public IEnumerator PlayerMagic()
        {
            yield return currentMagic.MagicAnimation();
        }

        public void Clear()
        {
            currentMagic.Clear();
        }
    }
}