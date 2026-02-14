using UnityEngine;

public interface ISelectableData
{
    string DisplayName { get; }
    Sprite Icon { get; }
    string Explanation { get; }
}