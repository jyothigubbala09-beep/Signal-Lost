using UnityEngine;

public class SignalRipple : MonoBehaviour
{
    public Sprite rippleSprite;
    public float speed = 1.0f;
    public float maxScale = 3.0f;
    public Color rippleColor = new Color(0.2f, 0.8f, 0.2f, 0.45f);

    private SpriteRenderer sr1;
    private SpriteRenderer sr2;

    private float scale1 = 0f;
    private float scale2 = 1.5f; // Half-phase offset

    void Start()
    {
        // 1. Create Ripple wave 1 child object
        GameObject wave1Obj = new GameObject("Wave1");
        wave1Obj.transform.SetParent(transform, false);
        sr1 = wave1Obj.AddComponent<SpriteRenderer>();
        sr1.sprite = rippleSprite;
        sr1.color = rippleColor;
        sr1.sortingOrder = 3;

        // 2. Create Ripple wave 2 child object
        GameObject wave2Obj = new GameObject("Wave2");
        wave2Obj.transform.SetParent(transform, false);
        sr2 = wave2Obj.AddComponent<SpriteRenderer>();
        sr2.sprite = rippleSprite;
        sr2.color = rippleColor;
        sr2.sortingOrder = 3;

        // Turn off parent SpriteRenderer if any was automatically attached by initializer
        SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
        if (parentSR != null)
        {
            parentSR.enabled = false;
        }
    }

    void Update()
    {
        // Animate Wave 1
        scale1 += speed * Time.deltaTime;
        if (scale1 > maxScale) scale1 = 0f;
        sr1.transform.localScale = new Vector3(scale1, scale1, 1f);
        float alpha1 = (1f - (scale1 / maxScale)) * rippleColor.a;
        sr1.color = new Color(rippleColor.r, rippleColor.g, rippleColor.b, alpha1);

        // Animate Wave 2
        scale2 += speed * Time.deltaTime;
        if (scale2 > maxScale) scale2 = 0f;
        sr2.transform.localScale = new Vector3(scale2, scale2, 1f);
        float alpha2 = (1f - (scale2 / maxScale)) * rippleColor.a;
        sr2.color = new Color(rippleColor.r, rippleColor.g, rippleColor.b, alpha2);
    }
}
