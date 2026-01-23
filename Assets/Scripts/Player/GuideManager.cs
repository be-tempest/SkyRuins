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
