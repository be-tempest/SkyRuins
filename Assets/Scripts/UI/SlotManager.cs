using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SkyRuins.Common;

namespace SkyRuins.UI
{
    // コマンドUIのスロットを管理するクラス

    public class SlotManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText; // コマンド名を表示するテキスト
        [SerializeField] private Image icon; // コマンドアイコンを表示するイメージ

        public void Set(ISelectableData selectable)
        {
            nameText.text = selectable.DisplayName;
            // icon.sprite = selectable.Icon;
        }

        public void Clear()
        {
            nameText.text = "";
            // icon.sprite = null;
        }
    }
}