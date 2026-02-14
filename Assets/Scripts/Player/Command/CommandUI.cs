using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace UI
{
    public class CommandUI : MonoBehaviour
    {
        [SerializeField] private Player.PlayerData playerData;
        [SerializeField] private List<Image> commandItems;
        [SerializeField] private List<SlotManager> slots;
        [SerializeField] private RectTransform selectionFrame;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightColor;
        [SerializeField] private TextMeshProUGUI explainText;

        private int _index = 0;
        public int index => _index;

        private int count = 0;
        private IReadOnlyList<ISelectableData> list;

        void OnEnable()
        {
            UpdateHighlight();
        }

        public bool SelectCommand(InputCommand input)
        {
            switch (input)
            {
                case InputCommand.Up:
                    _index = (_index - 1 + count) % count;
                    UpdateHighlight();
                    SetExplainText();
                    break;

                case InputCommand.Down:
                    _index = (_index + 1) % count;
                    UpdateHighlight();
                    SetExplainText();
                    break;

                case InputCommand.Decide:
                    return true;
            }

            return false;
        }

        public void UpdateHighlight()
        {
            for (int i = 0; i < count; i++)
            {
                commandItems[i].color = (i == _index) ? highlightColor : normalColor;
            }

            var target = commandItems[_index].rectTransform;

            selectionFrame.SetParent(target);
            selectionFrame.anchoredPosition = Vector2.zero;
        }

        public void SetExplainText()
        {
            if (explainText == null || _index >= list.Count) return;
            explainText.text = list[_index].Explanation;
        }

        public void Refresh(CommandState currentState)
        {
            switch (currentState)
            {
                case CommandState.MainSelect:
                    count = commandItems.Count;
                    return;

                case CommandState.MagicSelect:
                    list = playerData.magicList.ConvertAll<ISelectableData>(x => x);
                    count = Mathf.Max(1, Mathf.Min(list.Count, slots.Count));
                    break;

                case CommandState.ItemSelect:
                    list = playerData.itemList.ConvertAll<ISelectableData>(x => x);
                    count = Mathf.Max(1, Mathf.Min(list.Count, slots.Count));
                    break;

                default:
                    return;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (i < list.Count)
                    slots[i].Set(list[i]);
                else
                    slots[i].Clear();
            }

            explainText.text = "";

            SetExplainText();
        }
    }
}