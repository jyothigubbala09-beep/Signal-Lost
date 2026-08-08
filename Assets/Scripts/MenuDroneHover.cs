using UnityEngine;

public class MenuDroneHover : MonoBehaviour
{
    public float hoverSpeed = 1.8f;
    public float hoverAmount = 0.35f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Smooth sine wave float vertically
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmount, 0f);
    }
}
