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
}
