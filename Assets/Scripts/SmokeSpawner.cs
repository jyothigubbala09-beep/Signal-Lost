using UnityEngine;

public class SmokeSpawner : MonoBehaviour
{
    public Sprite smokeSprite;
    public float spawnInterval = 0.5f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPuff();
        }
    }

    void SpawnPuff()
    {
        if (smokeSprite == null) return;

        GameObject puffObj = new GameObject("SmokePuff");
        puffObj.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.4f, 0f);

        SpriteRenderer sr = puffObj.AddComponent<SpriteRenderer>();
        sr.sprite = smokeSprite;
        sr.color = new Color(0.48f, 0.48f, 0.48f, 0.3f); // Translucent grey smoke
        sr.sortingOrder = 4; // Render above background roads but behind tower elements

        puffObj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // Attach lightweight animator component
        puffObj.AddComponent<SmokePuffMovement>();
    }
}

public class SmokePuffMovement : MonoBehaviour
{
    private float lifetime = 0f;
    public float maxLifetime = 1.6f;
    public float riseSpeed = 0.9f;
    public float windDrift = -0.25f;
    private float scaleSpeed = 0.55f;

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Rise and drift left with wind
        transform.position += new Vector3(windDrift, riseSpeed, 0f) * Time.deltaTime;

        float progress = lifetime / maxLifetime;
        float currentScale = 0.2f + progress * scaleSpeed;
        transform.localScale = new Vector3(currentScale, currentScale, 1f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Fade out opacity as it expands
            sr.color = new Color(0.48f, 0.48f, 0.48f, 0.3f * (1f - progress));
        }
    }
}
