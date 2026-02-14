using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public void OnEnterTitle()
    {
        // タイトルシーン読み込み
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    public void OnEnterInGame()
    {
        // インゲームシーン読み込み
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
    }

    public void OnEnterResult()
    {
        // リザルトシーン読み込み
        UnityEngine.SceneManagement.SceneManager.LoadScene("Result");
    }

    public void OnExitGame()
    {
        // ゲーム終了
        if (Application.isEditor)
        {
            // エディタ上ではプレイモードを停止
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            // ビルドされたゲームではアプリケーションを終了
            Application.Quit();
        }
    }
}
