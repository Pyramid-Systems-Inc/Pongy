using UnityEngine;
using Unity.Cinemachine;

namespace PongQuest.Combat
{
    /// <summary>
    /// Triggers camera shake effects via Cinemachine Impulse.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Impulse Sources")]
        [SerializeField] private CinemachineImpulseSource damageShake;
        [SerializeField] private CinemachineImpulseSource paddleHitShake;

        [Header("Shake Intensities")]
        [SerializeField] private float damageShakeForce = 1.2f;
        [SerializeField] private float paddleHitShakeForce = 0.3f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create impulse sources if not assigned
            if (damageShake == null)
            {
                damageShake = gameObject.AddComponent<CinemachineImpulseSource>();
                ConfigureImpulse(damageShake, 0.2f);
            }

            if (paddleHitShake == null)
            {
                paddleHitShake = gameObject.AddComponent<CinemachineImpulseSource>();
                ConfigureImpulse(paddleHitShake, 0.1f);
            }
        }

        private void ConfigureImpulse(CinemachineImpulseSource impulse, float duration)
        {
            impulse.ImpulseDefinition.ImpulseDuration = duration;
            impulse.DefaultVelocity = Vector3.one;
        }

        /// <summary>
        /// Shake when a player takes damage
        /// </summary>
        public void ShakeOnDamage(float intensity = 1f)
        {
            if (damageShake != null)
            {
                damageShake.GenerateImpulse(damageShakeForce * intensity);
            }
        }

        /// <summary>
        /// Small shake when paddle hits ball
        /// </summary>
        public void ShakeOnPaddleHit(float intensity = 1f)
        {
            if (paddleHitShake != null)
            {
                paddleHitShake.GenerateImpulse(paddleHitShakeForce * intensity);
            }
        }

        /// <summary>
        /// Custom shake with specific force
        /// </summary>
        public void ShakeCustom(float force)
        {
            if (damageShake != null)
            {
                damageShake.GenerateImpulse(force);
            }
        }
    }
}