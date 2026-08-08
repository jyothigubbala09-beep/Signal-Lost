using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips (Optional File Overrides)")]
    public AudioClip ambienceClip;
    public AudioClip droneClip;
    public AudioClip clickClip;
    public AudioClip rotateClip;
    public AudioClip connectClip;
    public AudioClip successClip;
    public AudioClip failureClip;
    public AudioClip victoryClip;
    public AudioClip radioClip;

    private AudioSource ambienceSource;
    private AudioSource droneSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayAmbience();
    }

    private void InitializeAudioSources()
    {
        // 1. Ambient channel
        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.loop = true;
        ambienceSource.volume = 0.35f;

        // 2. Continuous drone flight engine hum
        droneSource = gameObject.AddComponent<AudioSource>();
        droneSource.loop = true;
        droneSource.volume = 0f; // modulated at runtime

        // 3. One-shot SFX channel
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.6f;
    }

    public void PlayAmbience()
    {
        if (ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.Play();
        }
        else if (ambienceSource.clip == null)
        {
            // Synthesize low storm-wind/rumble ambience procedurally
            ambienceSource.clip = SynthesizeStormAmbience();
            ambienceSource.Play();
        }
    }

    public void SetDronePitchAndVolume(float speedRatio)
    {
        if (droneSource == null) return;

        if (droneClip != null && droneSource.clip != droneClip)
        {
            droneSource.clip = droneClip;
            droneSource.Play();
        }
        else if (droneSource.clip == null)
        {
            droneSource.clip = SynthesizeDroneEngineHum();
            droneSource.Play();
        }

        // Modulate volume and pitch based on velocity speed ratio [0, 1]
        droneSource.volume = Mathf.Lerp(0.08f, 0.28f, speedRatio);
        droneSource.pitch = Mathf.Lerp(0.9f, 1.25f, speedRatio);
    }

    public void PlaySFX(AudioEvent sfxType)
    {
        AudioClip clipToPlay = null;

        switch (sfxType)
        {
            case AudioEvent.RotateCable:
                clipToPlay = rotateClip != null ? rotateClip : SynthesizeRotateClick();
                break;
            case AudioEvent.ConnectCable:
                clipToPlay = connectClip != null ? connectClip : SynthesizeConnectPing();
                break;
            case AudioEvent.TowerActive:
            case AudioEvent.PuzzleSuccess:
                clipToPlay = successClip != null ? successClip : SynthesizeSuccessBeep();
                break;
            case AudioEvent.PuzzleFailure:
                clipToPlay = failureClip != null ? failureClip : SynthesizeFailureBeep();
                break;
            case AudioEvent.ButtonClick:
                clipToPlay = clickClip != null ? clickClip : SynthesizeButtonClickBeep();
                break;
            case AudioEvent.LevelComplete:
                clipToPlay = victoryClip != null ? victoryClip : SynthesizeVictoryChime();
                break;
            case AudioEvent.RadioTransmission:
                clipToPlay = radioClip != null ? radioClip : SynthesizeRadioSquelch();
                break;
        }

        if (clipToPlay != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
    }

    // --- PROCEDURAL AUDIO GENERATION ENGINES ---

    private AudioClip SynthesizeStormAmbience()
    {
        int sampleRate = 22050;
        int durationSamples = sampleRate * 4; // 4 second loop
        float[] data = new float[durationSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            float noise = Random.Range(-0.05f, 0.05f);
            float lowMod = Mathf.Sin(2f * Mathf.PI * 0.25f * time) * 0.04f;
            
            data[i] = (Mathf.Sin(2f * Mathf.PI * 55f * time) * 0.15f) + 
                      (Mathf.Sin(2f * Mathf.PI * 110f * time) * (0.05f + lowMod)) + 
                      noise;
        }

        AudioClip clip = AudioClip.Create("SynthAmbience", durationSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeDroneEngineHum()
    {
        int sampleRate = 22050;
        int durationSamples = sampleRate * 2; // 2 second loop
        float[] data = new float[durationSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            data[i] = (Mathf.Sin(2f * Mathf.PI * 180f * time) * 0.35f) + 
                      (Mathf.Sin(2f * Mathf.PI * 360f * time) * 0.12f);
        }

        AudioClip clip = AudioClip.Create("SynthDroneHum", durationSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeRotateClick()
    {
        int sampleRate = 44100;
        float duration = 0.06f;
        int numSamples = (int)(sampleRate * duration);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            float freq = Mathf.Lerp(1200f, 100f, time / duration);
            float envelope = 1f - (time / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * time) * 0.45f * envelope;
        }

        AudioClip clip = AudioClip.Create("SynthClick", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeConnectPing()
    {
        int sampleRate = 44100;
        float duration = 0.22f;
        int numSamples = (int)(sampleRate * duration);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            float envelope = 1f - (time / duration);
            data[i] = ((Mathf.Sin(2f * Mathf.PI * 440f * time) * 0.3f) + 
                       (Mathf.Sin(2f * Mathf.PI * 660f * time) * 0.15f)) * envelope;
        }

        AudioClip clip = AudioClip.Create("SynthPing", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeSuccessBeep()
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int numSamples = (int)(sampleRate * duration * 2f);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            if (time < duration)
            {
                float env = 1f - (time / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * 587.33f * time) * 0.4f * env; // D5
            }
            else
            {
                float t2 = time - duration;
                float env = 1f - (t2 / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t2) * 0.4f * env; // A5
            }
        }

        AudioClip clip = AudioClip.Create("SynthSuccess", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeFailureBeep()
    {
        int sampleRate = 44100;
        float duration = 0.16f;
        int numSamples = (int)(sampleRate * duration * 2f);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            if (time < duration)
            {
                float env = 1f - (time / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * 180f * time) * 0.45f * env;
            }
            else
            {
                float t2 = time - duration;
                float env = 1f - (t2 / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * 150f * t2) * 0.45f * env;
            }
        }

        AudioClip clip = AudioClip.Create("SynthFailure", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeButtonClickBeep()
    {
        int sampleRate = 44100;
        float duration = 0.08f;
        int numSamples = (int)(sampleRate * duration);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            float env = 1f - (time / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * 800f * time) * 0.35f * env;
        }

        AudioClip clip = AudioClip.Create("SynthBtn", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeVictoryChime()
    {
        int sampleRate = 44100;
        float noteDuration = 0.15f;
        int numSamples = (int)(sampleRate * noteDuration * 4f);
        float[] data = new float[numSamples];
        float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f }; // C4, E4, G4, C5 major arpeggio

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            int noteIndex = Mathf.FloorToInt(time / noteDuration);
            if (noteIndex >= 4) noteIndex = 3;

            float noteTime = time - (noteIndex * noteDuration);
            float env = 1f - (noteTime / noteDuration);
            data[i] = Mathf.Sin(2f * Mathf.PI * freqs[noteIndex] * noteTime) * 0.35f * env;
        }

        AudioClip clip = AudioClip.Create("SynthVictory", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip SynthesizeRadioSquelch()
    {
        int sampleRate = 22050;
        float duration = 0.14f;
        int numSamples = (int)(sampleRate * duration);
        float[] data = new float[numSamples];

        for (int i = 0; i < data.Length; i++)
        {
            float time = (float)i / sampleRate;
            float envelope = 1f - (time / duration);
            float noise = Random.Range(-0.35f, 0.35f);
            data[i] = (noise * 0.25f + Mathf.Sin(2f * Mathf.PI * 900f * time) * 0.15f) * envelope;
        }

        AudioClip clip = AudioClip.Create("SynthRadio", numSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

public enum AudioEvent
{
    RotateCable,
    ConnectCable,
    TowerActive,
    PuzzleSuccess,
    PuzzleFailure,
    ButtonClick,
    LevelComplete,
    RadioTransmission
}
