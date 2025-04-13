using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 10f; // Speed of the paddle
    [SerializeField] private KeyCode moveUpKey = KeyCode.W;    // Key to move up
    [SerializeField] private KeyCode moveDownKey = KeyCode.S;  // Key to move down

    [Header("Boundaries")]
    [SerializeField] private float boundaryPadding = 0.5f; // How far from edge paddle stops

    // --- Private Variables ---
    private Rigidbody2D rb;
    private float screenHeightInUnits;
    private Vector2 startPosition; // To store the initial position for reset
    private float paddleHeightHalf; // Store half the paddle height for boundary checks

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PaddleController requires a Rigidbody2D component.", this);
            enabled = false; // Disable script if no Rigidbody2D
            return;
        }

        // Calculate screen height in world units (using orthographicSize)
        // orthographicSize is half the vertical size of the camera view
        screenHeightInUnits = Camera.main.orthographicSize;
        startPosition = transform.position; // Store starting position

        // Get paddle height (assuming BoxCollider2D represents the paddle size)
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            paddleHeightHalf = collider.size.y * transform.localScale.y / 2f;
        }
        else
        {
            // Fallback if no collider, use transform scale (less accurate)
            paddleHeightHalf = transform.localScale.y / 2f;
            Debug.LogWarning("PaddleController using transform scale for height. Add BoxCollider2D for accuracy.", this);
        }


        // Ensure Rigidbody is set correctly if Dynamic
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        // If Kinematic, velocity won't work directly, movement needs to be done via MovePosition
    }

    // Use FixedUpdate for physics-based movement (especially Rigidbody manipulation)
    void FixedUpdate()
    {
        // --- Input Handling ---
        float moveInput = 0f;
        // Check input keys
        if (Input.GetKey(moveUpKey))
        {
            moveInput = 1f;
        }
        else if (Input.GetKey(moveDownKey))
        {
            moveInput = -1f;
        }

        // --- Calculate Target Position/Velocity ---
        Vector2 movement = new Vector2(0, moveInput * moveSpeed * Time.fixedDeltaTime);

        // --- Apply Movement ---
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // For Kinematic bodies, calculate the target position
            Vector2 targetPos = rb.position + movement;

            // --- Boundary Clamping (for Kinematic) ---
            float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
            float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            // Move the kinematic Rigidbody to the clamped target position
            rb.MovePosition(targetPos);
        }
        else // Dynamic Rigidbody
        {
            // For Dynamic bodies, set velocity directly
            rb.linearVelocity = new Vector2(0, moveInput * moveSpeed); // No Time.fixedDeltaTime needed for velocity

            // --- Boundary Clamping (for Dynamic - Position based) ---
            // Dynamic bodies also need position clamping to prevent phasing through boundaries if moving very fast
            float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
            float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;

            // Check if the current position is already out of bounds (e.g., due to physics)
            // Or if the next velocity step *would* take it out of bounds (more complex prediction)
            // Simple clamping:
            Vector2 clampedPosition = rb.position;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            // If clamping changes position, also stop vertical velocity to prevent sticking
            if (!Mathf.Approximately(clampedPosition.y, rb.position.y))
            {
                rb.position = clampedPosition;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Stop vertical movement if clamped
            }
        }
    }


    // Method to reset the paddle's position called by GameManager
    public void ResetPosition()
    {
        if (rb != null)
        {
            // Reset position correctly for both types
            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.position = startPosition; // Set position directly for Kinematic
            }
            else
            {
                transform.position = startPosition; // Set transform for Dynamic (physics will catch up)
            }
            rb.linearVelocity = Vector2.zero; // Stop any residual movement
        }
        else
        {
            transform.position = startPosition; // Fallback if no Rigidbody
        }
    }
}
