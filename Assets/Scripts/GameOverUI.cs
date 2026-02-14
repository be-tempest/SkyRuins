using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button defaultButton;

    public void Show()
    {
        AudioManager.Instance.PlayGameOverBGM();
        root.SetActive(true);

        // まず選択状態を作る
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
    }

    public void Hide()
    {
        root.SetActive(false);
    }


}