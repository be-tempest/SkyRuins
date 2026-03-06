using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using SkyRuins.Common;
using SkyRuins.Player;

namespace SkyRuins.UI
{
    // コマンド選択UIを管理するクラス
    // プレイヤーのコマンド選択を処理し、UIを更新する
    // メインコマンド、魔法選択、アイテム選択で使用

    public class CommandUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private List<SlotManager> slots;
        [SerializeField] private List<Image> commandItems;
        [SerializeField] private TextMeshProUGUI explainText; // コマンドの説明テキスト
        [SerializeField] private RectTransform selectionFrame; // 選択フレームのRectTransform
        [SerializeField] private Color normalColor; // 通常のコマンドの色
        [SerializeField] private Color highlightColor; // ハイライトされたコマンドの色

        private int _index = 0; // 現在選択されているコマンドのインデックス
        public int index => _index;

        private int count = 0; // 現在表示されているコマンドの数
        private IReadOnlyList<ISelectableData> list;

        void OnEnable()
        {
            UpdateHighlight();
        }

        // コマンド選択を処理する関数
        public bool SelectCommand(InputCommand input)
        {
            // 入力に応じてインデックスを更新し、UIを更新する
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

        // ハイライトを更新する関数
        public void UpdateHighlight()
        {
            for (int i = 0; i < count; i++)
            {
                commandItems[i].color = (i == _index) ? highlightColor : normalColor;
            }

            // 選択フレームを現在の選択項目に移動する
            var target = commandItems[_index].rectTransform;
            selectionFrame.SetParent(target);
            selectionFrame.anchoredPosition = Vector2.zero;
        }

        // コマンドの説明テキストを設定する関数
        public void SetExplainText()
        {
            if (explainText == null || _index >= list.Count) return;
            explainText.text = list[_index].Explanation;
        }

        // コマンドリストを更新する関数
        public void Refresh(CommandState currentState)
        {
            switch (currentState)
            {
                case CommandState.MainSelect: // メインコマンドの表示
                    count = commandItems.Count;
                    return;

                case CommandState.MagicSelect: // 魔法選択の表示
                    list = playerData.magicList.ConvertAll<ISelectableData>(x => x);
                    count = Mathf.Max(1, Mathf.Min(list.Count, slots.Count));
                    break;

                case CommandState.ItemSelect: // アイテム選択の表示
                    list = playerData.itemList.ConvertAll<ISelectableData>(x => x);
                    count = Mathf.Max(1, Mathf.Min(list.Count, slots.Count));
                    break;

                default:
                    return;
            }

            // スロットにコマンドを設定
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