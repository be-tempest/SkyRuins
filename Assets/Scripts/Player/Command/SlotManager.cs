using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class SlotManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image icon;

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