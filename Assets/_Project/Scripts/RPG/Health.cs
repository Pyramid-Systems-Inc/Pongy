using UnityEngine;
using System;

namespace PongQuest.RPG
{
    /// <summary>
    /// Generic Health component for any entity (Player, Enemy, Boss).
    /// Handles HP, damage, healing, and death.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int currentHP;
        [SerializeField] private bool isInvulnerable = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Events
        public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)
        public event Action<int> OnDamageTaken; // (damageAmount)
        public event Action<int> OnHealed; // (healAmount)
        public event Action OnDeath;

        // Properties
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public float HealthPercent => (float)currentHP / maxHP;
        public bool IsAlive => currentHP > 0;
        public bool IsDead => currentHP <= 0;

        private void Awake()
        {
            // Initialize HP to max on start
            currentHP = maxHP;
        }

        /// <summary>
        /// Deal damage to this entity
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isInvulnerable || IsDead) return;

            // Ensure damage is at least 1
            damage = Mathf.Max(1, damage);

            currentHP -= damage;
            currentHP = Mathf.Max(0, currentHP); // Prevent negative HP

            if (showDebugLogs)
                Debug.Log($"[Health] {gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");

            // Trigger events
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHP, maxHP);

            // Check for death
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this entity
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead) return;

            int oldHP = currentHP;
            currentHP += amount;
            currentHP = Mathf.Min(currentHP, maxHP); // Cap at max HP

            int actualHealing = currentHP - oldHP;

            if (showDebugLogs && actualHealing > 0)
                Debug.Log($"[Health] {gameObject.name} healed for {actualHealing}. HP: {currentHP}/{maxHP}");

            if (actualHealing > 0)
            {
                OnHealed?.Invoke(actualHealing);
                OnHealthChanged?.Invoke(currentHP, maxHP);
            }
        }

        /// <summary>
        /// Set HP to a specific value
        /// </summary>
        public void SetHP(int value)
        {
            currentHP = Mathf.Clamp(value, 0, maxHP);
            OnHealthChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
                Die();
        }

        /// <summary>
        /// Fully restore health
        /// </summary>
        public void FullHeal()
        {
            Heal(maxHP);
        }

        /// <summary>
        /// Set max HP (useful for level-ups and stat changes)
        /// </summary>
        public void SetMaxHP(int newMaxHP)
        {
            maxHP = newMaxHP;
            currentHP = Mathf.Min(currentHP, maxHP); // Adjust current if over new max
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }

        /// <summary>
        /// Handle death
        /// </summary>
        private void Die()
        {
            if (showDebugLogs)
                Debug.Log($"[Health] {gameObject.name} has died!");

            OnDeath?.Invoke();
        }

        /// <summary>
        /// Revive with specified HP
        /// </summary>
        public void Revive(int reviveHP)
        {
            currentHP = Mathf.Clamp(reviveHP, 1, maxHP);
            OnHealthChanged?.Invoke(currentHP, maxHP);

            if (showDebugLogs)
                Debug.Log($"[Health] {gameObject.name} revived with {currentHP} HP");
        }

        /// <summary>
        /// Toggle invulnerability (for testing/special states)
        /// </summary>
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }
    }
}