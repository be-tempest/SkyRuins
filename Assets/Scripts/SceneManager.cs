using UnityEngine;

namespace SkyRuins
{
    // シーン遷移を管理するクラス
    
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        private void Awake()
        {
            // 二重生成防止
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // タイトルシーンへ遷移
        public void OnEnterTitle()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }

        // インゲームシーンへ遷移
        public void OnEnterInGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
        }

        // ゲーム終了
        public void OnExitGame()
        {
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
}
