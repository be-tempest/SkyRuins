using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkyRuins.UI
{
    // ゲームオーバー画面のUI管理
    
    public class GameOverUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private Button defaultButton;

        // ゲームオーバー画面表示
        public void Show()
        {
            AudioManager.Instance.PlayGameOverBGM();
            root.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
        }

        // ゲームオーバー画面非表示
        public void Hide()
        {
            root.SetActive(false);
        }
    }
}