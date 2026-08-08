using UnityEngine;

public class RainController : MonoBehaviour
{
    public Sprite dropSprite;
    public int dropCount = 50;
    public float fallSpeed = 9f;
    public float windDrift = -1.8f;

    private Transform[] drops;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        drops = new Transform[dropCount];

        for (int i = 0; i < dropCount; i++)
        {
            GameObject drop = new GameObject("RainDrop_" + i);
            drop.transform.SetParent(transform);

            SpriteRenderer sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = dropSprite;
            sr.color = new Color(0.45f, 0.6f, 0.8f, 0.22f); // Rain blue translucent
            sr.sortingOrder = 8; // In front of standard assets

            // Spread out drops inside camera bounds initially
            float camHeight = (cam != null) ? cam.orthographicSize * 2f : 12f;
            float camWidth = camHeight * ((cam != null) ? cam.aspect : 1.7f);
            float rx = Random.Range(-camWidth / 2f, camWidth / 2f);
            float ry = Random.Range(-camHeight / 2f, camHeight / 2f);

            drop.transform.position = GetCamCenter() + new Vector3(rx, ry, 0f);
            drop.transform.localScale = new Vector3(0.03f, 0.35f, 1f);
            drop.transform.rotation = Quaternion.Euler(0, 0, 12f); // Wind slant angle

            drops[i] = drop.transform;
        }
    }

    Vector3 GetCamCenter()
    {
        if (cam != null) return cam.transform.position;
        return Vector3.zero;
    }

    void Update()
    {
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        Vector3 camCenter = cam.transform.position;

        for (int i = 0; i < dropCount; i++)
        {
            Transform t = drops[i];

            // Apply translation movement
            t.position += new Vector3(windDrift, -fallSpeed, 0f) * Time.deltaTime;

            // Recycle vertical limits
            if (t.position.y < camCenter.y - (camHeight / 2f) - 0.5f)
            {
                float rx = Random.Range(-camWidth / 2f - 2f, camWidth / 2f + 2f);
                t.position = new Vector3(camCenter.x + rx, camCenter.y + (camHeight / 2f) + 0.5f, 0f);
            }

            // Recycle horizontal bounds due to wind slant drift
            if (t.position.x < camCenter.x - (camWidth / 2f) - 2f)
            {
                t.position = new Vector3(camCenter.x + (camWidth / 2f) + 1f, t.position.y, 0f);
            }
        }
    }
}
