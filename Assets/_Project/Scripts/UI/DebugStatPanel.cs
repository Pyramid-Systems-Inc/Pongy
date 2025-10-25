using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PongQuest.RPG;

namespace PongQuest.UI
{
    /// <summary>
    /// Debug panel for testing stat values in real-time.
    /// Press Tab to show/hide.
    /// </summary>
    public class DebugStatPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterStats playerStats;
        [SerializeField] private CharacterStats enemyStats;

        [Header("UI Elements")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider powerSlider;
        [SerializeField] private Slider agilitySlider;
        [SerializeField] private Slider gritSlider;
        [SerializeField] private Slider focusSlider;

        [SerializeField] private TextMeshProUGUI powerLabel;
        [SerializeField] private TextMeshProUGUI agilityLabel;
        [SerializeField] private TextMeshProUGUI gritLabel;
        [SerializeField] private TextMeshProUGUI focusLabel;

        [SerializeField] private Toggle debugLogToggle;

        [Header("Settings")]
        [SerializeField] private bool startHidden = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        private bool isApplyingToEnemy = false;

        private void Start()
        {
            // Hide panel on start if configured
            if (startHidden)
            {
                panelRoot.SetActive(false);
            }

            // Initialize sliders with player's current stats
            if (playerStats != null)
            {
                powerSlider.value = playerStats.Power.GetValue();
                agilitySlider.value = playerStats.Agility.GetValue();
                gritSlider.value = playerStats.Grit.GetValue();
                focusSlider.value = playerStats.Focus.GetValue();
            }

            // Add listeners to sliders
            powerSlider.onValueChanged.AddListener(OnPowerChanged);
            agilitySlider.onValueChanged.AddListener(OnAgilityChanged);
            gritSlider.onValueChanged.AddListener(OnGritChanged);
            focusSlider.onValueChanged.AddListener(OnFocusChanged);

            // Initialize labels
            UpdateAllLabels();
        }

        private void Update()
        {
            // Toggle panel with Tab key
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            panelRoot.SetActive(!panelRoot.activeSelf);
        }

        private void OnPowerChanged(float value)
        {
            CharacterStats targetStats = isApplyingToEnemy ? enemyStats : playerStats;
            if (targetStats != null)
            {
                targetStats.Power.SetBaseValue(value);
                powerLabel.text = $"POWER: {value:F0}";
            }
        }

        private void OnAgilityChanged(float value)
        {
            CharacterStats targetStats = isApplyingToEnemy ? enemyStats : playerStats;
            if (targetStats != null)
            {
                targetStats.Agility.SetBaseValue(value);
                agilityLabel.text = $"AGILITY: {value:F0}";
            }
        }

        private void OnGritChanged(float value)
        {
            CharacterStats targetStats = isApplyingToEnemy ? enemyStats : playerStats;
            if (targetStats != null)
            {
                targetStats.Grit.SetBaseValue(value);
                gritLabel.text = $"GRIT: {value:F0}";
            }
        }

        private void OnFocusChanged(float value)
        {
            CharacterStats targetStats = isApplyingToEnemy ? enemyStats : playerStats;
            if (targetStats != null)
            {
                targetStats.Focus.SetBaseValue(value);
                focusLabel.text = $"FOCUS: {value:F0}";
            }
        }

        private void UpdateAllLabels()
        {
            powerLabel.text = $"POWER: {powerSlider.value:F0}";
            agilityLabel.text = $"AGILITY: {agilitySlider.value:F0}";
            gritLabel.text = $"GRIT: {gritSlider.value:F0}";
            focusLabel.text = $"FOCUS: {focusSlider.value:F0}";
        }

        /// <summary>
        /// Switch to modifying enemy stats instead of player
        /// </summary>
        public void ToggleApplyToEnemy()
        {
            isApplyingToEnemy = !isApplyingToEnemy;

            // Load current enemy stats into sliders
            if (isApplyingToEnemy && enemyStats != null)
            {
                powerSlider.value = enemyStats.Power.GetValue();
                agilitySlider.value = enemyStats.Agility.GetValue();
                gritSlider.value = enemyStats.Grit.GetValue();
                focusSlider.value = enemyStats.Focus.GetValue();
            }
            // Load current player stats into sliders
            else if (playerStats != null)
            {
                powerSlider.value = playerStats.Power.GetValue();
                agilitySlider.value = playerStats.Agility.GetValue();
                gritSlider.value = playerStats.Grit.GetValue();
                focusSlider.value = playerStats.Focus.GetValue();
            }

            Debug.Log($"[DebugStatPanel] Now modifying: {(isApplyingToEnemy ? "ENEMY" : "PLAYER")}");
        }

        /// <summary>
        /// Toggle debug logs for all systems
        /// </summary>
        public void ToggleDebugLogs()
        {
            bool showLogs = debugLogToggle.isOn;
            Debug.Log($"[DebugStatPanel] Debug logs: {(showLogs ? "ON" : "OFF")}");

            // You can add code here to toggle debug flags on other systems
        }
    }
}