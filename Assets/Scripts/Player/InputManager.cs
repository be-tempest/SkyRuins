using UnityEngine;
using SkyRuins.Common;

namespace SkyRuins.Player
{
    // プレイヤーの入力を管理するクラス

    public class InputManager : MonoBehaviour
    {
        public InputCommand GetInput()
        {
            // 矢印キーで移動、Zキーで決定、Xキーでキャンセル
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.PlaySE(SEType.Select);
                return InputCommand.Up;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.PlaySE(SEType.Select);
                return InputCommand.Down;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AudioManager.Instance.PlaySE(SEType.Select);
                return InputCommand.Left;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AudioManager.Instance.PlaySE(SEType.Select);
                return InputCommand.Right;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                AudioManager.Instance.PlaySE(SEType.Decide);
                return InputCommand.Decide;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                AudioManager.Instance.PlaySE(SEType.Cancel);
                return InputCommand.Cancel;
            }

            return InputCommand.None;
        }
    }
}
