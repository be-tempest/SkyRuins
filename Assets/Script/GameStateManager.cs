using System;
using UnityEngine;

// フェーズの「中央管理」を担当するシンプルなクラス。
// - フェーズ状態の保持
// - フェーズ変更イベントの発行
// - 現状はキー入力 (A / S / D) によるリクエストを受け付けて、Board や他システムへ要請イベントを出す
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GamePhase { Add, PlayerMove, Slide, Delete }

    public GamePhase CurrentPhase { get; private set; }

    // フェーズが変わったことを通知
    public event Action<GamePhase> OnPhaseChanged;

    // 外部（BoardManager など）に「この操作をやってください」と要求するイベント
    // 例: Add を押したら BoardManager が AddBlocks() を実行し、その後 SetPhase(PlayerMove) を呼ぶ
    public event Action OnRequestAdd;
    public event Action OnRequestSlide;
    public event Action OnRequestDelete;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        // 初期フェーズ
        SetPhase(GamePhase.Add);
    }

    void Update()
    {
        // 現時点では開発用にキーでフェーズ処理のリクエストを飛ばせるようにしておきます。
        // 将来的に自動遷移（PlayerMove -> Slide ...）に変えたい場合はここを変更すればよい。
        if (Input.GetKeyDown(KeyCode.A) && CurrentPhase == GamePhase.Add)
        {
            OnRequestAdd?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.S) && CurrentPhase == GamePhase.Slide)
        {
            OnRequestSlide?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.D) && CurrentPhase == GamePhase.Delete)
        {
            OnRequestDelete?.Invoke();
        }
    }

    // 外部からフェーズを明示的に変更する
    public void SetPhase(GamePhase newPhase)
    {
        if (CurrentPhase == newPhase) return;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
        Debug.Log($"[GameStateManager] Phase -> {newPhase}");
    }
}
