using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
                    break;

                case InputCommand.Down:
                    _index = (_index + 1) % count;
                    UpdateHighlight();
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

        public void Refresh(CommandState currentState)
        {
            switch (currentState)
            {
                case CommandState.MainSelect:
                    count = commandItems.Count;
                    return;

                case CommandState.ItemSelect:
                    list = playerData.itemList.ConvertAll<ISelectableData>(x => x);
                    count = Mathf.Max(1, Mathf.Min(list.Count, slots.Count));
                    break;
            }
            
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < list.Count)
                    slots[i].Set(list[i]);
                else
                    slots[i].Clear();
            }
        }
    }
}