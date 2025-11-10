using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GamePhase { Add, PlayerMove, Slide, Delete }
    public GamePhase CurrentPhase { get; private set; }

    // イベント通知
    public event Action<GamePhase> OnPhaseChanged;
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
        SetPhase(GamePhase.Add);
    }

    void Update()
    {
        // フェーズ切り替え
        if (Input.GetKeyDown(KeyCode.A) && CurrentPhase == GamePhase.Add)
        // if (CurrentPhase == GamePhase.Add)
        {
            OnRequestAdd?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.S) && CurrentPhase == GamePhase.Slide)
        // else if (CurrentPhase == GamePhase.Slide)
        {
            OnRequestSlide?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.D) && CurrentPhase == GamePhase.Delete)
        // else if (CurrentPhase == GamePhase.Delete)
        {
            OnRequestDelete?.Invoke();
        }
    }

    // フェーズ変更
    public void SetPhase(GamePhase newPhase)
    {
        if (CurrentPhase == newPhase) return;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
        Debug.Log($"[GameStateManager] Phase -> {newPhase}");
    }
}
