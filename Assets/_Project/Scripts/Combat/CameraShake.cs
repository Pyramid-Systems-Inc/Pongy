using UnityEngine;
using Unity.Cinemachine;

namespace PongQuest.Combat
{
    /// <summary>
    /// Triggers camera shake effects via Cinemachine 3.
    /// Uses coroutines to temporarily boost noise.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Shake Settings")]
        [SerializeField] private float damageShakeIntensity = 3f;
        [SerializeField] private float damageShakeDuration = 0.3f;
        [SerializeField] private float paddleHitShakeIntensity = 0.8f;
        [SerializeField] private float paddleHitShakeDuration = 0.1f;

        private CinemachineBasicMultiChannelPerlin noise;
        private float shakeTimer;
        private float currentShakeIntensity;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-find virtual camera if not assigned
            if (virtualCamera == null)
            {
                virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            // Get the noise component
            if (virtualCamera != null)
            {
                noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

                if (noise == null)
                {
                    Debug.LogWarning("[CameraShake] No CinemachineBasicMultiChannelPerlin found on camera. Adding it now...");
                    noise = virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
                }
            }
            else
            {
                Debug.LogError("[CameraShake] No CinemachineCamera found in scene!");
            }
        }

        private void Update()
        {
            // Decay shake over time
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;

                if (noise != null)
                {
                    // Gradually reduce amplitude
                    float amplitude = Mathf.Lerp(0f, currentShakeIntensity, shakeTimer / damageShakeDuration);
                    noise.AmplitudeGain = amplitude;
                }
            }
            else if (noise != null)
            {
                noise.AmplitudeGain = 0f;
            }
        }

        /// <summary>
        /// Shake when a player takes damage
        /// </summary>
        public void ShakeOnDamage(float intensity = 1f)
        {
            TriggerShake(damageShakeIntensity * intensity, damageShakeDuration);
        }

        /// <summary>
        /// Small shake when paddle hits ball
        /// </summary>
        public void ShakeOnPaddleHit(float intensity = 1f)
        {
            TriggerShake(paddleHitShakeIntensity * intensity, paddleHitShakeDuration);
        }

        /// <summary>
        /// Custom shake with specific force and duration
        /// </summary>
        public void ShakeCustom(float intensity, float duration)
        {
            TriggerShake(intensity, duration);
        }

        private void TriggerShake(float intensity, float duration)
        {
            if (noise == null) return;

            currentShakeIntensity = intensity;
            shakeTimer = duration;
            noise.AmplitudeGain = intensity;
        }
    }
}