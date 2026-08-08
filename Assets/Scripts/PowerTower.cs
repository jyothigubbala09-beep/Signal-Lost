using UnityEngine;

public class PowerTower : Tower
{
    public Color powerSourceColor = new Color(1.0f, 0.8f, 0.1f, 1.0f); // Glowing Gold

    public override void Start()
    {
        isActive = true;
        isPowerSource = true;

        // Base start handles auto-registration with NetworkManager
        base.Start();

        UpdateVisuals();
    }

    public override void Interact()
    {
        // Power source is already active, nothing to repair here!
    }

    public override void ActivateTower()
    {
        // Already active
    }

    protected override void UpdateVisuals()
    {
        if (statusLightRenderer != null)
        {
            statusLightRenderer.color = powerSourceColor;
            if (activeSprite != null)
            {
                statusLightRenderer.sprite = activeSprite;
            }
        }
    }
}
