using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class CommandUIGroup : MonoBehaviour
    {
        [SerializeField] private List<Image> commandItems;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;

        private int _index = 0;
        public int index => _index;

        private int count => commandItems.Count;

        void OnEnable()
        {
            UpdateHighlight();
        }

        public void MoveUp()
        {
            _index = (_index - 1 + count) % count;
            UpdateHighlight();
        }

        public void MoveDown()
        {
            _index = (_index + 1) % count;
            UpdateHighlight();
        }

        public void UpdateHighlight()
        {
            for (int i = 0; i < count; i++)
            {
                commandItems[i].color = (i == _index) ? highlightColor : normalColor;
            }
        }
    }
}