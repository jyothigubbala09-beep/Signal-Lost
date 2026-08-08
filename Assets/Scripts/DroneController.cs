using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 15f;
    public float maxSpeed = 5.0f;
    public float drag = 3f;

    [Header("Boundaries")]
    public Vector2 minBounds = new Vector2(-14.5f, -9.5f);
    public Vector2 maxBounds = new Vector2(14.5f, 9.5f);

    [Header("Interaction")]
    public float interactionRange = 1.5f;

    [Header("Visuals & Light")]
    public Transform spotlightTransform;
    public float lightRotationSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;
    private Tower nearestTower;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.drag = drag;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        // Auto-assign spotlight child if not set
        if (spotlightTransform == null)
        {
            var lightChild = transform.Find("Spotlight");
            if (lightChild != null)
            {
                spotlightTransform = lightChild;
            }
        }
    }

    void Update()
    {
        // Get movement input (WASD and arrow-key)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // Flip sprite horizontally based on horizontal input
        if (spriteRenderer != null)
        {
            if (moveInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }

        // Find nearest tower
        FindNearestTower();

        // Handle interaction
        if (nearestTower != null && Input.GetKeyDown(KeyCode.E))
        {
            nearestTower.Interact();
        }

        // Smoothly rotate spotlight in direction of movement
        RotateSpotlight();

        // Modulate continuous drone engine hum in AudioManager
        if (AudioManager.Instance != null && rb != null)
        {
            float speedRatio = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
            AudioManager.Instance.SetDronePitchAndVolume(speedRatio);
        }
    }

    void FixedUpdate()
    {
        if (moveInput.sqrMagnitude > 0.01f && rb != null)
        {
            // Apply physics force for smooth hover/inertia movement
            rb.AddForce(moveInput * acceleration, ForceMode2D.Force);

            // Clamp max speed
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        // Hard clamp position to prevent drone from leaving the playable map
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minBounds.y, maxBounds.y);
        transform.position = clampedPos;
    }

    void RotateSpotlight()
    {
        if (spotlightTransform == null) return;

        Vector2 direction = moveInput;
        // Fallback to velocity if not active inputting, to slide/fade light direction
        if (direction.sqrMagnitude < 0.01f && rb != null)
        {
            direction = rb.velocity;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            // Calculate angle in degrees (the light cone sprite points downwards by default, 0 degrees points down)
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            spotlightTransform.rotation = Quaternion.Slerp(spotlightTransform.rotation, targetRotation, lightRotationSpeed * Time.deltaTime);
        }
    }

    void FindNearestTower()
    {
        Tower[] allTowers = FindObjectsOfType<Tower>();
        float minDistance = float.MaxValue;
        Tower closest = null;

        foreach (var tower in allTowers)
        {
            if (tower == null || tower.isActive) continue;

            float distance = Vector2.Distance(transform.position, tower.transform.position);
            if (distance < minDistance && distance <= tower.detectionRadius)
            {
                minDistance = distance;
                closest = tower;
            }
        }

        if (closest != nearestTower)
        {
            if (nearestTower != null) nearestTower.SetPromptActive(false);
            nearestTower = closest;
            if (nearestTower != null) nearestTower.SetPromptActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
