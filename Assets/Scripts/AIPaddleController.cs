using UnityEngine;

public class AIPaddleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ballTransform; // Assign the Ball's Transform

    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 8f; // AI paddle speed
    [Range(0f, 1f)] // Difficulty: 0 = Easy/Slow Reaction, 1 = Tracks ball perfectly (adjust speed too)
    [SerializeField] private float difficultyFactor = 0.5f; // Controls reaction speed / interpolation factor
    [SerializeField] private float predictionAmount = 0.1f; // How much to 'lead' the ball based on its velocity (higher difficulty)
    [SerializeField] private float deadZone = 0.1f; // Prevent jittering when ball is vertically aligned

    [Header("Boundaries")]
    [SerializeField] private float boundaryPadding = 0.5f; // How far from edge paddle stops

    // --- Private Variables ---
    private Rigidbody2D rb;
    private BallController ballController; // Reference to the ball's script to get velocity
    private float screenHeightInUnits;
    private Vector2 startPosition;
    private float targetY; // Where the AI intends to move
    private float paddleHeightHalf; // Store half the paddle height

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("AIPaddleController requires a Rigidbody2D component.", this);
            enabled = false;
            return;
        }
        if (ballTransform == null)
        {
            Debug.LogError("AI Paddle needs a reference to the Ball Transform.", this);
            enabled = false;
            return;
        }
        // Try to get the BallController script from the ball transform
        ballController = ballTransform.GetComponent<BallController>();
        if (ballController == null)
        {
            Debug.LogWarning("AI Paddle could not find BallController script on the ball. Prediction disabled.", this);
        }


        screenHeightInUnits = Camera.main.orthographicSize;
        startPosition = transform.position;

        // Get paddle height
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            paddleHeightHalf = collider.size.y * transform.localScale.y / 2f;
        }
        else
        {
            paddleHeightHalf = transform.localScale.y / 2f;
            Debug.LogWarning("AIPaddleController using transform scale for height. Add BoxCollider2D for accuracy.", this);
        }


        // Ensure Rigidbody is set correctly (should match player paddle setup)
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        // Kinematic setup is also valid
        targetY = startPosition.y; // Initialize target Y
    }


    void FixedUpdate() // Use FixedUpdate for physics and consistent AI movement
    {
        // --- AI Logic: Determine Target Y ---
        float currentBallY = ballTransform.position.y;
        float idealTargetY = currentBallY; // Start with current ball position

        // Add prediction if BallController exists and ball is moving towards AI
        // Assuming AI is the right paddle (positive X direction)
        if (ballController != null && ballController.GetVelocity().x > 0)
        {
            // Calculate a simple prediction based on ball's vertical velocity
            // Higher predictionAmount makes AI lead the ball more
            // Scale prediction by difficulty factor
            float prediction = ballController.GetVelocity().y * predictionAmount * difficultyFactor;
            idealTargetY += prediction;
        }

        // Smoothly interpolate the AI's target Y towards the ideal target Y
        // Lerp factor based on difficultyFactor * Time.fixedDeltaTime for smooth, frame-rate independent reaction
        // A higher difficultyFactor means faster reaction (closer to idealTargetY each frame)
        targetY = Mathf.Lerp(rb.position.y, idealTargetY, difficultyFactor * moveSpeed * Time.fixedDeltaTime);


        // --- Movement Calculation ---
        float currentY = rb.position.y;
        float direction = 0f;

        // Move only if the target is sufficiently different from current position (outside dead zone)
        if (Mathf.Abs(targetY - currentY) > deadZone)
        {
            // Determine direction to move: 1 for up, -1 for down
            direction = Mathf.Sign(targetY - currentY);
        }

        // --- Apply Movement ---
        Vector2 movement = new Vector2(0, direction * moveSpeed * Time.fixedDeltaTime);

        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Vector2 targetPos = rb.position + movement;

            // --- Boundary Clamping (for Kinematic) ---
            float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
            float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            rb.MovePosition(targetPos);
        }
        else // Dynamic Rigidbody
        {
            // Set velocity directly for dynamic body
            rb.linearVelocity = new Vector2(0, direction * moveSpeed);

            // --- Boundary Clamping (for Dynamic - Position based) ---
            float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
            float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;

            Vector2 clampedPosition = rb.position;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            if (!Mathf.Approximately(clampedPosition.y, rb.position.y))
            {
                rb.position = clampedPosition;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Stop vertical movement if clamped
            }
        }
    }

    // Method to reset the paddle's position
    public void ResetPosition()
    {
        if (rb != null)
        {
            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.position = startPosition;
            }
            else
            {
                transform.position = startPosition;
            }
            rb.linearVelocity = Vector2.zero;
            targetY = startPosition.y; // Reset target as well
        }
        else
        {
            transform.position = startPosition;
            targetY = startPosition.y;
        }
    }
}
