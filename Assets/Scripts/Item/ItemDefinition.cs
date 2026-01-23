using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Scriptable Objects/ItemDefinition")]

public class ItemDefinition : ScriptableObject, ISelectableData
{
    public int id;
    public string name;
    public string explanation;
    public Sprite icon;

    public string DisplayName => name;
    public Sprite Icon => icon;
}