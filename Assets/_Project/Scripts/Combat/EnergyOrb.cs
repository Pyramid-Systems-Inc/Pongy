using UnityEngine;

namespace PongQuest.Combat
{
    /// <summary>
    /// The Energy Orb (ball) used in Pong battles.
    /// Handles movement, bouncing, and deflection physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnergyOrb : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 7f;
        [SerializeField] private float maxSpeed = 15f;

        [Header("Launch Settings")]
        [SerializeField] private float launchAngleMin = -45f;
        [SerializeField] private float launchAngleMax = 45f;
        [SerializeField] private bool launchTowardsPlayer = true; // For testing

        [Header("Deflection Settings")]
        [Tooltip("How much hitting the paddle edge affects angle (0-1)")]
        [SerializeField] private float deflectionStrength = 0.7f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Components
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider;

        // State
        private float currentSpeed;
        private bool isLaunched = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            circleCollider = GetComponent<CircleCollider2D>();
            currentSpeed = baseSpeed;
        }

        private void Start()
        {
            // Auto-launch on start (temporary, will be controlled by BattleManager later)
            Invoke(nameof(Launch), 1f); // 1 second delay
        }

        private void FixedUpdate()
        {
            if (isLaunched)
            {
                MaintainConstantSpeed();
            }
        }

        /// <summary>
        /// Launch the orb in a random direction
        /// </summary>
        public void Launch()
        {
            // Random angle within specified range
            float randomAngle = Random.Range(launchAngleMin, launchAngleMax);

            // Convert to radians
            float angleInRadians = randomAngle * Mathf.Deg2Rad;

            // Create direction vector
            Vector2 direction;
            if (launchTowardsPlayer)
            {
                // Launch towards left (player side)
                direction = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
            }
            else
            {
                // Launch towards right (enemy side)
                direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
            }

            // Apply velocity
            rb.linearVelocity = direction.normalized * currentSpeed;
            isLaunched = true;

            if (showDebugLogs)
                Debug.Log($"[EnergyOrb] Launched at {randomAngle}° with speed {currentSpeed}");
        }

        /// <summary>
        /// Relaunch with specific direction (for serves)
        /// </summary>
        public void Launch(Vector2 direction)
        {
            rb.linearVelocity = direction.normalized * currentSpeed;
            isLaunched = true;
        }

        /// <summary>
        /// Reset orb to center and stop
        /// </summary>
        public void ResetToCenter()
        {
            transform.position = Vector3.zero;
            rb.linearVelocity = Vector2.zero;
            isLaunched = false;
        }

        /// <summary>
        /// Keeps the ball moving at constant speed (prevents slowdown)
        /// </summary>
        private void MaintainConstantSpeed()
        {
            // Get current velocity
            Vector2 velocity = rb.linearVelocity;

            // Normalize and multiply by desired speed
            velocity = velocity.normalized * currentSpeed;

            // Clamp to max speed (for when Power stat boosts it later)
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }

            rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Handle collision with paddles and walls
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if we hit a paddle
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
            {
                HandlePaddleDeflection(collision);
            }
            else
            {
                // Hit a wall - just reflect normally
                if (showDebugLogs)
                    Debug.Log($"[EnergyOrb] Bounced off {collision.gameObject.name}");
            }
        }

        /// <summary>
        /// Creates angle-based deflection when hitting a paddle
        /// </summary>
        private void HandlePaddleDeflection(Collision2D collision)
        {
            // Get the paddle's collider
            Collider2D paddleCollider = collision.collider;

            // Get contact point
            ContactPoint2D contact = collision.GetContact(0);

            // Calculate where the ball hit on the paddle (-1 = bottom edge, 0 = center, 1 = top edge)
            float paddleHeight = paddleCollider.bounds.size.y;
            float paddleCenter = paddleCollider.bounds.center.y;
            float hitPoint = contact.point.y;

            float relativeIntersectY = (hitPoint - paddleCenter) / (paddleHeight / 2f);
            relativeIntersectY = Mathf.Clamp(relativeIntersectY, -1f, 1f);

            // Calculate bounce angle (max 75 degrees)
            float maxBounceAngle = 75f * Mathf.Deg2Rad;
            float bounceAngle = relativeIntersectY * maxBounceAngle * deflectionStrength;

            // Determine direction (left or right)
            float directionX = collision.gameObject.CompareTag("Player") ? 1f : -1f;

            // Create new velocity direction
            Vector2 newDirection = new Vector2(
                directionX * Mathf.Cos(bounceAngle),
                Mathf.Sin(bounceAngle)
            );

            // Apply velocity
            rb.linearVelocity = newDirection.normalized * currentSpeed;

            if (showDebugLogs)
            {
                Debug.Log($"[EnergyOrb] Hit {collision.gameObject.name} at relative Y: {relativeIntersectY:F2}, " +
                          $"Bounce angle: {bounceAngle * Mathf.Rad2Deg:F1}°");
            }
        }

        /// <summary>
        /// Increase ball speed (for Power stat later)
        /// </summary>
        public void AddSpeed(float amount)
        {
            currentSpeed = Mathf.Min(currentSpeed + amount, maxSpeed);
        }

        /// <summary>
        /// Reset to base speed
        /// </summary>
        public void ResetSpeed()
        {
            currentSpeed = baseSpeed;
        }

        // Visualize direction in editor
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && isLaunched)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
            }
        }
    }
}