using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PongQuest.RPG;

namespace PongQuest.UI
{
    /// <summary>
    /// Displays a Health component's HP on a UI bar.
    /// Updates in real-time when health changes.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health targetHealth;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Settings")]
        [SerializeField] private bool showNumbers = true;
        [SerializeField] private string labelPrefix = "HP: ";

        [Header("Animation")]
        [SerializeField] private bool smoothTransition = true;
        [SerializeField] private float smoothSpeed = 10f;

        private float targetFillAmount = 1f;

        private void OnEnable()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHealthBar;
                // Initialize with current values
                UpdateHealthBar(targetHealth.CurrentHP, targetHealth.MaxHP);
            }
        }

        private void OnDisable()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void Update()
        {
            // Smooth fill animation
            if (smoothTransition && fillImage != null)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            }
        }

        /// <summary>
        /// Update the health bar when health changes
        /// </summary>
        private void UpdateHealthBar(int currentHP, int maxHP)
        {
            // Calculate fill percentage
            float fillPercent = (float)currentHP / maxHP;
            targetFillAmount = fillPercent;

            // Update fill immediately if not using smooth transition
            if (!smoothTransition && fillImage != null)
            {
                fillImage.fillAmount = targetFillAmount;
            }

            // Update text label
            if (labelText != null && showNumbers)
            {
                labelText.text = $"{labelPrefix}{currentHP}/{maxHP}";
            }
        }

        /// <summary>
        /// Manually set the target health (in case it wasn't assigned in inspector)
        /// </summary>
        public void SetTargetHealth(Health health)
        {
            // Unsubscribe from old health
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHealthBar;
            }

            // Subscribe to new health
            targetHealth = health;
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(targetHealth.CurrentHP, targetHealth.MaxHP);
            }
        }
    }
}