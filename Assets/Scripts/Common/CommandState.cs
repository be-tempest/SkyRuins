using UnityEngine;

namespace SkyRuins.Common
{
    // プレイヤーのコマンド選択状態を表す列挙型
    
    public enum CommandState
    {
        None,
        MainSelect,
        MoveSelect,
        AttackSelect,
        MagicSelect,
        MagicExecute,
        ItemSelect,
        ItemExecute
    }
}