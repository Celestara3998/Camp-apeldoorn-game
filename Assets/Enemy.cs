using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float targetRefreshInterval = 1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpCooldown = 2.5f;
    [SerializeField] private float randomJumpIntervalMin = 4f;
    [SerializeField] private float randomJumpIntervalMax = 8f;
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Jump Dash")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.3f;

    [Header("Erratic Behaviour")]
    [SerializeField] private float erraticStrength = 0.45f;
    [SerializeField] private float directionChangeInterval = 0.8f;
    [SerializeField] private float targetOffsetRadius = 2f;

    [Header("Swarming")]
    [SerializeField] private float swarmRadius = 5f;
    [SerializeField] private float separationRadius = 1.4f;
    [SerializeField] private float separationStrength = 1.8f;
    [SerializeField] private float cohesionStrength = 0.35f;
    [SerializeField] private float alignmentStrength = 0.2f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;
    private Vector3 currentVelocity;
    private Vector3 erraticDirection;
    private Vector3 targetOffset;
    private Vector3 dashDirection;

    private float targetRefreshTimer;
    private float directionChangeTimer;
    private float jumpTimer;
    private float randomJumpTimer;
    private float dashTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        ChangeErraticDirection();
        ResetRandomJumpTimer();
    }

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        targetRefreshTimer -= Time.deltaTime;
        directionChangeTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
        randomJumpTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;

        if (targetRefreshTimer <= 0f || target == null)
        {
            FindTarget();
        }

        if (directionChangeTimer <= 0f)
        {
            ChangeErraticDirection();
        }

        MoveTowardsTarget();
    }

    private void FindTarget()
    {
        targetRefreshTimer = targetRefreshInterval;

        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(targetTag);

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null)
        {
            ApplyGravity();
            characterController.Move(verticalVelocity * Time.deltaTime);
            return;
        }

        Vector3 targetPosition = target.position + targetOffset;
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        Vector3 targetDirection =
            toTarget.sqrMagnitude > 0.01f
                ? toTarget.normalized
                : Vector3.zero;

        Vector3 swarmDirection = CalculateSwarmDirection();

        Vector3 desiredDirection = targetDirection
                                   + swarmDirection
                                   + erraticDirection * erraticStrength;

        desiredDirection.y = 0f;
        desiredDirection = desiredDirection.sqrMagnitude > 0.01f
            ? desiredDirection.normalized
            : Vector3.zero;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            desiredDirection * moveSpeed,
            acceleration * Time.deltaTime);

        TryJump(desiredDirection);
        ApplyGravity();

        Vector3 movement = currentVelocity;

        if (dashTimer > 0f)
        {
            movement += dashDirection * dashSpeed;
        }

        movement.y = verticalVelocity.y;

        characterController.Move(movement * Time.deltaTime);

        RotateTowardsMovement();
    }

    private Vector3 CalculateSwarmDirection()
    {
        Collider[] nearbyColliders =
            Physics.OverlapSphere(transform.position, swarmRadius);

        Vector3 separation = Vector3.zero;
        Vector3 averagePosition = Vector3.zero;
        Vector3 averageVelocity = Vector3.zero;

        int nearbyEnemyCount = 0;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            Enemy nearbyEnemy =
                nearbyCollider.GetComponentInParent<Enemy>();

            if (nearbyEnemy == null || nearbyEnemy == this)
            {
                continue;
            }

            Vector3 offset = transform.position - nearbyEnemy.transform.position;
            offset.y = 0f;

            float distance = offset.magnitude;

            if (distance > 0.01f && distance < separationRadius)
            {
                separation += offset.normalized / distance;
            }

            averagePosition += nearbyEnemy.transform.position;
            averageVelocity += nearbyEnemy.currentVelocity;
            nearbyEnemyCount++;
        }

        if (nearbyEnemyCount == 0)
        {
            return Vector3.zero;
        }

        averagePosition /= nearbyEnemyCount;
        averageVelocity /= nearbyEnemyCount;

        Vector3 cohesion = averagePosition - transform.position;
        cohesion.y = 0f;

        Vector3 alignment = averageVelocity;
        alignment.y = 0f;

        return separation * separationStrength
               + cohesion.normalized * cohesionStrength
               + alignment.normalized * alignmentStrength;
    }

    private void TryJump(Vector3 movementDirection)
    {
        if (!characterController.isGrounded || jumpTimer > 0f)
        {
            return;
        }

        bool obstacleAhead = Physics.SphereCast(
            transform.position + Vector3.up * 0.5f,
            characterController.radius * 0.8f,
            movementDirection,
            out _,
            obstacleCheckDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore);

        bool randomJump = randomJumpTimer <= 0f;

        if (!obstacleAhead && !randomJump)
        {
            return;
        }

        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumpTimer = jumpCooldown;

        ResetRandomJumpTimer();
        StartDash(movementDirection);
    }

    private void StartDash(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        dashDirection = direction.normalized;
        dashTimer = dashDuration;
    }

    private void ResetRandomJumpTimer()
    {
        randomJumpTimer = Random.Range(
            randomJumpIntervalMin,
            randomJumpIntervalMax);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void RotateTowardsMovement()
    {
        Vector3 flatVelocity = currentVelocity;

        if (dashTimer > 0f)
        {
            flatVelocity = dashDirection;
        }

        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(flatVelocity.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void ChangeErraticDirection()
    {
        directionChangeTimer =
            directionChangeInterval * Random.Range(0.6f, 1.4f);

        erraticDirection = Random.insideUnitSphere;
        erraticDirection.y = 0f;
        erraticDirection = erraticDirection.normalized;

        Vector2 randomOffset = Random.insideUnitCircle * targetOffsetRadius;

        targetOffset = new Vector3(
            randomOffset.x,
            0f,
            randomOffset.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, swarmRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
