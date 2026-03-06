using UnityEngine;

namespace SkyRuins
{
    // SEの種類
    public enum SEType
    {
        Footstep,
        Attack,
        Flame,
        Ice,
        Item,
        Slide,
        Select,
        Decide,
        Cancel
    }

    // ゲーム全体のオーディオ管理

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance; // シングルトンインスタンス

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource; // BGM用オーディオソース
        [SerializeField] private AudioSource seSource; // SE用オーディオソース

        [Header("BGM Clips")]
        [SerializeField] private AudioClip titleBgm; // タイトルBGM
        [SerializeField] private AudioClip gameBgm; // インゲームBGM
        [SerializeField] private AudioClip gameOver; // ゲームオーバーBGM

        [Header("SE Clips")]
        [SerializeField] private AudioClip footstep; // 足音
        [SerializeField] private AudioClip attack; // 攻撃
        [SerializeField] private AudioClip flame; // フレイム
        [SerializeField] private AudioClip ice; // アイスピラー
        [SerializeField] private AudioClip slide; // スライド
        [SerializeField] private AudioClip item; // アイテム取得
        [SerializeField] private AudioClip select; // 選択
        [SerializeField] private AudioClip decide; // 決定
        [SerializeField] private AudioClip cancel; // キャンセル


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

        // タイトルBGM再生
        public void PlayTitleBGM()
        {
            bgmSource.clip = titleBgm;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // インゲームBGM再生
        public void PlayGameBGM()
        {
            bgmSource.clip = gameBgm;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // ゲームオーバーBGM再生
        public void PlayGameOverBGM()
        {
            bgmSource.clip = gameOver;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // SE再生
        public void PlaySE(SEType type)
        {
            switch (type)
            {
                case SEType.Footstep: // 足音
                    seSource.PlayOneShot(footstep);
                    break;

                case SEType.Attack: // 攻撃
                    seSource.PlayOneShot(attack);
                    break;

                case SEType.Flame: // フレイム
                    seSource.PlayOneShot(flame);
                    break;

                case SEType.Ice: // アイスピラー
                    seSource.PlayOneShot(ice);
                    break;

                case SEType.Item: // アイテム取得
                    seSource.PlayOneShot(item);
                    break;

                case SEType.Slide: // スライド
                    seSource.PlayOneShot(slide);
                    break;

                case SEType.Select: // 選択
                    seSource.PlayOneShot(select);
                    break;

                case SEType.Decide: // 決定
                    seSource.PlayOneShot(decide);
                    break;

                case SEType.Cancel: // キャンセル
                    seSource.PlayOneShot(cancel);
                    break;
            }
        }

        // SE停止
        public void StopSE()
        {
            seSource.Stop();
        }
    }
}
