using UnityEngine;

namespace Player
{
    public class InputManager : MonoBehaviour
    {
        public InputCommand GetInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                return InputCommand.Up;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                return InputCommand.Down;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                return InputCommand.Left;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                return InputCommand.Right;

            if (Input.GetKeyDown(KeyCode.Z))
                return InputCommand.Decide;

            if (Input.GetKeyDown(KeyCode.X))
                return InputCommand.Cancel;

            return InputCommand.None;
        }
    }
}
