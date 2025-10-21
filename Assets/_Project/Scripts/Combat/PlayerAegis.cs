using UnityEngine;
using UnityEngine.InputSystem;

namespace PongQuest.Combat
{
    /// <summary>
    /// Controls the player's paddle (Aegis) movement during battle.
    /// Uses the new Input System for movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerAegis : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private bool constrainToVertical = true;

        [Header("Boundary Constraints")]
        [SerializeField] private float minY = -3.5f;
        [SerializeField] private float maxY = 3.5f;

        // Components
        private Rigidbody2D rb;
        private Controls controls;

        // Input
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // Configure Rigidbody2D for tight control
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            // Initialize input
            controls = new Controls();
        }

        private void OnEnable()
        {
            controls.Enable();
            controls.Player.Move.performed += OnMove;
            controls.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            controls.Player.Move.performed -= OnMove;
            controls.Player.Move.canceled -= OnMove;
            controls.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 velocity;

            if (constrainToVertical)
            {
                // Only allow vertical movement (classic Pong)
                velocity = new Vector2(0f, moveInput.y * moveSpeed);
            }
            else
            {
                // Allow full 2D movement (for future arena hazards)
                velocity = moveInput * moveSpeed;
            }

            rb.linearVelocity = velocity;

            // Clamp position to stay within arena bounds
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            transform.position = clampedPosition;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize movement boundaries in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(-10, minY, 0), new Vector3(10, minY, 0));
            Gizmos.DrawLine(new Vector3(-10, maxY, 0), new Vector3(10, maxY, 0));
        }
    }
}