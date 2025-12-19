using UnityEngine;

namespace Data
{
    public class ItemData : MonoBehaviour
    {
        [SerializeField] private int _itemID;
        public int itemID => _itemID;
    }
}
