using UnityEngine;
using UnityEngine.UI;

public class CableTile : MonoBehaviour
{
    [Header("Tile Settings")]
    public CableType type;
    public int x;
    public int y;
    public int rotationIndex = 0; // 0 = 0 deg, 1 = 90 deg CW, 2 = 180 deg, 3 = 270 deg

    [Header("State")]
    public bool isPowered = false;

    private bool[] baseConnections;
    private CablePuzzleManager manager;
    private Image image;

    public void Init(CableType type, int x, int y, int startRotation, CablePuzzleManager manager)
    {
        this.type = type;
        this.x = x;
        this.y = y;
        this.rotationIndex = startRotation;
        this.manager = manager;
        this.image = GetComponent<Image>();

        baseConnections = CableConnection.GetBaseConnections(type);
        UpdateVisuals();
    }

    public bool[] GetCurrentConnections()
    {
        bool[] current = new bool[4];
        if (baseConnections == null) return current;

        for (int i = 0; i < 4; i++)
        {
            int baseIndex = (i - rotationIndex) % 4;
            if (baseIndex < 0) baseIndex += 4;
            current[i] = baseConnections[baseIndex];
        }
        return current;
    }

    public void RotateTile()
    {
        rotationIndex = (rotationIndex + 1) % 4;
        UpdateVisuals();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioEvent.RotateCable);
        }

        if (manager != null)
        {
            manager.OnTileClicked();
        }
    }

    public void UpdateVisuals()
    {
        // 90 degree clockwise rotation visually in Unity UI requires a negative z rotation
        transform.localRotation = Quaternion.Euler(0, 0, -90f * rotationIndex);

        if (image != null && manager != null)
        {
            image.color = isPowered ? manager.poweredColor : manager.unpoweredColor;
        }
    }
}
