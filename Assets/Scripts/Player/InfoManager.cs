using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Player
{
    public class InfoManager : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Image hpBar;
        [SerializeField] private Image mpBar;

        private void OnEnable()
        {
            playerData.OnInfoChanged += Refresh;
        }

        private void OnDisable()
        {
            playerData.OnInfoChanged -= Refresh;
        }

        private void Refresh()
        {
            hpBar.fillAmount = (float)playerData.currentHP / playerData.maxHP;
            mpBar.fillAmount = (float)playerData.currentMP / playerData.maxMP;
        }
    }
}