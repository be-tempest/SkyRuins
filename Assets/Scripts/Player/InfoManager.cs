using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SkyRuins.Player
{
    // プレイヤーのHPとMPのUIを管理するクラス

    public class InfoManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Image hpBar; // HPバーのイメージコンポーネント
        [SerializeField] private Image mpBar; // MPバーのイメージコンポーネント

        private void OnEnable()
        {
            playerData.OnInfoChanged += Refresh;
        }

        private void OnDisable()
        {
            playerData.OnInfoChanged -= Refresh;
        }

        // プレイヤーのHPとMPのUIを更新する関数
        private void Refresh()
        {
            // HPとMPの割合を計算してバーに反映
            hpBar.fillAmount = (float)playerData.currentHP / playerData.maxHP;
            mpBar.fillAmount = (float)playerData.currentMP / playerData.maxMP;
        }
    }
}