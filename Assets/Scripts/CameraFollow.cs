using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Map Boundaries")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-15f, -10f);
    public Vector2 maxBounds = new Vector2(15f, 10f);

    void Start()
    {
        if (target == null)
        {
            var player = FindObjectOfType<DroneController>();
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
        }

        // Maintain the camera Z position
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }
}
