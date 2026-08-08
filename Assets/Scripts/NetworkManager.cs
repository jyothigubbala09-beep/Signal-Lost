using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    private List<Tower> registeredTowers = new List<Tower>();
    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void RegisterTower(Tower tower)
    {
        if (tower != null && !registeredTowers.Contains(tower))
        {
            registeredTowers.Add(tower);
            Debug.Log($"NetworkManager: Registered tower '{tower.towerId}' ({tower.towerName})");
        }
    }

    public void OnTowerActivated(Tower tower)
    {
        Debug.Log($"NetworkManager: Tower '{tower.towerId}' successfully activated!");

        if (gameManager != null)
        {
            gameManager.OnTowerRestored(tower);
        }
    }

    public int GetActiveDamagedTowersCount()
    {
        int count = 0;
        foreach (var tower in registeredTowers)
        {
            if (tower != null && !tower.isPowerSource && tower.isActive)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalDamagedTowersCount()
    {
        int count = 0;
        foreach (var tower in registeredTowers)
        {
            if (tower != null && !tower.isPowerSource)
            {
                count++;
            }
        }
        return count;
    }

    public float GetSignalStrength()
    {
        int total = GetTotalDamagedTowersCount();
        if (total == 0) return 100f;

        int active = GetActiveDamagedTowersCount();
        return ((float)active / total) * 100f;
    }

    public void ClearRegistry()
    {
        registeredTowers.Clear();
    }
}
