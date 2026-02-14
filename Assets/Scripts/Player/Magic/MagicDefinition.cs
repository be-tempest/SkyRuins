using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    [CreateAssetMenu(fileName = "MagicDefinition", menuName = "Scriptable Objects/MagicDefinition")]

    public class MagicDefinition : ScriptableObject, ISelectableData
    {
        public int id;
        public string name;
        public Sprite icon;
        public string explanation;

        public string DisplayName => name;
        public Sprite Icon => icon;
        public string Explanation => explanation;

        public int costMP;
        public int power;
        public List<Vector2Int> magicRange;
        public int animationType;
        public MagicExecuter executerPrefab;
        public GameObject effectPrefab;
        public SEType seType;
    }
}
