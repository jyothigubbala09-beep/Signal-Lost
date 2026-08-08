using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadioMessageManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject radioPanel;
    public Text senderNameText;
    public Text messageText;
    public Button continueButton;

    private Queue<string> messageQueue = new Queue<string>();
    private System.Action onCompleteCallback;

    void Start()
    {
        if (radioPanel != null)
        {
            radioPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(DisplayNextMessage);
        }

        // Auto-start level introduction on load
        TriggerLevelIntroduction();
    }

    public void StartDialogue(string sender, string[] messages, System.Action onComplete)
    {
        senderNameText.text = sender;
        onCompleteCallback = onComplete;

        messageQueue.Clear();
        foreach (var msg in messages)
        {
            messageQueue.Enqueue(msg);
        }

        // Lock drone controls and stop speed
        var player = FindObjectOfType<DroneController>();
        if (player != null)
        {
            player.enabled = false;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        if (radioPanel != null)
        {
            radioPanel.SetActive(true);
        }

        DisplayNextMessage();
    }

    void DisplayNextMessage()
    {
        if (messageQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        string msg = messageQueue.Dequeue();
        if (messageText != null)
        {
            messageText.text = msg;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioEvent.RadioTransmission);
        }
    }

    void EndDialogue()
    {
        if (radioPanel != null)
        {
            radioPanel.SetActive(false);
        }

        // Unlock drone controls
        var player = FindObjectOfType<DroneController>();
        if (player != null)
        {
            player.enabled = true;
        }

        if (onCompleteCallback != null)
        {
            onCompleteCallback.Invoke();
        }
    }

    void TriggerLevelIntroduction()
    {
        int lvl = LevelManager.currentLevel;
        string sender = "HQ DISPATCH";
        string[] messages;

        switch (lvl)
        {
            case 1:
                messages = new string[] {
                    "Emergency channel active...",
                    "Cyclone 'Vardah' has knocked out primary communications.",
                    "Pilot drone ASTRA to repair the nearest offline tower."
                };
                break;
            case 2:
                messages = new string[] {
                    "Signal partially restored, but rescue teams need wider coverage.",
                    "Grid load increasing. We have located 2 more damaged towers."
                };
                break;
            case 3:
                messages = new string[] {
                    "Heavy rain is flooding key sectors.",
                    "Restore the 3 outlying towers to establish an emergency bridge."
                };
                break;
            case 4:
                messages = new string[] {
                    "Comms blackout in the eastern residential district.",
                    "Repairs will require complex cable routing. Proceed with caution."
                };
                break;
            case 5:
                messages = new string[] {
                    "This is the final sector. The wind is picking up.",
                    "Astra, reconnect all remaining towers to restore the complete network!"
                };
                break;
            default:
                messages = new string[] {
                    "Restore connections to the offline towers."
                };
                break;
        }

        StartDialogue(sender, messages, null);
    }
}
