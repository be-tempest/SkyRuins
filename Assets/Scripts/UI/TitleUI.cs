using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkyRuins.UI
{
    // タイトル画面のUI管理

    public class TitleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button defaultButton;

        void Start()
        {
            AudioManager.Instance.PlayTitleBGM();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);           
        }
    }
}