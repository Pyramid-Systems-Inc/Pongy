using UnityEngine;

namespace PongQuest.RPG
{
    /// <summary>
    /// Holds all stats for a character (Player or Enemy).
    /// Attach this to any GameObject that needs stats.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [Header("Core Stats")]
        [SerializeField] private float basePower = 10f;
        [SerializeField] private float baseAgility = 10f;
        [SerializeField] private float baseGrit = 10f;
        [SerializeField] private float baseFocus = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // The actual stat objects
        public Stat Power { get; private set; }
        public Stat Agility { get; private set; }
        public Stat Grit { get; private set; }
        public Stat Focus { get; private set; }

        private void Awake()
        {
            InitializeStats();
        }

        private void InitializeStats()
        {
            // Create stat instances
            Power = new Stat("Power", basePower);
            Agility = new Stat("Agility", baseAgility);
            Grit = new Stat("Grit", baseGrit);
            Focus = new Stat("Focus", baseFocus);

            if (showDebugLogs)
            {
                Debug.Log($"[CharacterStats] {gameObject.name} initialized with stats:");
                Debug.Log($"  PWR: {Power.GetValue()} | AGI: {Agility.GetValue()} | GRT: {Grit.GetValue()} | FCS: {Focus.GetValue()}");
            }
        }

        /// <summary>
        /// Get a summary of all stats (for UI display)
        /// </summary>
        public string GetStatsSummary()
        {
            return $"PWR: {Power.GetValue():F0} | AGI: {Agility.GetValue():F0} | GRT: {Grit.GetValue():F0} | FCS: {Focus.GetValue():F0}";
        }

        /// <summary>
        /// Increase a stat's base value (for leveling up)
        /// </summary>
        public void IncreaseStat(string statName, float amount)
        {
            switch (statName.ToUpper())
            {
                case "POWER":
                case "PWR":
                    Power.IncreaseBase(amount);
                    break;
                case "AGILITY":
                case "AGI":
                    Agility.IncreaseBase(amount);
                    break;
                case "GRIT":
                case "GRT":
                    Grit.IncreaseBase(amount);
                    break;
                case "FOCUS":
                case "FCS":
                    Focus.IncreaseBase(amount);
                    break;
                default:
                    Debug.LogWarning($"[CharacterStats] Unknown stat: {statName}");
                    break;
            }

            if (showDebugLogs)
                Debug.Log($"[CharacterStats] {statName} increased by {amount}. New value: {GetStatValue(statName)}");
        }

        /// <summary>
        /// Get a specific stat's value
        /// </summary>
        public float GetStatValue(string statName)
        {
            switch (statName.ToUpper())
            {
                case "POWER":
                case "PWR":
                    return Power.GetValue();
                case "AGILITY":
                case "AGI":
                    return Agility.GetValue();
                case "GRIT":
                case "GRT":
                    return Grit.GetValue();
                case "FOCUS":
                case "FCS":
                    return Focus.GetValue();
                default:
                    return 0f;
            }
        }
    }
}