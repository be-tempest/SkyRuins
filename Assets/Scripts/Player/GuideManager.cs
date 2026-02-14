using UnityEngine;
using System.Collections.Generic;
using Pool;

namespace Player
{
    public class GuideManager : MonoBehaviour
    {
        [SerializeField] private ObjectPool guidePool;

        [SerializeField] private Color moveColor;
        [SerializeField] private Color attackColor;
        [SerializeField] private Color magicColor;
        [SerializeField] private Color itemColor;

        private List<PooledObject> guides = new();

        public void Show(int posX, int posY, CommandState currentState)
        {
            PooledObject guide = guidePool.GetPooledObject();
            var renderer = guide.GetComponent<Renderer>();

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
