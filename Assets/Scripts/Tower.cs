using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Identity")]
    public string towerId;
    public string towerName = "Comm Tower";
    public bool isActive = false;
    public bool isPowerSource = false;
    public float detectionRadius = 1.5f;

    [Header("Visual Indicators")]
    public SpriteRenderer statusLightRenderer;
    public Sprite brokenSprite;
    public Sprite activeSprite;
    public Color brokenColor = Color.red;
    public Color activeColor = Color.green;

    [Header("UI Prompts")]
    public GameObject interactionPrompt; // UI or Sprite prompt like "Press E"

    protected CablePuzzleManager puzzleManager;
    protected NetworkManager networkManager;

    public virtual void Start()
    {
        puzzleManager = FindObjectOfType<CablePuzzleManager>();
        networkManager = FindObjectOfType<NetworkManager>();

        if (networkManager != null)
        {
            networkManager.RegisterTower(this);
        }

        UpdateVisuals();
        if (interactionPrompt != null)
        {
            UnityEngine.UI.Text pText = interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>();
            if (pText != null)
            {
                pText.text = "REPAIR SIGNAL\n[E] Repair";
            }
            interactionPrompt.SetActive(false);
        }

        // Enable ripple immediately on startup if already active (e.g. Power Tower)
        if (isActive)
        {
            var ripple = transform.Find("RippleEffect");
            if (ripple != null)
            {
                ripple.gameObject.SetActive(true);
            }
        }
    }

    public void SetPromptActive(bool active)
    {
        if (interactionPrompt != null && !isActive)
        {
            interactionPrompt.SetActive(active);
        }
    }

    public virtual void Interact()
    {
        if (isActive) return;

        // Hide prompt while puzzle is active
        SetPromptActive(false);

        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle(this);
        }
        else
        {
            Debug.LogError("Puzzle Manager not found in the scene.");
        }
    }

    public virtual void ActivateTower()
    {
        if (isActive) return;

        isActive = true;
        UpdateVisuals();
        SetPromptActive(false);

        // 1. Enable radio wave ripple animation
        var ripple = transform.Find("RippleEffect");
        if (ripple != null)
        {
            ripple.gameObject.SetActive(true);
        }

        // 2. Play synthesizer success sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioEvent.TowerActive);
        }
        else
        {
            PlaySuccessSound();
        }

        // 3. Draw connection line to nearest active tower in grid
        DrawNetworkConnection();

        if (networkManager != null)
        {
            networkManager.OnTowerActivated(this);
        }
    }

    protected virtual void UpdateVisuals()
    {
        if (statusLightRenderer != null)
        {
            statusLightRenderer.color = isActive ? activeColor : brokenColor;
            if (isActive && activeSprite != null)
            {
                statusLightRenderer.sprite = activeSprite;
            }
            else if (!isActive && brokenSprite != null)
            {
                statusLightRenderer.sprite = brokenSprite;
            }
        }
    }

    private void PlaySuccessSound()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        int sampleRate = 44100;
        float frequency1 = 587.33f; // D5 note
        float frequency2 = 880f;    // A5 note
        float duration = 0.15f;

        AudioClip clip = AudioClip.Create("SuccessBeep", sampleRate * (int)(duration * 2f), 1, sampleRate, false);
        float[] data = new float[sampleRate * (int)(duration * 2f)];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            if (time < duration)
            {
                // First beep (sine wave with fade out envelope)
                float envelope = 1f - (time / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency1 * time) * 0.4f * envelope;
            }
            else
            {
                // Second beep (sine wave with fade out envelope)
                float t2 = time - duration;
                float envelope = 1f - (t2 / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency2 * t2) * 0.4f * envelope;
            }
        }

        clip.SetData(data, 0);
        source.clip = clip;
        source.volume = 0.5f;
        source.Play();

        // Destroy temporary audio source component after playing completes
        Destroy(source, duration * 2.5f + 0.5f);
    }

    private void DrawNetworkConnection()
    {
        // Find nearest active tower in the network
        Tower[] allTowers = FindObjectsOfType<Tower>();
        Tower nearestActive = null;
        float minDistance = float.MaxValue;

        foreach (var tower in allTowers)
        {
            if (tower == null || tower == this || !tower.isActive) continue;

            float distance = Vector3.Distance(transform.position, tower.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestActive = tower;
            }
        }

        if (nearestActive != null)
        {
            LineRenderer lr = gameObject.GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = gameObject.AddComponent<LineRenderer>();
            }

            // Assign standard default sprite shader
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = activeColor;
            lr.endColor = activeColor;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;
            lr.positionCount = 2;

            // Position line slightly in Z-depth behind active assets
            Vector3 startPos = transform.position;
            startPos.z = 1f;
            Vector3 endPos = nearestActive.transform.position;
            endPos.z = 1f;

            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);

            // Add the pulsing color/gradient line animator
            NetworkLineAnimator animator = gameObject.GetComponent<NetworkLineAnimator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<NetworkLineAnimator>();
            }

            // Spawn the packet particle travelling from source (nearestActive) to destination (this)
            GameObject pulseObj = new GameObject("PulseParticle");
            pulseObj.transform.SetParent(transform);
            SignalPulseParticle particle = pulseObj.AddComponent<SignalPulseParticle>();
            Sprite glowSprite = (statusLightRenderer != null) ? statusLightRenderer.sprite : null;
            particle.Init(endPos, startPos, glowSprite); // Flow signal outwards from the power core network (endPos to startPos)

            Debug.Log($"Tower: Placed network connection path from {towerName} to {nearestActive.towerName}");
        }
    }
}
