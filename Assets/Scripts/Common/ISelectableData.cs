using UnityEngine;

namespace SkyRuins.Common
{
    // 選択可能なデータのインターフェース
    
    public interface ISelectableData
    {
        string DisplayName { get; }
        Sprite Icon { get; }
        string Explanation { get; }
    }
}