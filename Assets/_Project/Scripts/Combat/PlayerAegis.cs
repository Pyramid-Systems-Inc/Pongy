using UnityEngine;
using UnityEngine.InputSystem;
using PongQuest.RPG;

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
        [SerializeField] private float baseMoveSpeed = 8f;
        [SerializeField] private bool constrainToVertical = true;

        [Header("Stat Scaling")]
        [SerializeField] private float agilityMultiplier = 0.5f;

        [Header("Boundary Constraints")]
        [SerializeField] private float minY = -3.5f;
        [SerializeField] private float maxY = 3.5f;

        // Components
        private Rigidbody2D rb;
        private Controls controls;
        private CharacterStats stats;

        // Input
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<CharacterStats>();

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
            // Calculate actual move speed based on stats
            float actualMoveSpeed = baseMoveSpeed;

            if (stats != null)
            {
                // AGI increases movement speed
                // Formula: baseMoveSpeed + (Agility * multiplier)
                actualMoveSpeed = baseMoveSpeed + (stats.Agility.GetValue() * agilityMultiplier);
            }

            Vector2 velocity;

            if (constrainToVertical)
            {
                // Only allow vertical movement (classic Pong)
                velocity = new Vector2(0f, moveInput.y * actualMoveSpeed);
            }
            else
            {
                // Allow full 2D movement (for future arena hazards)
                velocity = moveInput * actualMoveSpeed;
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