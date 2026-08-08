using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class ProjectInitializer
{
    private const string LockFilePath = "Assets/initializer.lock";

    static ProjectInitializer()
    {
        // Delay execution until the first editor update to ensure Unity is fully loaded
        EditorApplication.delayCall += InitializeProject;
    }

    private static void InitializeProject()
    {
        if (File.Exists(LockFilePath))
        {
            return;
        }

        Debug.Log("Signal Lost: Initializing project assets, map grid, and scenes...");

        // 1. Create required directories
        string[] dirs = {
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/Editor",
            "Assets/Prefabs",
            "Assets/Sprites",
            "Assets/Audio",
            "Assets/UI",
            "Assets/Materials"
        };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        // 2. Load Font
        Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // 3. Generate default Sprites
        GenerateSprites();

        // 4. Create Tile Prefab for Cable Puzzle
        CreateTilePrefab();

        // 5. Create Scenes
        CreateMainMenuScene(defaultFont);
        CreateGameScene(defaultFont);

        // 6. Configure Build Settings
        ConfigureBuildSettings();

        // 7. Write Lock File and Refresh
        File.WriteAllText(LockFilePath, "Initialization Complete.");
        AssetDatabase.Refresh();

        Debug.Log("Signal Lost: Disaster Map initialization completed successfully!");
    }

    private static void GenerateSprites()
    {
        // Astra Drone Sprite (Blue circle, yellow eyes, antennas)
        CreateAndSaveSprite("Assets/Sprites/AstraSprite.png", (x, y) =>
        {
            float dx = x - 32f;
            float dy = y - 32f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // Eyes
            float eyeL = Mathf.Sqrt((dx + 6) * (dx + 6) + (dy - 4) * (dy - 4));
            float eyeR = Mathf.Sqrt((dx - 6) * (dx - 6) + (dy - 4) * (dy - 4));
            if (eyeL <= 3f || eyeR <= 3f) return Color.yellow;

            // Antennas
            if (Mathf.Abs(dx) >= 8f && Mathf.Abs(dx) <= 10f && dy >= 0f && dy <= 22f) return Color.gray;
            // Antenna tips
            float tipL = Mathf.Sqrt((dx + 9) * (dx + 9) + (dy - 22) * (dy - 22));
            float tipR = Mathf.Sqrt((dx - 9) * (dx - 9) + (dy - 22) * (dy - 22));
            if (tipL <= 3f || tipR <= 3f) return Color.red;

            // Main body
            if (dist <= 16f) return new Color(0.2f, 0.6f, 1.0f, 1.0f);
            // Side thrusters
            if (Mathf.Abs(dy) <= 3f && Mathf.Abs(dx) <= 24f && Mathf.Abs(dx) >= 16f) return Color.gray;

            return Color.clear;
        });

        // Damaged Tower Sprite (Dark base, mast, white satellite dish)
        CreateAndSaveSprite("Assets/Sprites/TowerSprite.png", (x, y) =>
        {
            float dx = x - 32f;

            // Satellite Dish
            if (y >= 44 && y <= 58 && Mathf.Abs(dx) <= 14f)
            {
                float dishCurve = (dx * dx) / 16f;
                if (y - 44 >= dishCurve && y - 44 <= dishCurve + 3f) return Color.white;
            }

            // Mast
            if (y >= 12 && y < 44 && Mathf.Abs(dx) <= 3f) return Color.gray;
            // Mast trusses
            if (y >= 12 && y < 44 && (y % 6 == 0 || Mathf.Abs(dx) - (y % 6) == 0)) return new Color(0.3f, 0.3f, 0.3f, 1.0f);

            // Base
            if (y < 12 && Mathf.Abs(dx) <= 12f - y) return new Color(0.2f, 0.2f, 0.2f, 1.0f);

            return Color.clear;
        });

        // Central Power Tower Sprite (Large golden dish, glowing energy core base)
        CreateAndSaveSprite("Assets/Sprites/CentralTowerSprite.png", (x, y) =>
        {
            float dx = x - 32f;

            // Massive Satellite Dish (Golden)
            if (y >= 42 && y <= 58 && Mathf.Abs(dx) <= 18f)
            {
                float dishCurve = (dx * dx) / 12f;
                if (y - 42 >= dishCurve && y - 42 <= dishCurve + 4f) return new Color(1.0f, 0.75f, 0.1f, 1.0f);
            }

            // Heavy Mast
            if (y >= 16 && y < 42 && Mathf.Abs(dx) <= 5f) return new Color(0.3f, 0.35f, 0.4f, 1.0f);
            // Energy conduit lines on mast
            if (y >= 16 && y < 42 && Mathf.Abs(dx) <= 1f) return new Color(1.0f, 0.85f, 0.2f, 1.0f);

            // Power Generator Base
            if (y < 16 && Mathf.Abs(dx) <= 16f - y)
            {
                // Glowing golden core
                if (Mathf.Abs(dx) <= 7f && y >= 4) return new Color(1.0f, 0.8f, 0.1f, 1.0f);
                return new Color(0.2f, 0.2f, 0.25f, 1.0f);
            }

            return Color.clear;
        });

        // Status Light (Circle with soft gradient)
        CreateAndSaveSprite("Assets/Sprites/StatusLightSprite.png", (x, y) =>
        {
            float dx = x - 32f;
            float dy = y - 32f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist <= 14f)
            {
                float alpha = 1.0f - (dist / 14f);
                return new Color(1.0f, 1.0f, 1.0f, alpha);
            }
            return Color.clear;
        });

        // Disaster Ground (Mud/Dark Grass)
        CreateAndSaveSprite("Assets/Sprites/MudTile.png", (x, y) =>
        {
            // Dark dark muddy greenish brown with slight noise
            float noise = Mathf.Sin(x * 0.3f) * Mathf.Cos(y * 0.3f);
            float r = 0.12f + noise * 0.02f;
            float g = 0.12f + noise * 0.01f;
            float b = 0.10f;
            return new Color(r, g, b, 1.0f);
        });

        // Road Sprite (Asphalt grey, yellow dashed lines, cracked textures)
        CreateAndSaveSprite("Assets/Sprites/RoadTile.png", (x, y) =>
        {
            Color c = new Color(0.18f, 0.18f, 0.20f, 1.0f); // Dark asphalt
            
            // Yellow dash center markings (vertical dash)
            if (x >= 30 && x <= 34 && y >= 16 && y <= 48)
            {
                return new Color(0.85f, 0.70f, 0.10f, 1.0f);
            }

            // Cracks
            if ((x + y) % 30 == 0 && x > 8 && x < 56)
            {
                return new Color(0.12f, 0.12f, 0.14f, 1.0f);
            }

            return c;
        });

        // Destroyed Building (Jagged concrete structures, hollow window frames)
        CreateAndSaveSprite("Assets/Sprites/BuildingTile.png", (x, y) =>
        {
            // Jagged broken top roof edge
            int jaggedRoof = 58 - (int)(Mathf.Sin(x * 0.5f) * 5f);
            if (y > jaggedRoof) return Color.clear;

            // Grid of broken window frames
            if (x % 16 >= 5 && x % 16 <= 11 && y % 16 >= 5 && y % 16 <= 11)
            {
                // Cracks/breaks inside windows (some clear pixels to represent breaks)
                if ((x * y) % 7 == 0) return Color.clear;
                return new Color(0.08f, 0.08f, 0.10f, 1.0f); // Void interior
            }

            // Concrete facade with cracks
            if ((x - y) % 24 == 0) return new Color(0.20f, 0.20f, 0.25f, 1.0f); // Crack
            return new Color(0.28f, 0.28f, 0.33f, 1.0f); // Base concrete
        });

        // Rubble Debris (Piles of rock, concrete block shards, brown wooden planks)
        CreateAndSaveSprite("Assets/Sprites/DebrisTile.png", (x, y) =>
        {
            float dx = x - 32f;
            float dy = y - 32f;

            // Planks (slanted lines)
            if (Mathf.Abs(dx + dy) <= 6f && Mathf.Abs(dx - dy) <= 22f)
            {
                return new Color(0.35f, 0.25f, 0.15f, 1.0f); // Wood brown
            }

            // Rubble pile bounds
            float rubbleShape = (dx * dx) / 1.5f + (dy * dy);
            if (rubbleShape <= 220f)
            {
                if ((x + y) % 6 < 2) return new Color(0.30f, 0.30f, 0.32f, 1.0f); // Shading
                return new Color(0.42f, 0.42f, 0.45f, 1.0f); // Concrete grey
            }

            return Color.clear;
        });

        // Water Sprite (Teal flooded streets with light blue waves)
        CreateAndSaveSprite("Assets/Sprites/WaterTile.png", (x, y) =>
        {
            Color baseWater = new Color(0.10f, 0.25f, 0.38f, 0.70f); // Translucent teal
            float wave = Mathf.Sin((x + y) * 0.18f) * Mathf.Cos((x - y) * 0.18f);
            if (wave > 0.6f)
            {
                return new Color(0.20f, 0.40f, 0.55f, 0.85f); // Wave highlights
            }
            return baseWater;
        });

        // Cable Puzzle: Straight (vertical line)
        CreateAndSaveSprite("Assets/Sprites/StraightCable.png", (x, y) =>
        {
            if (x >= 28 && x <= 36) return Color.white;
            return Color.clear;
        });

        // Cable Puzzle: L-Shape (Up to Right)
        CreateAndSaveSprite("Assets/Sprites/LCable.png", (x, y) =>
        {
            bool inUp = (x >= 28 && x <= 36 && y >= 28);
            bool inRight = (y >= 28 && y <= 36 && x >= 28);
            if (inUp || inRight) return Color.white;
            return Color.clear;
        });

        // Cable Puzzle: T-Shape (Up, Right, Down)
        CreateAndSaveSprite("Assets/Sprites/TCable.png", (x, y) =>
        {
            bool inVertical = (x >= 28 && x <= 36);
            bool inRight = (y >= 28 && y <= 36 && x >= 28);
            if (inVertical || inRight) return Color.white;
            return Color.clear;
        });

        // Cable Puzzle: Cross (Up, Right, Down, Left)
        CreateAndSaveSprite("Assets/Sprites/CrossCable.png", (x, y) =>
        {
            bool inVertical = (x >= 28 && x <= 36);
            bool inHorizontal = (y >= 28 && y <= 36);
            if (inVertical || inHorizontal) return Color.white;
            return Color.clear;
        });

        // Cable Puzzle: EndCap (Up only)
        CreateAndSaveSprite("Assets/Sprites/EndCapCable.png", (x, y) =>
        {
            bool inUp = (x >= 28 && x <= 36 && y >= 28);
            float dx = x - 32f;
            float dy = y - 32f;
            bool inDot = (dx * dx + dy * dy <= 8f * 8f);
            if (inUp || inDot) return Color.white;
            return Color.clear;
        });

        // Spotlight Cone Sprite (White spotlight cone pointing downwards)
        CreateAndSaveSprite("Assets/Sprites/SpotlightSprite.png", (x, y) =>
        {
            float dx = x - 32f;
            float dy = 63f - y; // distance from top of texture
            float width = dy * (24f / 63f);
            if (Mathf.Abs(dx) <= width && y < 63)
            {
                float verticalFade = (y / 63f); // 1 at top (apex), 0 at bottom
                float horizontalFade = 1f - (Mathf.Abs(dx) / (width + 0.1f));
                float alpha = verticalFade * horizontalFade * 0.8f;
                return new Color(1f, 1f, 1f, alpha);
            }
            return Color.clear;
        });
    }

    private static void CreateAndSaveSprite(string path, System.Func<int, int, Color> pixelColorFunc, int size = 64)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, pixelColorFunc(x, y));
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }

    private static void CreateTilePrefab()
    {
        string prefabPath = "Assets/Prefabs/TilePrefab.prefab";
        if (File.Exists(prefabPath)) return;

        GameObject tilePrefabObj = new GameObject("TilePrefab");
        tilePrefabObj.AddComponent<RectTransform>();
        tilePrefabObj.AddComponent<CanvasRenderer>();
        Image img = tilePrefabObj.AddComponent<Image>();
        img.color = Color.white;

        Button btn = tilePrefabObj.AddComponent<Button>();
        tilePrefabObj.AddComponent<CableTile>();

        PrefabUtility.SaveAsPrefabAsset(tilePrefabObj, prefabPath);
        Object.DestroyImmediate(tilePrefabObj);
    }

    private static void CreateMainMenuScene(Font font)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null) Object.DestroyImmediate(dirLight);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.06f, 1f); // Dark disaster background
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Attach ambient storm visual effects directly to the Main Menu camera
            cam.gameObject.AddComponent<LightningFlash>();
            RainController rain = cam.gameObject.AddComponent<RainController>();
            rain.dropSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StatusLightSprite.png");
            rain.dropCount = 30; // slightly fewer drops for menu cleanliness
        }

        // Load background sprites
        Sprite damagedTowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/TowerSprite.png");
        Sprite statusLightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StatusLightSprite.png");
        Sprite droneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/AstraSprite.png");

        // Spawn background ruined tower on the right
        GameObject menuTowerObj = new GameObject("MenuTower");
        menuTowerObj.transform.position = new Vector3(3.2f, -1.8f, 0);
        SpriteRenderer mtSR = menuTowerObj.AddComponent<SpriteRenderer>();
        mtSR.sprite = damagedTowerSprite;
        mtSR.sortingOrder = 3;

        GameObject mtLight = new GameObject("StatusLight");
        mtLight.transform.SetParent(menuTowerObj.transform);
        mtLight.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        SpriteRenderer mtLightSR = mtLight.AddComponent<SpriteRenderer>();
        mtLightSR.sprite = statusLightSprite;
        mtLightSR.color = Color.red;
        mtLightSR.sortingOrder = 4;
        mtLight.AddComponent<EmergencyStrobe>();

        Tower dummyTower = menuTowerObj.AddComponent<Tower>();
        dummyTower.statusLightRenderer = mtLightSR;
        dummyTower.isActive = false;

        // Spawn background hovering rescue drone on the left
        GameObject menuDroneObj = new GameObject("MenuDrone");
        menuDroneObj.transform.position = new Vector3(-3.2f, -1.2f, 0);
        SpriteRenderer mdSR = menuDroneObj.AddComponent<SpriteRenderer>();
        mdSR.sprite = droneSprite;
        mdSR.sortingOrder = 5;
        menuDroneObj.AddComponent<MenuDroneHover>();

        // Add dynamic light spotlight cone shining from menu drone
        GameObject menuSpotlight = new GameObject("Spotlight");
        menuSpotlight.transform.SetParent(menuDroneObj.transform);
        menuSpotlight.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        SpriteRenderer msSR = menuSpotlight.AddComponent<SpriteRenderer>();
        msSR.sprite = statusLightSprite;
        msSR.color = new Color(0.2f, 0.7f, 1.0f, 0.35f); // glowing cyan signal light
        msSR.sortingOrder = 4;
        menuSpotlight.transform.localScale = new Vector3(1.5f, 4.0f, 1f);
        menuSpotlight.transform.rotation = Quaternion.Euler(0, 0, -35f); // points down-right

        // Spawn UI Canvas overlay
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Main Menu Panel (Semi-transparent black overlay to let backgrounds show through)
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.02f, 0.04f, 0.55f); // translucent dark
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.text = "SIGNAL LOST";
        titleTxt.font = font;
        titleTxt.fontSize = 64;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.2f, 0.75f, 1.0f, 1.0f); // Neon Cyan title
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 130);
        titleRt.sizeDelta = new Vector2(600, 100);

        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(panelObj.transform, false);
        Text subtitleTxt = subtitleObj.AddComponent<Text>();
        subtitleTxt.text = "Reconnect. Restore. Rebuild.";
        subtitleTxt.font = font;
        subtitleTxt.fontSize = 18;
        subtitleTxt.alignment = TextAnchor.MiddleCenter;
        subtitleTxt.color = new Color(1.0f, 0.5f, 0.0f, 1.0f); // Emergency Orange subtitle
        RectTransform subtitleRt = subtitleObj.GetComponent<RectTransform>();
        subtitleRt.anchoredPosition = new Vector2(0, 75);
        subtitleRt.sizeDelta = new Vector2(600, 40);

        // Buttons Stack (PLAY, LEVELS, SETTINGS, EXIT)
        GameObject startBtnObj = CreateUIButton(panelObj.transform, "PlayButton", "PLAY", new Vector2(0, -10), font);
        Button startBtn = startBtnObj.GetComponent<Button>();

        GameObject levelsBtnObj = CreateUIButton(panelObj.transform, "LevelsButton", "LEVELS", new Vector2(0, -60), font);
        Button levelsBtn = levelsBtnObj.GetComponent<Button>();

        GameObject settingsBtnObj = CreateUIButton(panelObj.transform, "SettingsButton", "SETTINGS", new Vector2(0, -110), font);
        Button settingsBtn = settingsBtnObj.GetComponent<Button>();

        GameObject quitBtnObj = CreateUIButton(panelObj.transform, "ExitButton", "EXIT", new Vector2(0, -160), font);
        Button quitBtn = quitBtnObj.GetComponent<Button>();

        // Create the Level Selection Panel (Overlay)
        GameObject levelSelectPanelObj = new GameObject("LevelSelectPanel");
        levelSelectPanelObj.transform.SetParent(canvasObj.transform, false);
        Image selectImg = levelSelectPanelObj.AddComponent<Image>();
        selectImg.color = new Color(0.04f, 0.04f, 0.06f, 0.98f);
        RectTransform selectRt = levelSelectPanelObj.GetComponent<RectTransform>();
        selectRt.anchorMin = Vector2.zero;
        selectRt.anchorMax = Vector2.one;
        selectRt.sizeDelta = Vector2.zero;

        GameObject selectTitleObj = new GameObject("SelectTitleText");
        selectTitleObj.transform.SetParent(levelSelectPanelObj.transform, false);
        Text selectTitleTxt = selectTitleObj.AddComponent<Text>();
        selectTitleTxt.text = "LEVEL SELECTION";
        selectTitleTxt.font = font;
        selectTitleTxt.fontSize = 32;
        selectTitleTxt.alignment = TextAnchor.MiddleCenter;
        selectTitleTxt.color = new Color(0.2f, 0.7f, 1.0f, 1.0f);
        RectTransform selectTitleRt = selectTitleObj.GetComponent<RectTransform>();
        selectTitleRt.anchoredPosition = new Vector2(0, 140);
        selectTitleRt.sizeDelta = new Vector2(400, 50);

        Button[] levelButtons = new Button[5];
        Text[] starTexts = new Text[5];
        float[] rowY = { 80f, 35f, -10f, -55f, -100f };

        for (int i = 0; i < 5; i++)
        {
            int num = i + 1;
            // Spawn level button
            GameObject lBtnObj = CreateUIButton(levelSelectPanelObj.transform, "Level_" + num + "_Button", "LEVEL " + num, new Vector2(-70, rowY[i]), font);
            RectTransform lBtnRt = lBtnObj.GetComponent<RectTransform>();
            lBtnRt.sizeDelta = new Vector2(160, 32);
            levelButtons[i] = lBtnObj.GetComponent<Button>();

            // Spawn star text label beside it
            GameObject lStarObj = new GameObject("Level_" + num + "_Stars");
            lStarObj.transform.SetParent(levelSelectPanelObj.transform, false);
            Text lStarTxt = lStarObj.AddComponent<Text>();
            lStarTxt.text = "☆ ☆ ☆";
            lStarTxt.font = font;
            lStarTxt.fontSize = 18;
            lStarTxt.alignment = TextAnchor.MiddleLeft;
            lStarTxt.color = new Color(1.0f, 0.85f, 0.1f);
            RectTransform lStarRt = lStarObj.GetComponent<RectTransform>();
            lStarRt.anchoredPosition = new Vector2(100, rowY[i]);
            lStarRt.sizeDelta = new Vector2(140, 30);
            starTexts[i] = lStarTxt;
        }

        GameObject backBtnObj = CreateUIButton(levelSelectPanelObj.transform, "BackButton", "BACK", new Vector2(0, -150), font);
        Button backBtn = backBtnObj.GetComponent<Button>();

        // Create Settings Panel (Overlay)
        GameObject settingsPanelObj = new GameObject("SettingsPanel");
        settingsPanelObj.transform.SetParent(canvasObj.transform, false);
        Image settingsImg = settingsPanelObj.AddComponent<Image>();
        settingsImg.color = new Color(0.04f, 0.04f, 0.06f, 0.98f);
        RectTransform settingsRt = settingsPanelObj.GetComponent<RectTransform>();
        settingsRt.anchorMin = Vector2.zero;
        settingsRt.anchorMax = Vector2.one;
        settingsRt.sizeDelta = Vector2.zero;

        GameObject settingsTitleObj = new GameObject("SettingsTitleText");
        settingsTitleObj.transform.SetParent(settingsPanelObj.transform, false);
        Text settingsTitleTxt = settingsTitleObj.AddComponent<Text>();
        settingsTitleTxt.text = "SETTINGS";
        settingsTitleTxt.font = font;
        settingsTitleTxt.fontSize = 32;
        settingsTitleTxt.alignment = TextAnchor.MiddleCenter;
        settingsTitleTxt.color = new Color(0.2f, 0.7f, 1.0f, 1.0f);
        RectTransform settingsTitleRt = settingsTitleObj.GetComponent<RectTransform>();
        settingsTitleRt.anchoredPosition = new Vector2(0, 100);
        settingsTitleRt.sizeDelta = new Vector2(400, 50);

        GameObject resetBtnObj = CreateUIButton(settingsPanelObj.transform, "ResetProgressButton", "RESET PROGRESSION", new Vector2(0, 10), font);
        Button resetBtn = resetBtnObj.GetComponent<Button>();

        GameObject settingsBackBtnObj = CreateUIButton(settingsPanelObj.transform, "SettingsBackButton", "BACK", new Vector2(0, -60), font);
        Button settingsBackBtn = settingsBackBtnObj.GetComponent<Button>();

        // Spawn MainMenuController and wire links
        GameObject menuControllerObj = new GameObject("MainMenuController");
        MainMenuController controller = menuControllerObj.AddComponent<MainMenuController>();
        controller.mainMenuPanel = panelObj;
        controller.startButton = startBtn;
        controller.levelsButton = levelsBtn;
        controller.settingsButton = settingsBtn;
        controller.quitButton = quitBtn;
        controller.levelSelectPanel = levelSelectPanelObj;
        controller.levelButtons = levelButtons;
        controller.starTexts = starTexts;
        controller.backButton = backBtn;
        controller.settingsPanel = settingsPanelObj;
        controller.resetProgressButton = resetBtn;
        controller.settingsBackButton = settingsBackBtn;

        // Spawn AudioManager
        GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    private static void CreateGameScene(Font font)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Grid Boundaries & Specs
        int mapWidth = 30;
        int mapHeight = 20;

        // Assets
        Sprite mudSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MudTile.png");
        Sprite roadSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/RoadTile.png");
        Sprite buildingSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BuildingTile.png");
        Sprite debrisSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/DebrisTile.png");
        Sprite waterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/WaterTile.png");
        Sprite centralTowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CentralTowerSprite.png");
        Sprite damagedTowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/TowerSprite.png");
        Sprite statusLightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StatusLightSprite.png");

        // Main Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.0f;
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.06f, 1f); // Dark disaster atmosphere
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 0, -10);

        // Attach environmental rain and lightning effects
        camObj.AddComponent<LightningFlash>();
        RainController rain = camObj.AddComponent<RainController>();
        rain.dropSprite = statusLightSprite;

        CameraFollow camFollow = camObj.AddComponent<CameraFollow>();
        // Constrain camera position to prevent looking past the 30x20 tile boundary
        // Map extends from X: -15 to +15 and Y: -10 to +10.
        // Screen aspect ratio is usually 16:9. (Orthographic size 6 = 12 height units, ~21.3 width units)
        camFollow.useBounds = true;
        camFollow.minBounds = new Vector2(-4.3f, -4.0f);
        camFollow.maxBounds = new Vector2(4.3f, 4.0f);

        // Map Parent
        GameObject mapGridParent = new GameObject("MapGrid");

        // 30x20 Map Grid Construction
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float worldX = x - (mapWidth / 2f) + 0.5f;
                float worldY = y - (mapHeight / 2f) + 0.5f;

                // Determine Tile Type
                bool isRoad = (y == 10 || x == 15 || ((x == 6 || x == 24) && y >= 3 && y <= 17) || ((y == 4 || y == 16) && x >= 6 && x <= 24));
                bool isWater = (!isRoad && ((x >= 2 && x <= 5 && y >= 2 && y <= 3) || (x >= 25 && x <= 28 && y >= 17 && y <= 18)));
                bool isBuilding = (!isRoad && !isWater && ((x >= 2 && x <= 4 && y >= 12 && y <= 14) || (x >= 20 && x <= 22 && y >= 12 && y <= 14) || (x >= 2 && x <= 4 && y >= 6 && y <= 8) || (x >= 26 && x <= 28 && y >= 6 && y <= 8)));
                bool isDebris = (!isRoad && !isWater && !isBuilding && ((x == 1 && y == 1) || (x == 28 && y == 1) || (x == 1 && y == 18) || (x == 28 && y == 18) || (x == 10 && y == 15) || (x == 19 && y == 5)));

                GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.SetParent(mapGridParent.transform);
                tileObj.transform.position = new Vector3(worldX, worldY, 0);

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();

                if (isRoad)
                {
                    sr.sprite = roadSprite;
                    sr.sortingOrder = 1;
                    sr.color = new Color(0.6f, 0.65f, 0.8f); // Wet flooded dark tint
                }
                else if (isWater)
                {
                    sr.sprite = waterSprite;
                    sr.sortingOrder = 2;
                    // Astra drone can fly over water, so no solid collider is added
                }
                else if (isBuilding)
                {
                    sr.sprite = buildingSprite;
                    sr.sortingOrder = 4;
                    // Add solid collider
                    BoxCollider2D col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = Vector2.one;

                    // 30% chance for destroyed building to emit rising smoke columns
                    if (Random.Range(0, 100) < 30)
                    {
                        SmokeSpawner spawner = tileObj.AddComponent<SmokeSpawner>();
                        spawner.smokeSprite = statusLightSprite;
                    }
                }
                else if (isDebris)
                {
                    sr.sprite = debrisSprite;
                    sr.sortingOrder = 3;
                    // Add solid collider
                    BoxCollider2D col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = Vector2.one;
                }
                else
                {
                    sr.sprite = mudSprite;
                    sr.sortingOrder = 0;
                }
            }
        }

        // Central Power Station (Main Signal Source) - Placed at center (world pos: 0.5, 0.5)
        GameObject centralTowerObj = new GameObject("Central Power Station");
        centralTowerObj.transform.position = new Vector3(0.5f, 0.5f, 0);
        SpriteRenderer centralSR = centralTowerObj.AddComponent<SpriteRenderer>();
        centralSR.sprite = centralTowerSprite;
        centralSR.sortingOrder = 5;

        BoxCollider2D centralCol = centralTowerObj.AddComponent<BoxCollider2D>();
        centralCol.size = new Vector2(1.5f, 2.0f);

        // Glowing indicator on central tower
        GameObject centralLightObj = new GameObject("ActiveSignalLight");
        centralLightObj.transform.SetParent(centralTowerObj.transform);
        centralLightObj.AddComponent<EmergencyStrobe>();
        centralLightObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        SpriteRenderer centralLightSR = centralLightObj.AddComponent<SpriteRenderer>();
        centralLightSR.sprite = statusLightSprite;
        centralLightSR.color = new Color(1.0f, 0.8f, 0.1f, 1.0f); // Bright golden glow
        centralLightSR.sortingOrder = 6;

        // Floating Info Canvas for Central Tower
        GameObject centralCanvas = new GameObject("CentralPromptCanvas");
        centralCanvas.transform.SetParent(centralTowerObj.transform);
        centralCanvas.transform.localPosition = new Vector3(0, 1.6f, 0);
        Canvas cCanvas = centralCanvas.AddComponent<Canvas>();
        cCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform cCanvasRt = centralCanvas.GetComponent<RectTransform>();
        cCanvasRt.sizeDelta = new Vector2(8, 1);
        cCanvasRt.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        GameObject centralTextObj = new GameObject("InfoText");
        centralTextObj.transform.SetParent(centralCanvas.transform, false);
        Text cText = centralTextObj.AddComponent<Text>();
        cText.text = "MAIN POWER STATION\n[SIGNAL ACTIVE]";
        cText.font = font;
        cText.fontSize = 11;
        cText.alignment = TextAnchor.MiddleCenter;
        cText.color = new Color(1.0f, 0.8f, 0.1f, 1.0f);
        cText.horizontalOverflow = HorizontalWrapMode.Overflow;
        cText.verticalOverflow = VerticalWrapMode.Overflow;

        // Central tower ripple child
        GameObject centralRippleObj = new GameObject("RippleEffect");
        centralRippleObj.transform.SetParent(centralTowerObj.transform);
        centralRippleObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        SpriteRenderer centralRippleSR = centralRippleObj.AddComponent<SpriteRenderer>();
        centralRippleSR.sprite = statusLightSprite;
        centralRippleSR.color = new Color(1.0f, 0.8f, 0.1f, 0.4f); // Golden translucent ripple

        SignalRipple centralRippleComp = centralRippleObj.AddComponent<SignalRipple>();
        centralRippleComp.rippleSprite = statusLightSprite;
        centralRippleComp.rippleColor = new Color(1.0f, 0.8f, 0.1f, 0.4f);
        centralRippleComp.speed = 1.0f;
        centralRippleComp.maxScale = 3.0f;

        // Start disabled, script Start() enables it
        centralRippleObj.SetActive(false);

        // Configure PowerTower script component
        PowerTower powerTowerComp = centralTowerObj.AddComponent<PowerTower>();
        powerTowerComp.towerId = "PT_CENTER";
        powerTowerComp.towerName = "Central Power Station";
        powerTowerComp.statusLightRenderer = centralLightSR;
        powerTowerComp.interactionPrompt = centralCanvas;
        powerTowerComp.brokenSprite = centralLightSR.sprite;
        powerTowerComp.activeSprite = centralLightSR.sprite;
        powerTowerComp.brokenColor = Color.red;
        powerTowerComp.activeColor = new Color(1.0f, 0.8f, 0.1f, 1.0f);

        // Spawning Player (Astra) - starts right next to the Central Station
        GameObject playerObj = new GameObject("Astra");
        SpriteRenderer playerSR = playerObj.AddComponent<SpriteRenderer>();
        playerSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/AstraSprite.png");
        playerSR.sortingOrder = 6;

        Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        BoxCollider2D playerCol = playerObj.AddComponent<BoxCollider2D>();
        playerCol.size = new Vector2(0.8f, 0.8f);

        // Create Spotlight child
        GameObject lightChildObj = new GameObject("Spotlight");
        lightChildObj.transform.SetParent(playerObj.transform);
        lightChildObj.transform.localPosition = new Vector3(0, -0.4f, 0); // offset slightly down
        lightChildObj.transform.localRotation = Quaternion.identity;

        SpriteRenderer lightChildSR = lightChildObj.AddComponent<SpriteRenderer>();
        lightChildSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SpotlightSprite.png");
        lightChildSR.color = new Color(1.0f, 1.0f, 0.8f, 0.35f); // soft yellow spotlight
        lightChildSR.sortingOrder = 4; // draw above road/ground, below buildings/Astra

        DroneController droneCtrl = playerObj.AddComponent<DroneController>();
        droneCtrl.spotlightTransform = lightChildObj.transform;

        playerObj.transform.position = new Vector3(1.5f, 0.5f, 0); // Start on the east road

        // Setup camera target link
        camFollow.target = playerObj.transform;

        // Damaged Towers at road terminals
        Vector3[] towerPositions = {
            new Vector3(-10.5f, 5.5f, 0),  // NW Station (Col 4, Row 15)
            new Vector3(-10.5f, -4.5f, 0), // SW Station (Col 4, Row 5)
            new Vector3(10.5f, 5.5f, 0)    // NE Station (Col 25, Row 15)
        };
        string[] towerNames = {
            "Comm Tower NorthWest",
            "Comm Tower SouthWest",
            "Comm Tower NorthEast"
        };
        string[] towerIds = {
            "T_NW",
            "T_SW",
            "T_NE"
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject towerObj = new GameObject(towerNames[i]);
            SpriteRenderer towerSR = towerObj.AddComponent<SpriteRenderer>();
            towerSR.sprite = damagedTowerSprite;
            towerSR.sortingOrder = 5;
            towerObj.transform.position = towerPositions[i];

            BoxCollider2D towerCol = towerObj.AddComponent<BoxCollider2D>();
            towerCol.size = new Vector2(1.2f, 1.8f);

            // Red light indicating damaged state
            GameObject lightObj = new GameObject("StatusLight");
            lightObj.transform.SetParent(towerObj.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            SpriteRenderer lightSR = lightObj.AddComponent<SpriteRenderer>();
            lightSR.sprite = statusLightSprite;
            lightSR.color = Color.red;
            lightSR.sortingOrder = 6;

            // Alternates blinking strobes based on tower activity
            lightObj.AddComponent<EmergencyStrobe>();

            // Ripple child (disabled at start)
            GameObject rippleObj = new GameObject("RippleEffect");
            rippleObj.transform.SetParent(towerObj.transform);
            rippleObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            SpriteRenderer rippleSR = rippleObj.AddComponent<SpriteRenderer>();
            rippleSR.sprite = statusLightSprite;
            rippleSR.color = new Color(0.2f, 0.8f, 0.2f, 0.4f); // Green ripple

            SignalRipple rippleComp = rippleObj.AddComponent<SignalRipple>();
            rippleComp.rippleSprite = statusLightSprite;
            rippleComp.rippleColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
            rippleComp.speed = 1.2f;
            rippleComp.maxScale = 2.5f;

            rippleObj.SetActive(false); // starts inactive

            // Prompt Canvas
            GameObject promptCanvasObj = new GameObject("PromptCanvas");
            promptCanvasObj.transform.SetParent(towerObj.transform);
            promptCanvasObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            Canvas pCanvas = promptCanvasObj.AddComponent<Canvas>();
            pCanvas.renderMode = RenderMode.WorldSpace;
            RectTransform pCanvasRt = promptCanvasObj.GetComponent<RectTransform>();
            pCanvasRt.sizeDelta = new Vector2(6, 1);
            pCanvasRt.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            GameObject promptTextObj = new GameObject("PromptText");
            promptTextObj.transform.SetParent(promptCanvasObj.transform, false);
            Text pText = promptTextObj.AddComponent<Text>();
            pText.text = "REPAIR SIGNAL\n[E] Repair";
            pText.font = font;
            pText.fontSize = 12;
            pText.alignment = TextAnchor.MiddleCenter;
            pText.color = Color.yellow;
            pText.horizontalOverflow = HorizontalWrapMode.Overflow;
            pText.verticalOverflow = VerticalWrapMode.Overflow;

            Tower tComp = towerObj.AddComponent<Tower>();
            tComp.towerId = towerIds[i];
            tComp.towerName = towerNames[i];
            tComp.isActive = false;
            tComp.detectionRadius = 1.5f;
            tComp.statusLightRenderer = lightSR;
            tComp.interactionPrompt = promptCanvasObj;
            tComp.brokenSprite = lightSR.sprite;
            tComp.activeSprite = lightSR.sprite;
            tComp.brokenColor = Color.red;
            tComp.activeColor = Color.green;
        }

        // Canvas / HUD Configuration
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // HUD Panel (Top-Center emergency style)
        GameObject hudObj = new GameObject("HUDPanel");
        hudObj.transform.SetParent(canvasObj.transform, false);
        RectTransform hudRt = hudObj.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0.5f, 1f); // top center
        hudRt.anchorMax = new Vector2(0.5f, 1f);
        hudRt.pivot = new Vector2(0.5f, 1f);
        hudRt.anchoredPosition = new Vector2(0, -15); // 15 pixels below top
        hudRt.sizeDelta = new Vector2(520, 70);

        Image hudImg = hudObj.AddComponent<Image>();
        hudImg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f); // dark glass-like backing

        // Status Text (Time, moves, towers)
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(hudObj.transform, false);
        Text statusTxt = statusTextObj.AddComponent<Text>();
        statusTxt.text = "TIME: 00:00    |    MOVES: 0    |    TOWERS: 0/3";
        statusTxt.font = font;
        statusTxt.fontSize = 15;
        statusTxt.alignment = TextAnchor.MiddleCenter;
        statusTxt.color = new Color(1.0f, 0.75f, 0.15f, 1.0f); // High-vis amber warning text
        RectTransform statusTxtRt = statusTextObj.GetComponent<RectTransform>();
        statusTxtRt.anchorMin = new Vector2(0, 0.5f);
        statusTxtRt.anchorMax = new Vector2(1, 1);
        statusTxtRt.anchoredPosition = new Vector2(0, -2);
        statusTxtRt.sizeDelta = new Vector2(0, 0);

        // Signal Strength Text (Alert status indicator)
        GameObject signalTextObj = new GameObject("SignalText");
        signalTextObj.transform.SetParent(hudObj.transform, false);
        Text signalTxt = signalTextObj.AddComponent<Text>();
        signalTxt.text = "SIGNAL STATUS: 0% [ALERT: RESTORE GRID]";
        signalTxt.font = font;
        signalTxt.fontSize = 13;
        signalTxt.alignment = TextAnchor.MiddleCenter;
        signalTxt.color = new Color(0.2f, 0.8f, 1.0f, 1.0f); // Cyan alert state text
        RectTransform signalTxtRt = signalTextObj.GetComponent<RectTransform>();
        signalTxtRt.anchorMin = new Vector2(0, 0);
        signalTxtRt.anchorMax = new Vector2(1, 0.5f);
        signalTxtRt.anchoredPosition = new Vector2(0, 2);
        signalTxtRt.sizeDelta = new Vector2(0, 0);

        // Victory Panel (Centered)
        GameObject victoryObj = new GameObject("VictoryPanel");
        victoryObj.transform.SetParent(canvasObj.transform, false);
        Image vicImg = victoryObj.AddComponent<Image>();
        vicImg.color = new Color(0.04f, 0.04f, 0.08f, 0.98f);
        RectTransform vicRt = victoryObj.GetComponent<RectTransform>();
        vicRt.anchorMin = new Vector2(0.5f, 0.5f);
        vicRt.anchorMax = new Vector2(0.5f, 0.5f);
        vicRt.sizeDelta = new Vector2(480, 320); // polished width and height

        GameObject vicTitleObj = new GameObject("VictoryTitle");
        vicTitleObj.transform.SetParent(victoryObj.transform, false);
        Text vicTitleTxt = vicTitleObj.AddComponent<Text>();
        vicTitleTxt.text = "LEVEL COMPLETE!";
        vicTitleTxt.font = font;
        vicTitleTxt.fontSize = 32;
        vicTitleTxt.alignment = TextAnchor.MiddleCenter;
        vicTitleTxt.color = Color.green;
        RectTransform vicTitleRt = vicTitleObj.GetComponent<RectTransform>();
        vicTitleRt.anchoredPosition = new Vector2(0, 100);
        vicTitleRt.sizeDelta = new Vector2(380, 45);

        // Victory Stars text element
        GameObject vicStarsObj = new GameObject("VictoryStars");
        vicStarsObj.transform.SetParent(victoryObj.transform, false);
        Text vicStarsTxt = vicStarsObj.AddComponent<Text>();
        vicStarsTxt.text = "★ ★ ★";
        vicStarsTxt.font = font;
        vicStarsTxt.fontSize = 44;
        vicStarsTxt.alignment = TextAnchor.MiddleCenter;
        vicStarsTxt.color = new Color(1.0f, 0.85f, 0.1f);
        RectTransform vicStarsRt = vicStarsObj.GetComponent<RectTransform>();
        vicStarsRt.anchoredPosition = new Vector2(0, 40);
        vicStarsRt.sizeDelta = new Vector2(380, 50);

        GameObject vicDescObj = new GameObject("VictoryDesc");
        vicDescObj.transform.SetParent(victoryObj.transform, false);
        Text vicDescTxt = vicDescObj.AddComponent<Text>();
        vicDescTxt.text = "Time: 00:00\nMoves: 0";
        vicDescTxt.font = font;
        vicDescTxt.fontSize = 16;
        vicDescTxt.alignment = TextAnchor.MiddleCenter;
        vicDescTxt.color = Color.white;
        RectTransform vicDescRt = vicDescObj.GetComponent<RectTransform>();
        vicDescRt.anchoredPosition = new Vector2(0, -25);
        vicDescRt.sizeDelta = new Vector2(380, 60);

        // Horizontal Row of 3 Buttons: Next Level, Retry, Home
        GameObject vicNextBtn = CreateUIButton(victoryObj.transform, "NextLevelButton", "NEXT LEVEL", new Vector2(-130, -100), font);
        GameObject vicRestartBtn = CreateUIButton(victoryObj.transform, "RestartButton", "RETRY", new Vector2(0, -100), font);
        GameObject vicMenuBtn = CreateUIButton(victoryObj.transform, "MenuButton", "HOME", new Vector2(130, -100), font);

        // Puzzle Panel (Centered, overlay)
        GameObject puzzlePanelObj = new GameObject("PuzzlePanel");
        puzzlePanelObj.transform.SetParent(canvasObj.transform, false);
        Image pzlImg = puzzlePanelObj.AddComponent<Image>();
        pzlImg.color = new Color(0.06f, 0.06f, 0.08f, 0.98f);
        RectTransform pzlRt = puzzlePanelObj.GetComponent<RectTransform>();
        pzlRt.anchorMin = new Vector2(0.5f, 0.5f);
        pzlRt.anchorMax = new Vector2(0.5f, 0.5f);
        pzlRt.sizeDelta = new Vector2(400, 480);

        GameObject pzlTitleObj = new GameObject("PuzzleTitleText");
        pzlTitleObj.transform.SetParent(puzzlePanelObj.transform, false);
        Text pzlTitleTxt = pzlTitleObj.AddComponent<Text>();
        pzlTitleTxt.text = "REPAIR: COMM TOWER";
        pzlTitleTxt.font = font;
        pzlTitleTxt.fontSize = 24;
        pzlTitleTxt.alignment = TextAnchor.MiddleCenter;
        pzlTitleTxt.color = new Color(0.2f, 0.7f, 1.0f, 1.0f);
        RectTransform pzlTitleRt = pzlTitleObj.GetComponent<RectTransform>();
        pzlTitleRt.anchoredPosition = new Vector2(0, 200);
        pzlTitleRt.sizeDelta = new Vector2(350, 40);

        GameObject pzlSubObj = new GameObject("PuzzleSubtext");
        pzlSubObj.transform.SetParent(puzzlePanelObj.transform, false);
        Text pzlSubTxt = pzlSubObj.AddComponent<Text>();
        pzlSubTxt.text = "Click to rotate cables. Connect power (PWR) to transmitter (ANT).";
        pzlSubTxt.font = font;
        pzlSubTxt.fontSize = 11;
        pzlSubTxt.alignment = TextAnchor.MiddleCenter;
        pzlSubTxt.color = Color.gray;
        RectTransform pzlSubRt = pzlSubObj.GetComponent<RectTransform>();
        pzlSubRt.anchoredPosition = new Vector2(0, 170);
        pzlSubRt.sizeDelta = new Vector2(350, 30);

        // Grid Parent for tiles
        GameObject gridParentObj = new GameObject("GridParent");
        gridParentObj.transform.SetParent(puzzlePanelObj.transform, false);
        RectTransform gridParentRt = gridParentObj.AddComponent<RectTransform>();
        gridParentRt.anchoredPosition = new Vector2(0, -20);
        gridParentRt.sizeDelta = new Vector2(280, 280);

        GridLayoutGroup glg = gridParentObj.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(50, 50);
        glg.spacing = new Vector2(6, 6);
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment = TextAnchor.MiddleCenter;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;

        // Visual indicators flanking grid
        GameObject srcIndObj = new GameObject("SourceIndicator");
        srcIndObj.transform.SetParent(puzzlePanelObj.transform, false);
        Text srcIndTxt = srcIndObj.AddComponent<Text>();
        srcIndTxt.text = "PWR\n==>";
        srcIndTxt.font = font;
        srcIndTxt.fontSize = 12;
        srcIndTxt.alignment = TextAnchor.MiddleRight;
        srcIndTxt.color = Color.green;
        RectTransform srcIndRt = srcIndObj.GetComponent<RectTransform>();
        srcIndRt.anchoredPosition = new Vector2(-170, -20);
        srcIndRt.sizeDelta = new Vector2(50, 50);

        GameObject emIndObj = new GameObject("EmitterIndicator");
        emIndObj.transform.SetParent(puzzlePanelObj.transform, false);
        Text emIndTxt = emIndObj.AddComponent<Text>();
        emIndTxt.text = "==>\nANT";
        emIndTxt.font = font;
        emIndTxt.fontSize = 12;
        emIndTxt.alignment = TextAnchor.MiddleLeft;
        emIndTxt.color = new Color(0.2f, 0.7f, 1.0f, 1.0f);
        RectTransform emIndRt = emIndObj.GetComponent<RectTransform>();
        emIndRt.anchoredPosition = new Vector2(170, -20);
        emIndRt.sizeDelta = new Vector2(50, 50);

        GameObject pzlCancelBtn = CreateUIButton(puzzlePanelObj.transform, "CancelButton", "ABANDON REPAIR", new Vector2(0, -190), font);

        // CablePuzzleManager Config
        CablePuzzleManager pzlManager = puzzlePanelObj.AddComponent<CablePuzzleManager>();
        pzlManager.puzzlePanel = puzzlePanelObj;
        pzlManager.gridParent = gridParentObj.transform;
        pzlManager.towerNameText = pzlTitleTxt;
        pzlManager.closeButton = pzlCancelBtn.GetComponent<Button>();
        pzlManager.straightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StraightCable.png");
        pzlManager.cornerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LCable.png");
        pzlManager.tJunctionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/TCable.png");
        pzlManager.crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CrossCable.png");
        pzlManager.endPointSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EndCapCable.png");

        // Spawn NetworkManager
        GameObject netManagerObj = new GameObject("NetworkManager");
        netManagerObj.AddComponent<NetworkManager>();

        // Spawn LevelManager (Runtime Dynamic Loader)
        GameObject levelManagerObj = new GameObject("LevelManager");
        levelManagerObj.AddComponent<LevelManager>();

        // Spawn AudioManager (for testing standalone Game scene)
        GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();

        // GameManager Game Logic Linking
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.statusText = statusTxt;
        gm.signalStrengthText = signalTxt;
        gm.victoryPanel = victoryObj;
        gm.victoryTitleText = vicTitleTxt;
        gm.victoryStarsText = vicStarsTxt;
        gm.victoryDescText = vicDescTxt;
        gm.nextLevelButton = vicNextBtn.GetComponent<Button>();
        gm.restartButton = vicRestartBtn.GetComponent<Button>();
        gm.mainMenuButton = vicMenuBtn.GetComponent<Button>();

        // Radio UI Panel (Bottom Center)
        GameObject radioPanelObj = new GameObject("RadioPanel");
        radioPanelObj.transform.SetParent(canvasObj.transform, false);
        Image radioImg = radioPanelObj.AddComponent<Image>();
        radioImg.color = new Color(0.04f, 0.04f, 0.06f, 0.96f); // slate dark translucent backing
        RectTransform radioRt = radioPanelObj.GetComponent<RectTransform>();
        radioRt.anchorMin = new Vector2(0.5f, 0f); // bottom center
        radioRt.anchorMax = new Vector2(0.5f, 0f);
        radioRt.pivot = new Vector2(0.5f, 0f);
        radioRt.anchoredPosition = new Vector2(0f, 25f); // 25 pixels above bottom screen boundary
        radioRt.sizeDelta = new Vector2(460f, 110f);

        // Emergency-orange top trim line
        GameObject radioTrim = new GameObject("OrangeTrim");
        radioTrim.transform.SetParent(radioPanelObj.transform, false);
        Image trimImg = radioTrim.AddComponent<Image>();
        trimImg.color = new Color(1.0f, 0.5f, 0.0f, 1.0f); // Emergency Orange
        RectTransform trimRt = radioTrim.GetComponent<RectTransform>();
        trimRt.anchorMin = new Vector2(0f, 1f);
        trimRt.anchorMax = new Vector2(1f, 1f);
        trimRt.pivot = new Vector2(0.5f, 1f);
        trimRt.anchoredPosition = new Vector2(0f, 0f);
        trimRt.sizeDelta = new Vector2(0f, 3f); // 3 pixel line

        // Waveform graphic block
        GameObject radioWaveform = new GameObject("WaveformText");
        radioWaveform.transform.SetParent(radioPanelObj.transform, false);
        Text waveTxt = radioWaveform.AddComponent<Text>();
        waveTxt.text = "COMMS\n[|||||]";
        waveTxt.font = font;
        waveTxt.fontSize = 11;
        waveTxt.alignment = TextAnchor.MiddleCenter;
        waveTxt.color = new Color(0.2f, 0.7f, 1.0f, 0.7f); // glowing wave
        RectTransform waveRt = radioWaveform.GetComponent<RectTransform>();
        waveRt.anchorMin = new Vector2(0f, 0.5f);
        waveRt.anchorMax = new Vector2(0f, 0.5f);
        waveRt.pivot = new Vector2(0f, 0.5f);
        waveRt.anchoredPosition = new Vector2(15f, 0f);
        waveRt.sizeDelta = new Vector2(65f, 50f);

        // Sender name text
        GameObject senderObj = new GameObject("SenderNameText");
        senderObj.transform.SetParent(radioPanelObj.transform, false);
        Text senderTxt = senderObj.AddComponent<Text>();
        senderTxt.text = "HQ DISPATCH";
        senderTxt.font = font;
        senderTxt.fontSize = 13;
        senderTxt.alignment = TextAnchor.MiddleLeft;
        senderTxt.color = new Color(1.0f, 0.5f, 0.0f, 1.0f); // Emergency Orange
        RectTransform senderRt = senderObj.GetComponent<RectTransform>();
        senderRt.anchorMin = new Vector2(0f, 1f);
        senderRt.anchorMax = new Vector2(1f, 1f);
        senderRt.pivot = new Vector2(0.5f, 1f);
        senderRt.anchoredPosition = new Vector2(95f, -15f);
        senderRt.sizeDelta = new Vector2(-110f, 20f);

        // Message body text
        GameObject radioMsgObj = new GameObject("MessageText");
        radioMsgObj.transform.SetParent(radioPanelObj.transform, false);
        Text radioMsgTxt = radioMsgObj.AddComponent<Text>();
        radioMsgTxt.text = "Radio message text goes here...";
        radioMsgTxt.font = font;
        radioMsgTxt.fontSize = 13;
        radioMsgTxt.alignment = TextAnchor.UpperLeft;
        radioMsgTxt.color = Color.white;
        radioMsgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        radioMsgTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform radioMsgRt = radioMsgObj.GetComponent<RectTransform>();
        radioMsgRt.anchorMin = new Vector2(0f, 0f);
        radioMsgRt.anchorMax = new Vector2(1f, 1f);
        radioMsgRt.pivot = new Vector2(0.5f, 0.5f);
        radioMsgRt.anchoredPosition = new Vector2(95f, -38f);
        radioMsgRt.sizeDelta = new Vector2(-195f, -50f);

        // Acknowledge Button
        GameObject ackBtnObj = CreateUIButton(radioPanelObj.transform, "AcknowledgeButton", "ACKNOWLEDGE", new Vector2(165f, -35f), font);
        Button ackBtn = ackBtnObj.GetComponent<Button>();
        RectTransform ackRt = ackBtnObj.GetComponent<RectTransform>();
        ackRt.sizeDelta = new Vector2(100f, 25f);
        Text ackTxt = ackBtnObj.GetComponentInChildren<Text>();
        if (ackTxt != null) ackTxt.fontSize = 10;

        // Spawn RadioMessageManager component
        RadioMessageManager rmm = canvasObj.AddComponent<RadioMessageManager>();
        rmm.radioPanel = radioPanelObj;
        rmm.senderNameText = senderTxt;
        rmm.messageText = radioMsgTxt;
        rmm.continueButton = ackBtn;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
    }

    private static GameObject CreateUIButton(Transform parent, string name, string label, Vector2 pos, Font font)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.16f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btnObj.AddComponent<UIButtonSound>(); // Auto-bind UI click SFX

        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.12f, 0.12f, 0.16f, 1f);
        cb.highlightedColor = new Color(0.20f, 0.20f, 0.28f, 1f);
        cb.pressedColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        btn.colors = cb;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(190, 42);
        rt.anchoredPosition = pos;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = font;
        text.fontSize = 12;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[2];
        newScenes[0] = new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true);
        newScenes[1] = new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true);
        EditorBuildSettings.scenes = newScenes;
    }

    [MenuItem("Signal Lost/Build Windows Standalone")]
    public static void BuildWindowsStandalone()
    {
        string buildFolder = "Build/Windows";
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string exePath = Path.Combine(buildFolder, "SignalLost.exe");
        
        // Update build settings scenes list
        ConfigureBuildSettings();

        string[] scenes = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity"
        };

        Debug.Log("Signal Lost: Building Windows Standalone...");
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = exePath;
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Signal Lost: Build Succeeded! Saved to: {exePath} ({summary.totalSize / 1024 / 1024} MB)");
        }
        else
        {
            Debug.LogError($"Signal Lost: Build Failed with status: {summary.result}");
        }
    }
}
