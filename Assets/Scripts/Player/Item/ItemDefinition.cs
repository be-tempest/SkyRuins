using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Scriptable Objects/ItemDefinition")]

    public class ItemDefinition : ScriptableObject, ISelectableData
    {
        public int id;
        public string name;
        public Sprite icon;
        public string explanation;

        public string DisplayName => name;
        public Sprite Icon => icon;
        public string Explanation => explanation;

        public int power;
        public ItemExecutor executerPrefab;
        public GameObject effectPrefab;
    }
}