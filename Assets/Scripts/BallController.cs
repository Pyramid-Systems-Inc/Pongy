using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Ensure Rigidbody2D is attached
public class BallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager; // Assign GameManager object
    // No need for SoundManager reference if using Singleton

    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 8f;     // Speed when served
    [SerializeField] private float speedIncreasePerHit = 0.5f; // How much speed increases per paddle hit
    [SerializeField] private float maxSpeed = 15f;        // Maximum speed of the ball

    [Header("Bounce Settings")]
    [Range(0f, 90f)]
    [SerializeField] private float maxBounceAngle = 60f; // Max angle change from paddle edge hit (degrees)

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("BallController requires a Rigidbody2D component.", this);
            enabled = false;
            return;
        }
        if (gameManager == null)
        {
            Debug.LogError("Ball needs a reference to the GameManager.", this);
            enabled = false;
            return;
        }

        startPosition = transform.position; // Store initial position for reset
        // Ensure Rigidbody settings are correct
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
        // Stop ball initially
        StopBall();
    }

    // Launches the ball from the center
    public void LaunchBall(bool towardsPlayer) // True = launch left, False = launch right
    {
        ResetBall(); // Ensure it's at the start position
        currentSpeed = initialSpeed;

        // Calculate launch direction
        float angle = Random.Range(-30f, 30f); // Add some random vertical angle
        float directionX = towardsPlayer ? -1f : 1f;
        Vector2 launchDirection = new Vector2(directionX, Mathf.Tan(angle * Mathf.Deg2Rad)).normalized;

        rb.linearVelocity = launchDirection * currentSpeed;
        Debug.Log($"Ball Launched towards {(towardsPlayer ? "Player" : "AI")} at speed {currentSpeed}");
    }

    // Resets ball to start position and stops it
    public void ResetBall()
    {
        transform.position = startPosition;
        StopBall();
    }

    // Stops the ball's movement
    public void StopBall()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // Handle collisions using FixedUpdate for physics accuracy
    void FixedUpdate()
    {
        // Optional: Clamp speed if it somehow exceeds maxSpeed due to physics quirks
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }


    // Detect collisions with paddles, walls, and goals
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision);
            // --- Play Paddle Sound ---
            SoundManager.Instance?.PlayPaddleBounceSound(); // Use ?. for null safety
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            // --- Play Wall Sound ---
            SoundManager.Instance?.PlayWallBounceSound(); // Use ?. for null safety
            Debug.Log("Ball hit Wall");
        }
        // Goals are handled by OnTriggerEnter2D
    }

    // Detect entering goal triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            Debug.Log("Ball entered Goal: " + other.gameObject.name);
            // Determine which goal was hit
            // Assuming player is left (scores in RightGoal), AI is right (scores in LeftGoal)
            bool playerScored = other.gameObject.name == "RightGoal";
            gameManager.ScorePoint(playerScored);
            // GameManager will handle resetting and serving the ball, and playing score sound
        }
    }


    // Handles the logic when the ball collides with a paddle
    private void HandlePaddleCollision(Collision2D collision)
    {
        // --- 1. Calculate Bounce Angle based on Hit Location ---
        Vector2 paddlePosition = collision.transform.position;
        float paddleHeight = collision.collider.bounds.size.y; // Use collider bounds size
        Vector2 contactPoint = collision.GetContact(0).point; // Get the first contact point

        // Calculate vertical offset from paddle center (-1 to 1)
        float offsetY = (contactPoint.y - paddlePosition.y) / (paddleHeight / 2f);
        offsetY = Mathf.Clamp(offsetY, -1f, 1f); // Ensure it's within bounds

        // Map offset to angle (linear mapping)
        float bounceAngleRad = offsetY * maxBounceAngle * Mathf.Deg2Rad;

        // Determine base direction based on which paddle was hit
        // If ball hit something on the left (Player Paddle), base direction is right (1, 0)
        // If ball hit something on the right (AI Paddle), base direction is left (-1, 0)
        bool hitPlayerPaddle = collision.transform.position.x < 0; // Simple check based on position
        float baseDirectionX = hitPlayerPaddle ? 1f : -1f;

        // Calculate the new direction vector by rotating the base direction
        // Rotation formula: x' = x*cos(a) - y*sin(a), y' = x*sin(a) + y*cos(a)
        // Base vector is (baseDirectionX, 0)
        float newDirX = baseDirectionX * Mathf.Cos(bounceAngleRad); // - 0 * Sin = baseDirectionX * Cos
        float newDirY = baseDirectionX * Mathf.Sin(bounceAngleRad); // + 0 * Cos = baseDirectionX * Sin

        Vector2 newDirection = new Vector2(newDirX, newDirY).normalized;


        // --- 2. Increase Speed ---
        currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maxSpeed);


        // --- 3. Apply New Velocity ---
        rb.linearVelocity = newDirection * currentSpeed;

        Debug.Log($"Ball hit Paddle ({(hitPlayerPaddle ? "Player" : "AI")}). Offset: {offsetY:F2}, Angle: {bounceAngleRad * Mathf.Rad2Deg:F1}, New Speed: {currentSpeed:F1}, New Vel: {rb.linearVelocity}");

        // Sound is played in OnCollisionEnter2D
    }

    // Public getter for velocity if needed by AI
    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }
}
