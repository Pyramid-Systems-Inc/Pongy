using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System namespace

public class PaddleController : MonoBehaviour
{
    // --- Control Scheme Enum & Settings ---
    public enum ControlScheme
    {
        Keyboard,
        Mouse,
        TouchDrag,
        Tilt
        // Add Swipe later if needed
    }

    [Header("Control Settings")]
    [SerializeField] private ControlScheme currentScheme = ControlScheme.Keyboard; // Default scheme
    [SerializeField] private string playerPrefsKey = "PlayerControlScheme"; // Key to save/load preference

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f; // Speed for Keyboard/Swipe/Tilt sensitivity
    [SerializeField] private float tiltSensitivity = 5f; // Adjust how much tilt affects movement

    [Header("Boundaries")]
    [SerializeField] private float boundaryPadding = 0.5f; // How far from edge paddle stops

    [Header("Visual/Haptic Feedback")]
    [SerializeField] private GameObject touchMarkerPrefab; // Optional: Prefab to show touch location
    [SerializeField] private Color activeInputColor = Color.yellow; // Color when input is active
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private GameObject currentTouchMarker;

    // --- Private Variables ---
    private Rigidbody2D rb;
    private PongControls controls; // Reference to the generated Input Actions class
    private Camera mainCamera;
    private float screenHeightInUnits;
    private Vector2 startPosition;
    private float paddleHeightHalf;
    private bool inputActive = false; // For visual feedback

    // --- Input System Setup ---
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; // Cache the main camera
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) originalColor = spriteRenderer.color;

        // Initialize Input Actions C# class
        controls = new PongControls();

        // Load saved control scheme preference
        LoadControlScheme();

        // --- Subscribe to Input Actions ---
        // Relative movement (Keyboard, Gamepad Stick)
        controls.Player.MoveRelative.performed += ctx => HandleRelativeMove(ctx.ReadValue<Vector2>().y);
        controls.Player.MoveRelative.canceled += ctx => HandleRelativeMove(0f); // Stop movement when key released

        // Absolute position (Mouse, Touch Drag) - processed in Update/FixedUpdate

        // Tilt (Accelerometer) - processed in Update/FixedUpdate

        // Touch Press (for feedback)
        controls.Player.TouchPress.performed += ctx => OnTouchStart(ctx);
        controls.Player.TouchPress.canceled += ctx => OnTouchEnd(ctx);

    }

    void OnEnable()
    {
        controls.Player.Enable(); // Enable the Player action map
        // Re-subscribe if needed, though Awake should handle initial setup
    }

    void OnDisable()
    {
        controls.Player.Disable(); // Disable the Player action map
        // Unsubscribe is good practice, but handled by disabling the map here
    }

    void Start()
    {
        // Calculate screen height and paddle height after Awake
        screenHeightInUnits = mainCamera.orthographicSize;
        startPosition = transform.position;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null) paddleHeightHalf = collider.size.y * transform.localScale.y / 2f;
        else paddleHeightHalf = transform.localScale.y / 2f;

        // Ensure Rigidbody setup (as before)
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }

        // Apply the loaded scheme initially
        ConfigureInputForScheme(currentScheme);
    }


    // --- Input Handling Logic ---

    private float relativeMoveInput = 0f; // Store input from MoveRelative action

    private void HandleRelativeMove(float yInput)
    {
        if (currentScheme == ControlScheme.Keyboard)
        {
            relativeMoveInput = yInput;
            inputActive = (yInput != 0); // Update visual feedback flag
        }
    }

    private void OnTouchStart(InputAction.CallbackContext context)
    {
         if (currentScheme == ControlScheme.TouchDrag) // Only react if in Touch mode
         {
            inputActive = true;
            TryVibrate(); // Haptic feedback

            // Visual feedback: Show marker at touch position
            if (touchMarkerPrefab != null)
            {
                Vector2 touchPosScreen = controls.Player.MoveAbsolute.ReadValue<Vector2>();
                Vector3 touchPosWorld = mainCamera.ScreenToWorldPoint(new Vector3(touchPosScreen.x, touchPosScreen.y, mainCamera.nearClipPlane + 1)); // Z offset needed
                touchPosWorld.z = 0; // Ensure Z is 0 for 2D
                 if (currentTouchMarker == null)
                     currentTouchMarker = Instantiate(touchMarkerPrefab, touchPosWorld, Quaternion.identity);
                 else
                     currentTouchMarker.transform.position = touchPosWorld;
                currentTouchMarker.SetActive(true);
            }
         }
    }

     private void OnTouchEnd(InputAction.CallbackContext context)
     {
         if (currentScheme == ControlScheme.TouchDrag)
         {
             inputActive = false;
             // Hide visual marker
             if (currentTouchMarker != null) currentTouchMarker.SetActive(false);
         }
     }


    // --- Movement Processing (FixedUpdate for Physics) ---
    void FixedUpdate()
    {
        float targetY = rb.position.y; // Start with current position

        // --- Read and Process Input based on Scheme ---
        switch (currentScheme)
        {
            case ControlScheme.Keyboard:
                // Use the stored relativeMoveInput from HandleRelativeMove
                MovePaddleKinematic(rb.position + new Vector2(0, relativeMoveInput * moveSpeed * Time.fixedDeltaTime));
                // Or set velocity for Dynamic:
                // MovePaddleDynamic(new Vector2(0, relativeMoveInput * moveSpeed));
                break;

            case ControlScheme.Mouse:
            case ControlScheme.TouchDrag:
                // Read absolute position (Mouse or Touch)
                Vector2 screenPos = controls.Player.MoveAbsolute.ReadValue<Vector2>();
                // Convert screen position to world position
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1)); // Z offset needed
                targetY = worldPos.y;
                inputActive = controls.Player.TouchPress.IsPressed(); // Check if mouse button or touch is down

                 // Update touch marker position if active
                 if (inputActive && currentScheme == ControlScheme.TouchDrag && currentTouchMarker != null) {
                     currentTouchMarker.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
                 }

                // Move paddle directly towards the target Y (using MovePosition for smoothness)
                MovePaddleKinematic(new Vector2(rb.position.x, targetY));
                // For Dynamic, could calculate required velocity, but MovePosition is often better for direct tracking
                break;

            case ControlScheme.Tilt:
                // Read accelerometer data
                Vector3 tilt = controls.Player.Tilt.ReadValue<Vector3>();
                // Map tilt (e.g., Y axis of accelerometer in landscape) to paddle movement
                // IMPORTANT: Axis mapping depends on device orientation and how it's held!
                // Assuming Landscape Left, Accelerometer Y often corresponds to vertical tilt
                float tiltInput = tilt.y; // Adjust this based on testing! Might need tilt.x or -tilt.y etc.
                targetY = rb.position.y + tiltInput * moveSpeed * tiltSensitivity * Time.fixedDeltaTime;
                inputActive = Mathf.Abs(tiltInput) > 0.1f; // Example threshold for activity

                MovePaddleKinematic(new Vector2(rb.position.x, targetY));
                // Or set velocity for Dynamic:
                // MovePaddleDynamic(new Vector2(0, tiltInput * moveSpeed * tiltSensitivity));
                break;
        }

        // --- Apply Visual Feedback ---
        UpdateVisualFeedback();
    }

    // --- Movement Application ---
    private void MovePaddleKinematic(Vector2 targetPosition)
    {
        // Clamp target position within boundaries
        float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
        float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Use MovePosition for kinematic bodies
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            // If somehow called for a dynamic body, maybe lerp towards target?
            // Or just set position directly (can cause physics issues)
             rb.position = Vector2.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * moveSpeed * 2f); // Example lerp
        }
    }

    private void MovePaddleDynamic(Vector2 targetVelocity)
    {
         if (rb.bodyType == RigidbodyType2D.Dynamic)
         {
            rb.linearVelocity = targetVelocity;
            // Dynamic bodies also need position clamping (done in FixedUpdate after velocity applied)
            float minY = -screenHeightInUnits + paddleHeightHalf + boundaryPadding;
            float maxY = screenHeightInUnits - paddleHeightHalf - boundaryPadding;
            Vector2 clampedPosition = rb.position;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            if (!Mathf.Approximately(clampedPosition.y, rb.position.y))
            {
                 rb.position = clampedPosition;
                 rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
         }
    }


    // --- Settings Management ---
    public void SetControlScheme(ControlScheme newScheme)
    {
        currentScheme = newScheme;
        PlayerPrefs.SetInt(playerPrefsKey, (int)newScheme);
        PlayerPrefs.Save();
        Debug.Log($"Control Scheme set to: {newScheme}");
        ConfigureInputForScheme(newScheme); // Reconfigure bindings if needed
    }

     public void SetControlSchemeFromString(string schemeName) {
         if (System.Enum.TryParse<ControlScheme>(schemeName, true, out ControlScheme newScheme)) {
             SetControlScheme(newScheme);
         } else {
             Debug.LogWarning($"Invalid control scheme name: {schemeName}");
         }
     }

    private void LoadControlScheme()
    {
        // Load from PlayerPrefs, default to Keyboard if not found or invalid
        currentScheme = (ControlScheme)PlayerPrefs.GetInt(playerPrefsKey, (int)GetDefaultSchemeForPlatform());
        Debug.Log($"Loaded Control Scheme: {currentScheme}");
    }

     private ControlScheme GetDefaultSchemeForPlatform() {
         #if UNITY_ANDROID || UNITY_IOS
             return ControlScheme.TouchDrag;
         #else // Standalone (Windows, Mac, Linux)
             return ControlScheme.Keyboard;
         #endif
     }

    // Optional: Disable specific actions if not needed by current scheme
    private void ConfigureInputForScheme(ControlScheme scheme) {
       // For this setup, we read all inputs but only *act* on the relevant one
       // in FixedUpdate based on the switch statement.
       // More complex scenarios might involve enabling/disabling action maps or bindings.
       relativeMoveInput = 0f; // Reset relative input when scheme changes
       inputActive = false;
       UpdateVisualFeedback(); // Reset visual feedback
       if (currentTouchMarker != null) currentTouchMarker.SetActive(false); // Hide touch marker
    }


    // --- Feedback Methods ---
    private void TryVibrate()
    {
        #if UNITY_ANDROID || UNITY_IOS
        // Check if device supports vibration before calling
        // if (SystemInfo.supportsVibration) // Requires Unity 2021.2+
        // {
             Handheld.Vibrate();
             Debug.Log("Vibrated!");
        // }
        #endif
    }

    private void UpdateVisualFeedback()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = inputActive ? activeInputColor : originalColor;
        }
    }

    // --- Reset Position ---
    public void ResetPosition()
    {
        // Simplified reset logic (as before)
        if (rb != null)
        {
            if (rb.bodyType == RigidbodyType2D.Kinematic) rb.position = startPosition;
            else transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;
        } else {
            transform.position = startPosition;
        }
         inputActive = false; // Reset feedback flag
         UpdateVisualFeedback(); // Update visuals on reset
         if (currentTouchMarker != null) currentTouchMarker.SetActive(false); // Hide marker
    }
}
