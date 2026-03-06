using UnityEngine;

namespace SkyRuins.Common
{
    // プレイヤーの入力コマンドを表す列挙型
    
    public enum InputCommand
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Decide,   // Z
        Cancel   // X
    }
}