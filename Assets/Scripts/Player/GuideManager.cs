using UnityEngine;
using System.Collections.Generic;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // 盤面上に選択ガイドを表示するクラス

    public class GuideManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ObjectPool guidePool;

        [SerializeField] private Color moveColor; // 移動選択の色
        [SerializeField] private Color attackColor; // 攻撃選択の色
        [SerializeField] private Color magicColor; // 魔法選択の色
        [SerializeField] private Color itemColor; // アイテム選択の色

        private List<PooledObject> guides = new(); // 表示中のガイドオブジェクトのリスト

        // ガイドを表示する関数
        public void Show(int posX, int posY, CommandState currentState)
        {
            PooledObject guide = guidePool.GetPooledObject();
            var renderer = guide.GetComponent<Renderer>();

            // ガイドの色をコマンドの種類に応じて変更
            switch (currentState)
            {
                case CommandState.MoveSelect:
                    renderer.material.color = moveColor;
                    break;
                case CommandState.AttackSelect:
                    renderer.material.color = attackColor;
                    break;
                case CommandState.MagicSelect:
                    renderer.material.color = magicColor;
                    break;
                case CommandState.ItemSelect:
                    renderer.material.color = itemColor;
                    break;
            }

            guide.transform.position = new Vector3(posX, 0.5f, posY);
            guides.Add(guide);
        }

        // ガイドをクリアする関数
        public void Clear()
        {
            foreach (var guide in guides)
            {
                guide.Release();
            }
            guides.Clear();
        }
    }
}
