using UnityEngine;
using PongQuest.RPG;
using PongQuest.Managers;

namespace PongQuest.Combat
{
    /// <summary>
    /// Manages battle flow: start, victory, defeat, reset.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Combatants")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Health enemyHealth;

        [Header("References")]
        [SerializeField] private EnergyOrb energyOrb;

        public enum BattleState
        {
            Idle,
            Active,
            Victory,
            Defeat
        }

        private BattleState currentState = BattleState.Idle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to death events
            if (playerHealth != null)
                playerHealth.OnDeath += OnPlayerDeath;

            if (enemyHealth != null)
                enemyHealth.OnDeath += OnEnemyDeath;

            // Start battle
            StartBattle();
        }

        private void OnDestroy()
        {
            // Unsubscribe
            if (playerHealth != null)
                playerHealth.OnDeath -= OnPlayerDeath;

            if (enemyHealth != null)
                enemyHealth.OnDeath -= OnEnemyDeath;
        }

        public void StartBattle()
        {
            currentState = BattleState.Active;
            Debug.Log("[BattleManager] Battle Started!");

            // Notify game state manager
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameStateManager.GameState.Battle);
            }
        }

        private void OnPlayerDeath()
        {
            if (currentState != BattleState.Active) return;

            currentState = BattleState.Defeat;
            Debug.Log("<color=red>[BattleManager] DEFEAT! Player HP reached 0!</color>");

            // Stop the ball
            if (energyOrb != null)
            {
                energyOrb.ResetToCenter();
            }

            // Show defeat UI (we'll add this later)
            Invoke(nameof(ShowDefeatScreen), 2f);
        }

        private void OnEnemyDeath()
        {
            if (currentState != BattleState.Active) return;

            currentState = BattleState.Victory;
            Debug.Log("<color=green>[BattleManager] VICTORY! Enemy HP reached 0!</color>");

            // Stop the ball
            if (energyOrb != null)
            {
                energyOrb.ResetToCenter();
            }

            // Show victory UI (we'll add this later)
            Invoke(nameof(ShowVictoryScreen), 2f);
        }

        private void ShowVictoryScreen()
        {
            Debug.Log("[BattleManager] Victory screen would appear here!");
            // Placeholder - we'll create proper UI in Phase 2
        }

        private void ShowDefeatScreen()
        {
            Debug.Log("[BattleManager] Defeat screen would appear here!");
            // Placeholder - we'll create proper UI in Phase 2
        }

        /// <summary>
        /// Reset battle (for testing or retry)
        /// </summary>
        public void ResetBattle()
        {
            playerHealth.FullHeal();
            enemyHealth.FullHeal();

            if (energyOrb != null)
            {
                energyOrb.ResetToCenter();
            }

            currentState = BattleState.Idle;
            StartBattle();
        }
    }
}