using UnityEngine;
using System.Collections;

public class LightningFlash : MonoBehaviour
{
    private Camera cam;
    private Color defaultColor;
    private Color flashColor = new Color(0.6f, 0.7f, 0.85f, 1f); // Thunderstorm cyan-white

    private float flashTimer = 0f;
    private float nextFlashTime = 6f;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            defaultColor = cam.backgroundColor;
        }
        nextFlashTime = Random.Range(6f, 16f);
    }

    void Update()
    {
        if (cam == null) return;

        flashTimer += Time.deltaTime;
        if (flashTimer >= nextFlashTime)
        {
            flashTimer = 0f;
            nextFlashTime = Random.Range(8f, 22f);
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        cam.backgroundColor = flashColor;
        yield return new WaitForSeconds(0.06f); // Quick initial lightning strike peak

        float elapsed = 0f;
        float duration = 0.4f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.backgroundColor = Color.Lerp(flashColor, defaultColor, elapsed / duration);
            yield return null;
        }
        cam.backgroundColor = defaultColor;
    }
}
