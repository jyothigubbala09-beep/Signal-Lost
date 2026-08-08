using UnityEngine;

public class NetworkLineAnimator : MonoBehaviour
{
    private LineRenderer lr;
    public float scrollSpeed = 3f;
    public float pulseSpeed = 6f;

    private Gradient gradient;
    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) return;

        lr.useWorldSpace = true;

        gradient = new Gradient();
        colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 0f); // Green
        colorKeys[1] = new GradientColorKey(new Color(1.0f, 0.85f, 0.1f), 0.5f); // Golden Yellow
        colorKeys[2] = new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 1f); // Green

        alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(0.7f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 0.5f);
        alphaKeys[2] = new GradientAlphaKey(0.7f, 1f);

        gradient.SetKeys(colorKeys, alphaKeys);
        lr.colorGradient = gradient;
    }

    void Update()
    {
        if (lr == null) return;

        // Shift gradient colors to simulate electricity/flow
        float wavePos = (Time.time * scrollSpeed) % 1f;

        colorKeys[0].time = (wavePos - 0.2f + 1f) % 1f;
        colorKeys[1].time = wavePos;
        colorKeys[2].time = (wavePos + 0.2f) % 1f;

        // Keep times in ascending order [0, 1] for Unity's Gradient constraints
        System.Array.Sort(colorKeys, (a, b) => a.time.CompareTo(b.time));

        gradient.SetKeys(colorKeys, alphaKeys);
        lr.colorGradient = gradient;

        // Add line width pulsation
        float widthPulse = 0.08f + Mathf.Sin(Time.time * pulseSpeed) * 0.018f;
        lr.startWidth = widthPulse;
        lr.endWidth = widthPulse;
    }
}
