using UnityEngine;

public class SignalPulseParticle : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    public float speed = 3f;

    private float progress = 0f;
    private SpriteRenderer sr;

    public void Init(Vector3 start, Vector3 end, Sprite sprite)
    {
        this.startPos = start;
        this.endPos = end;

        sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        sr.sprite = sprite;
        sr.color = new Color(0.3f, 1f, 0.3f, 0.95f); // Bright green glow
        sr.sortingOrder = 7; // Render in front of lines and tower structures

        transform.position = start;
        transform.localScale = new Vector3(0.35f, 0.35f, 1f);
    }

    void Update()
    {
        float distance = Vector3.Distance(startPos, endPos);
        if (distance <= 0.1f) return;

        progress += (speed / distance) * Time.deltaTime;
        if (progress > 1f)
        {
            progress = 0f; // Reset loop
        }

        transform.position = Vector3.Lerp(startPos, endPos, progress);
    }
}
