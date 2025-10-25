using System;
using System.Collections.Generic;
using UnityEngine;

namespace PongQuest.RPG
{
    /// <summary>
    /// Represents a single character stat (PWR, AGI, GRT, FCS).
    /// Supports base value + modifiers from equipment/buffs.
    /// </summary>
    [System.Serializable]
    public class Stat
    {
        [SerializeField] private string statName;
        [SerializeField] private float baseValue;

        private List<StatModifier> modifiers = new List<StatModifier>();

        public string StatName => statName;
        public float BaseValue => baseValue;

        // Events
        public event Action OnStatChanged;

        public Stat(string name, float baseValue)
        {
            this.statName = name;
            this.baseValue = baseValue;
        }

        /// <summary>
        /// Get the final calculated value (base + all modifiers)
        /// </summary>
        public float GetValue()
        {
            float finalValue = baseValue;

            // Apply all modifiers
            foreach (var modifier in modifiers)
            {
                finalValue += modifier.Value;
            }

            return Mathf.Max(0, finalValue); // Stats can't go negative
        }

        /// <summary>
        /// Set the base value
        /// </summary>
        public void SetBaseValue(float value)
        {
            baseValue = value;
            OnStatChanged?.Invoke();
        }

        /// <summary>
        /// Increase base value by amount
        /// </summary>
        public void IncreaseBase(float amount)
        {
            baseValue += amount;
            OnStatChanged?.Invoke();
        }

        /// <summary>
        /// Add a temporary modifier (from equipment, buffs, etc.)
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            OnStatChanged?.Invoke();
        }

        /// <summary>
        /// Remove a specific modifier
        /// </summary>
        public void RemoveModifier(StatModifier modifier)
        {
            modifiers.Remove(modifier);
            OnStatChanged?.Invoke();
        }

        /// <summary>
        /// Remove all modifiers from a specific source (e.g., when unequipping an item)
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            modifiers.RemoveAll(m => m.Source == source);
            OnStatChanged?.Invoke();
        }

        /// <summary>
        /// Clear all modifiers
        /// </summary>
        public void ClearModifiers()
        {
            modifiers.Clear();
            OnStatChanged?.Invoke();
        }
    }

    /// <summary>
    /// Represents a temporary modification to a stat (from gear, buffs, etc.)
    /// </summary>
    [System.Serializable]
    public class StatModifier
    {
        public float Value { get; private set; }
        public object Source { get; private set; } // The source (equipment, buff, etc.)

        public StatModifier(float value, object source)
        {
            Value = value;
            Source = source;
        }
    }
}