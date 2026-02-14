using UnityEngine;

namespace Player
{
    public class ItemData : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _itemDefinition;
        public ItemDefinition itemDefinition => _itemDefinition;
    }
}

