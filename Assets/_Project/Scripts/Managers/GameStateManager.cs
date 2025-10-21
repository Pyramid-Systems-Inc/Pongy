using UnityEngine;
using System;

namespace PongQuest.Managers
{
    /// <summary>
    /// Manages the current state of the game (Overworld, Battle, Menu, etc.)
    /// Uses events to notify other systems of state changes.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public enum GameState
        {
            MainMenu,
            Overworld,
            Battle,
            Paused,
            GameOver
        }

        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        // Events for state changes
        public static event Action<GameState> OnGameStateChanged;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Change the game state and notify all listeners
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameStateManager] State changed: {previousState} → {newState}");

            // Notify all subscribers
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Quick check methods
        /// </summary>
        public bool IsInBattle() => currentState == GameState.Battle;
        public bool IsPaused() => currentState == GameState.Paused;
    }
}