using UnityEngine;

namespace PongQuest.Combat
{
    /// <summary>
    /// Very basic AI that tracks the ball's Y position.
    /// Temporary placeholder until Week 8.
    /// </summary>
    public class SimpleEnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float reactionDelay = 0.1f;

        [Header("References")]
        [SerializeField] private Transform ball;

        private float targetY;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
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

            // Track ball's Y position with slight delay
            targetY = Mathf.Lerp(transform.position.y, ball.position.y, moveSpeed * reactionDelay * Time.fixedDeltaTime);

            // Clamp to arena bounds
            targetY = Mathf.Clamp(targetY, -3.5f, 3.5f);

            // Move paddle
            Vector2 newPosition = new Vector2(transform.position.x, targetY);
            rb.MovePosition(newPosition);
        }
    }
}