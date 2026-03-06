using UnityEngine;
using UnityEngine.EventSystems;

namespace SkyRuins.UI
{
    // ボタンの選択・決定時にSEを再生するクラス

    public class UIButtonSound : MonoBehaviour, ISelectHandler
    {
        // ボタンが選択されたときに呼ばれる関数
        public void OnSelect(BaseEventData eventData)
        {
            AudioManager.Instance.PlaySE(SEType.Select);
        }

        // ボタンが決定されたときに呼ばれる関数
        public void OnSubmit(BaseEventData eventData)
        {
            AudioManager.Instance.PlaySE(SEType.Decide);
        }
    }
}