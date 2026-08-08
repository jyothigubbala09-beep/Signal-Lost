using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevel = 1;

    [Header("Spawning Coordinates")]
    public Vector3 powerStationPos = new Vector3(0.5f, 0.5f, 0f);

    private static readonly Vector3[] LevelTowerPositions = {
        new Vector3(-10.5f, 5.5f, 0),  // NW Station (Level 1+)
        new Vector3(10.5f, 5.5f, 0),   // NE Station (Level 2+)
        new Vector3(-10.5f, -4.5f, 0), // SW Station (Level 3+)
        new Vector3(10.5f, -4.5f, 0),  // SE Station (Level 4+)
        new Vector3(15.0f, 0.5f, 0)    // East Station (Level 5)
    };

    private static readonly string[] TowerNames = {
        "Comm Tower NorthWest",
        "Comm Tower NorthEast",
        "Comm Tower SouthWest",
        "Comm Tower SouthEast",
        "Comm Tower East"
    };

    private static readonly string[] TowerIds = {
        "T_NW",
        "T_NE",
        "T_SW",
        "T_SE",
        "T_E"
    };

    void Awake()
    {
        // Unlock Level 1 by default
        if (PlayerPrefs.GetInt("Level_1_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("Level_1_Unlocked", 1);
            PlayerPrefs.Save();
        }

        // 1. Locate editor templates
        GameObject powerTemplateObj = GameObject.Find("Central Power Station");
        GameObject towerTemplateObj = GameObject.Find("Comm Tower NorthWest");
        if (towerTemplateObj == null) towerTemplateObj = GameObject.Find("Comm Tower NorthEast");
        if (towerTemplateObj == null) towerTemplateObj = GameObject.Find("Comm Tower SouthWest");
        GameObject droneTemplateObj = GameObject.Find("Astra Rescue Drone");

        if (powerTemplateObj == null || towerTemplateObj == null || droneTemplateObj == null)
        {
            Debug.LogError("LevelManager: Editor scene templates missing!");
            return;
        }

        // De-activate templates
        powerTemplateObj.SetActive(false);
        towerTemplateObj.SetActive(false);
        droneTemplateObj.SetActive(false);

        // 2. Clamp selected level index
        int lvl = Mathf.Clamp(currentLevel, 1, 5);

        // 3. Spawn Central Power Station
        GameObject activePowerObj = Instantiate(powerTemplateObj, powerStationPos, Quaternion.identity);
        activePowerObj.name = "Central Power Station";
        activePowerObj.SetActive(true);
        PowerTower ptComp = activePowerObj.GetComponent<PowerTower>();
        if (ptComp != null)
        {
            ptComp.towerId = "PT_CENTER";
            ptComp.towerName = "Central Power Station";
            ptComp.isActive = true;
        }

        // 4. Spawn Damaged Towers based on Level Index (Level X has X towers)
        int towersCount = lvl;
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        if (nm != null)
        {
            nm.ClearRegistry(); // Wipe previous registrations
        }

        for (int i = 0; i < towersCount; i++)
        {
            GameObject towerObj = Instantiate(towerTemplateObj, LevelTowerPositions[i], Quaternion.identity);
            towerObj.name = TowerNames[i];
            towerObj.SetActive(true);

            Tower towerComp = towerObj.GetComponent<Tower>();
            if (towerComp != null)
            {
                towerComp.towerId = TowerIds[i];
                towerComp.towerName = TowerNames[i];
                towerComp.isActive = false;

                // Re-bind StatusLightRenderer child
                Transform statusLight = towerObj.transform.Find("StatusLight");
                if (statusLight != null)
                {
                    towerComp.statusLightRenderer = statusLight.GetComponent<SpriteRenderer>();
                    if (towerComp.statusLightRenderer != null)
                    {
                        towerComp.statusLightRenderer.color = towerComp.brokenColor;
                    }
                }
            }
        }

        // 5. Spawn Player Drone at offset
        GameObject activeDroneObj = Instantiate(droneTemplateObj, powerStationPos + new Vector3(-2f, 0f, 0f), Quaternion.identity);
        activeDroneObj.name = "Astra Rescue Drone";
        activeDroneObj.SetActive(true);

        // Re-enable player movement
        DroneController dc = activeDroneObj.GetComponent<DroneController>();
        if (dc != null)
        {
            dc.enabled = true;
        }

        // Re-bind camera tracking target
        CameraFollow camFollow = FindObjectOfType<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.target = activeDroneObj.transform;
        }

        // 6. Configure Cable Grid size based on level
        CablePuzzleManager puzzleMgr = FindObjectOfType<CablePuzzleManager>();
        if (puzzleMgr != null)
        {
            int size = 3;
            if (lvl == 2) size = 4;
            else if (lvl == 3 || lvl == 4) size = 5;
            else if (lvl == 5) size = 6;

            puzzleMgr.SetGridSize(size, size);
        }

        Debug.Log($"LevelManager: Loaded Level {lvl} with {towersCount} towers.");
    }
}
