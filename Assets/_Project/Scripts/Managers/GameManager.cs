using UnityEngine;

namespace PongQuest.Managers
{
    /// <summary>
    /// Central manager for game-wide systems. Persists across scenes.
    /// Singleton pattern for easy global access.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (showDebugLogs)
                Debug.Log("[GameManager] Initialized and set to DontDestroyOnLoad");
        }

        /// <summary>
        /// Call this when quitting the game
        /// </summary>
        public void QuitGame()
        {
            if (showDebugLogs)
                Debug.Log("[GameManager] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}