using UnityEngine;
using PongQuest.RPG;

namespace PongQuest.Combat
{
    /// <summary>
    /// Improved AI that tracks the ball's Y position.
    /// Uses CharacterStats for agility-based movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleEnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float baseMoveSpeed = 6f;
#pragma warning disable 0414
        [SerializeField] private float reactionSpeed = 8f; // Reserved for future AI difficulty
#pragma warning restore 0414

        [Header("Stat Scaling")]
        [SerializeField] private float agilityMultiplier = 0.5f;

        [Header("References")]
        [SerializeField] private Transform ball;

        [Header("Arena Bounds")]
        [SerializeField] private float minY = -3.5f;
        [SerializeField] private float maxY = 3.5f;

        private Rigidbody2D rb;
        private CharacterStats stats;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<CharacterStats>();

            // Configure Rigidbody2D
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            // Auto-find ball if not assigned
            if (ball == null)
            {
                GameObject ballObj = GameObject.Find("EnergyOrb");
                if (ballObj != null)
                    ball = ballObj.transform;
            }
        }

        private void FixedUpdate()
        {
            if (ball == null) return;

            // Calculate actual move speed based on stats
            float actualMoveSpeed = baseMoveSpeed;

            if (stats != null)
            {
                // AGI increases movement speed (same as player)
                actualMoveSpeed = baseMoveSpeed + (stats.Agility.GetValue() * agilityMultiplier);
            }

            // Track ball's Y position
            float targetY = ball.position.y;

            // Smoothly move towards target
            float newY = Mathf.MoveTowards(transform.position.y, targetY, actualMoveSpeed * Time.fixedDeltaTime);

            // Clamp to arena bounds
            newY = Mathf.Clamp(newY, minY, maxY);

            // Move paddle
            Vector2 newPosition = new Vector2(transform.position.x, newY);
            rb.MovePosition(newPosition);
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize movement boundaries
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(transform.position.x, minY, 0), new Vector3(transform.position.x, maxY, 0));
        }
    }
}