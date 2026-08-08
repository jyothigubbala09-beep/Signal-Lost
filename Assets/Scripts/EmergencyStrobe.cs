using UnityEngine;

public class EmergencyStrobe : MonoBehaviour
{
    private SpriteRenderer sr;
    private Tower parentTower;

    public float pulseSpeed = 5f;
    private float timer = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        parentTower = GetComponentInParent<Tower>();
    }

    void Update()
    {
        if (sr == null) return;

        timer += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.PingPong(timer, 1f);

        if (parentTower != null && parentTower.isActive)
        {
            // Active beacon: alternate green and glowing cyan
            Color green = parentTower.activeColor;
            Color cyan = new Color(0.2f, 0.7f, 1f, 0.4f);
            sr.color = Color.Lerp(green, cyan, pulse);
        }
        else
        {
            // Damaged alert strobe: blink bright red and dark red
            Color red = Color.red;
            Color darkRed = new Color(0.25f, 0f, 0f, 0.35f);
            sr.color = Color.Lerp(red, darkRed, pulse);
        }
    }
}
