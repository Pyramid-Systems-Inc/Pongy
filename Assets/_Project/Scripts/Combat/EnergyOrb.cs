using UnityEngine;
using PongQuest.RPG;

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

        [Header("Damage Settings")]
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private float powerDamageMultiplier = 0.5f; // PWR increases damage
        [SerializeField] private float powerSpeedBoost = 0.3f; // PWR increases ball speed

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Components
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider;

        // State
        private float currentSpeed;
        private bool isLaunched = false;
        private string lastHitBy = ""; // "Player" or "Enemy"

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

            // Apply velocity with power boost
            float finalSpeed = currentSpeed;

            // Check if the paddle has stats (Player/Enemy might have CharacterStats)
            CharacterStats paddleStats = collision.gameObject.GetComponent<CharacterStats>();
            if (paddleStats != null)
            {
                // Power increases ball speed
                float powerBoost = paddleStats.Power.GetValue() * powerSpeedBoost;
                finalSpeed += powerBoost;

                if (showDebugLogs)
                    Debug.Log($"[EnergyOrb] Power boost applied: +{powerBoost:F1} speed (PWR: {paddleStats.Power.GetValue()})");
            }

            // Apply velocity
            rb.linearVelocity = newDirection.normalized * finalSpeed;

            // Track who last hit the ball
            lastHitBy = collision.gameObject.tag; // Will be "Player" or "Enemy"

            if (showDebugLogs)
            {
                Debug.Log($"[EnergyOrb] Hit {collision.gameObject.name} at relative Y: {relativeIntersectY:F2}, " +
                          $"Bounce angle: {bounceAngle * Mathf.Rad2Deg:F1}°");
                Debug.Log($"[EnergyOrb] lastHitBy set to: '{lastHitBy}'"); // <-- ADD THIS LINE
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
        /// <summary>
        /// Detect when ball enters a goal zone
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[EnergyOrb] TRIGGER DETECTED with: {collision.gameObject.name} (Tag: {collision.tag})");
            Debug.Log($"[EnergyOrb] Last hit by: '{lastHitBy}'");

            // Check if we hit a goal zone
            if (collision.CompareTag("PlayerGoal"))
            {
                Debug.Log("[EnergyOrb] Hit PLAYER GOAL ZONE");

                // Ball went past player - Enemy scores if enemy hit it last
                if (lastHitBy == "Enemy")
                {
                    Debug.Log("[EnergyOrb] Enemy scored! Dealing damage to player...");
                    DealDamageToPlayer();
                }
                else
                {
                    Debug.Log($"[EnergyOrb] No damage dealt. Last hit by: '{lastHitBy}' (expected 'Enemy')");
                }
                ResetAfterGoal();
            }
            else if (collision.CompareTag("EnemyGoal"))
            {
                Debug.Log("[EnergyOrb] Hit ENEMY GOAL ZONE");

                // Ball went past enemy - Player scores if player hit it last
                if (lastHitBy == "Player")
                {
                    Debug.Log("[EnergyOrb] Player scored! Dealing damage to enemy...");
                    DealDamageToEnemy();
                }
                else
                {
                    Debug.Log($"[EnergyOrb] No damage dealt. Last hit by: '{lastHitBy}' (expected 'Player')");
                }
                ResetAfterGoal();
            }
            else
            {
                Debug.Log($"[EnergyOrb] Trigger was not a goal zone. Tag: {collision.tag}");
            }
        }

        /// <summary>
        /// Deal damage to the player
        /// </summary>
        private void DealDamageToPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Health playerHealth = playerObj.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // Calculate damage (base damage + power modifier from enemy)
                    int finalDamage = CalculateDamage("Enemy");

                    playerHealth.TakeDamage(finalDamage);

                    if (showDebugLogs)
                        Debug.Log($"[EnergyOrb] Player took {finalDamage} damage!");
                }
            }
        }

        /// <summary>
        /// Deal damage to the enemy
        /// </summary>
        private void DealDamageToEnemy()
        {
            GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyObj != null)
            {
                Health enemyHealth = enemyObj.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    // Calculate damage (base damage + power modifier from player)
                    int finalDamage = CalculateDamage("Player");

                    enemyHealth.TakeDamage(finalDamage);

                    if (showDebugLogs)
                        Debug.Log($"[EnergyOrb] Enemy took {finalDamage} damage!");
                }
            }
        }
        /// <summary>
        /// Calculate damage based on attacker's Power stat
        /// </summary>
        private int CalculateDamage(string attackerTag)
        {
            int damage = baseDamage;

            GameObject attacker = GameObject.FindGameObjectWithTag(attackerTag);
            if (attacker != null)
            {
                CharacterStats attackerStats = attacker.GetComponent<CharacterStats>();
                if (attackerStats != null)
                {
                    // Power increases damage
                    float powerBonus = attackerStats.Power.GetValue() * powerDamageMultiplier;
                    damage += Mathf.RoundToInt(powerBonus);
                }
            }

            return Mathf.Max(1, damage); // Minimum 1 damage
        }

        /// <summary>
        /// Reset ball after a goal
        /// </summary>
        private void ResetAfterGoal()
        {
            ResetToCenter();
            lastHitBy = "";

            // Disable ball briefly to prevent immediate re-trigger
            circleCollider.enabled = false;

            // Relaunch after short delay
            Invoke(nameof(ReenableBallAndLaunch), 1.5f);
        }
        /// <summary>
        /// Re-enable the ball and launch it
        /// </summary>
        private void ReenableBallAndLaunch()
        {
            circleCollider.enabled = true;
            Launch();
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