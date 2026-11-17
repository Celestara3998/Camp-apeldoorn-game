using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float gravity = -20f;
            
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float staminaRegenerationRate = 1.5f;
    [SerializeField] private float staminaRegenerationDelay = 1f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.2f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;
    private float currentStamina;
    private float timeSinceSprint;
    private int lastLoggedStamina = -1;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        Move();
        RegenerateStamina();
        LogStamina();
    }

    private void Move()
    {
        bool isGrounded = IsGrounded();

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            movement += transform.forward;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            movement -= transform.forward;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            movement += transform.right;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            movement -= transform.right;
        }

        movement = Vector3.ClampMagnitude(movement, 1f);

        bool isMoving = movement.sqrMagnitude > 0.01f;
        bool isRunning = Keyboard.current.leftShiftKey.isPressed
                         && isMoving
                         && currentStamina > 0f;

        float speed = isRunning ? runSpeed : walkSpeed;

        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            timeSinceSprint = 0f;
        }
        else
        {
            timeSinceSprint += Time.deltaTime;
        }

        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        ApplyGravity();

        Vector3 velocity = movement * speed;
        velocity.y = verticalVelocity.y;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void RegenerateStamina()
    {
        if (timeSinceSprint < staminaRegenerationDelay)
        {
            return;
        }

        currentStamina += staminaRegenerationRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }

    private bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    private void LogStamina()
    {
        int displayedStamina = Mathf.FloorToInt(currentStamina);

        if (displayedStamina == lastLoggedStamina)
        {
            return;
        }

        lastLoggedStamina = displayedStamina;

        Debug.Log(
            $"Stamina: {currentStamina:F1}/{maxStamina}");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        DamageBrick damageBrick =
            hit.collider.GetComponentInParent<DamageBrick>();

        if (damageBrick == null)
        {
            return;
        }

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();

        damageBrick.TryDamagePlayer(playerHealth);
    }
}