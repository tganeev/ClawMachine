using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        Debug.Log("[SceneBuilder] Инициализация...");

        if (SoundManager.Instance == null)
        {
            GameObject smObj = new GameObject("SoundManager");
            smObj.AddComponent<SoundManager>();
        }

        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Menu Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.orthographic = true;
            camObj.transform.position = new Vector3(0, 0, -10);
        }

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        if (MainMenuManager.Instance == null)
        {
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuManager>();
        }
    }

    public void BuildGame(InputManager.ControlMode mode, bool isTutorial = false)
    {
        if (isBuilt) return;
        isBuilt = true;

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject && obj.name != "SoundManager" && obj.name != "EventSystem")
            {
                Destroy(obj);
            }
        }

        if (SoundManager.Instance != null) SoundManager.Instance.StartMusic();

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 2.2f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        GameObject lightObj = new GameObject("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        GameObject clawMachinePrefab = Resources.Load<GameObject>("claw_machine");
        if (clawMachinePrefab != null)
        {
            GameObject machineInstance = Instantiate(clawMachinePrefab, Vector3.zero, Quaternion.Euler(-90, 90, 0));
            machineInstance.name = "ClawMachineModel";
            machineInstance.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);

            foreach (Transform child in machineInstance.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("SM_HorizontalBar")) child.gameObject.SetActive(false);
            }
        }

        GameObject invisibleFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invisibleFloor.name = "InvisibleFloor";
        invisibleFloor.transform.position = new Vector3(0, 0.9f, 0);
        invisibleFloor.transform.localScale = new Vector3(4f, 0.2f, 4f);
        Destroy(invisibleFloor.GetComponent<Renderer>());

        GameObject imObj = new GameObject("InputManager");
        InputManager inputManager = imObj.AddComponent<InputManager>();
        inputManager.UpdateInputMode(mode);

        GameObject gm = new GameObject("GameManager");
        ClawController controller = gm.AddComponent<ClawController>();

        controller.maxAngle = 55f;
        controller.speed = 100f;
        controller.invertHand1 = true;
        controller.squareSize = 0.4f;
        controller.moveSpeed = 0.7f;
        controller.minY = 1.4f;
        controller.lowerSpeed = 0.8f;
        controller.grabDistance = 0.3f;
        controller.ropeTopY = 3.5f;

        SpawnToys(controller);

        // --- UI ---
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject safeAreaObj = new GameObject("SafeArea");
        safeAreaObj.transform.SetParent(canvasObj.transform, false);
        RectTransform saRt = safeAreaObj.AddComponent<RectTransform>();
        saRt.anchorMin = Vector2.zero;
        saRt.anchorMax = Vector2.one;
        saRt.offsetMin = Vector2.zero;
        saRt.offsetMax = Vector2.zero;
        safeAreaObj.AddComponent<SafeAreaHandler>();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // --- ЛОГИКА УРОВНЕЙ ИЛИ ОБУЧЕНИЯ ---
        if (isTutorial)
        {
            GameObject tmObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tmObj.AddComponent<TutorialManager>();
            tutorialManager.Initialize(controller, safeAreaObj.transform);
        }
        else
        {
            GameObject lmObj = new GameObject("LevelManager");
            LevelManager levelManager = lmObj.AddComponent<LevelManager>();
            levelManager.clawController = controller;
            levelManager.sceneBuilder = this;

            CreateScoreBoard(safeAreaObj.transform, levelManager);

            GameObject mmObj = new GameObject("MissionManager");
            MissionManager missionManager = mmObj.AddComponent<MissionManager>();
            missionManager.Initialize(safeAreaObj.transform, levelManager.missionsAnchor);

            CreateBigLevelText(safeAreaObj.transform, levelManager);
        }

        // --- УПРАВЛЕНИЕ ---
        if (mode == InputManager.ControlMode.DirectButtons)
        {
            CreateButton(safeAreaObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-150f, 100f), controller, true);
            CreateButton(safeAreaObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(150f, 100f), controller, false);
        }
        else if (mode == InputManager.ControlMode.EMG)
        {
            // Подсказка убрана
        }
        else
        {
            CreateControlHint(safeAreaObj.transform, mode, "Управление: W (Сжать) / S (Разжать)");
        }

        if (mode != InputManager.ControlMode.DirectButtons)
        {
            GameObject sigObj = new GameObject("SignalUI");
            sigObj.transform.SetParent(safeAreaObj.transform, false);
            SignalUI signalUI = sigObj.AddComponent<SignalUI>();
            signalUI.Initialize();
        }
    }

        private void CreateScoreBoard(Transform parent, LevelManager levelManager)
    {
        GameObject scoreBoardObj = new GameObject("ScoreBoard");
        scoreBoardObj.transform.SetParent(parent, false);

        RectTransform rt = scoreBoardObj.AddComponent<RectTransform>();

        // УМЕНЬШИЛИ ВЫСОТУ на 5 (было 160, стало 155)
        rt.sizeDelta = new Vector2(680f, 150f);

        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);

        rt.anchoredPosition = new Vector2(0f, -15f);

        Image img = scoreBoardObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // --- КОЛОНКА 1: УРОВЕНЬ (Слева) ---
        GameObject levelPanel = new GameObject("LevelPanel");
        levelPanel.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform lprt = levelPanel.AddComponent<RectTransform>();
        lprt.anchorMin = new Vector2(0f, 0f);
        lprt.anchorMax = new Vector2(0.20f, 1f);
        lprt.offsetMin = Vector2.zero;
        lprt.offsetMax = Vector2.zero;

        GameObject lvlTitleObj = new GameObject("LvlTitle");
        lvlTitleObj.transform.SetParent(levelPanel.transform, false);
        RectTransform ltRt = lvlTitleObj.AddComponent<RectTransform>();
        ltRt.anchorMin = new Vector2(0f, 1f);
        ltRt.anchorMax = new Vector2(1f, 1f);
        ltRt.pivot = new Vector2(0.5f, 1f);
        ltRt.anchoredPosition = new Vector2(0, -5f);
        ltRt.sizeDelta = new Vector2(0, 30f);

        Text lvlTitle = lvlTitleObj.AddComponent<Text>();
        lvlTitle.text = "УРОВЕНЬ";
        lvlTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lvlTitle.alignment = TextAnchor.MiddleCenter;
        lvlTitle.color = Color.white;
        lvlTitle.fontStyle = FontStyle.Bold;
        lvlTitle.fontSize = 20;

        GameObject levelTextObj = new GameObject("LevelNumber");
        levelTextObj.transform.SetParent(levelPanel.transform, false);
        RectTransform ltrt = levelTextObj.AddComponent<RectTransform>();
        ltrt.anchorMin = new Vector2(0f, 0f);
        ltrt.anchorMax = new Vector2(1f, 1f);
        ltrt.offsetMin = new Vector2(0, 5f);
        ltrt.offsetMax = new Vector2(0, -35f);

        Text levelText = levelTextObj.AddComponent<Text>();
        levelText.text = "1";
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = Color.white;
        levelText.fontSize = 44;
        levelText.fontStyle = FontStyle.Bold;

        CreateDivider(scoreBoardObj.transform, 0.20f);

        // --- КОЛОНКА 2: МИССИИ (Центр) ---
        GameObject missionsPanel = new GameObject("MissionsPanel");
        missionsPanel.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform mprt = missionsPanel.AddComponent<RectTransform>();
        mprt.anchorMin = new Vector2(0.20f, 0f);
        mprt.anchorMax = new Vector2(0.75f, 1f);
        mprt.offsetMin = new Vector2(10f, 5f);
        mprt.offsetMax = new Vector2(-10f, -5f);

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(missionsPanel.transform, false);
        RectTransform trt = titleObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(0f, 1f);
        trt.pivot = new Vector2(0f, 1f);
        trt.anchoredPosition = new Vector2(5f, -5f);
        trt.sizeDelta = new Vector2(200f, 30f);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "МИССИИ:";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;
        titleText.fontSize = 22;

        GameObject missionsAnchor = new GameObject("MissionsAnchor");
        missionsAnchor.transform.SetParent(missionsPanel.transform, false);
        RectTransform mart = missionsAnchor.AddComponent<RectTransform>();
        mart.anchorMin = Vector2.zero;
        mart.anchorMax = Vector2.one;
        mart.offsetMin = new Vector2(5f, 5f);
        mart.offsetMax = new Vector2(-5f, -35f);

        levelManager.missionsAnchor = missionsAnchor.transform;

        CreateDivider(scoreBoardObj.transform, 0.75f);

        // --- КОЛОНКА 3: ИКОНКИ (Справа) ---
        GameObject iconsPanel = new GameObject("IconsPanel");
        iconsPanel.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform iprt = iconsPanel.AddComponent<RectTransform>();
        iprt.anchorMin = new Vector2(0.75f, 0f);
        iprt.anchorMax = new Vector2(1f, 1f);
        iprt.offsetMin = new Vector2(5f, 5f);
        iprt.offsetMax = new Vector2(-5f, -5f);

        GameObject containerObj = new GameObject("IconsContainer");
        containerObj.transform.SetParent(iconsPanel.transform, false);
        RectTransform crt = containerObj.AddComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.sizeDelta = Vector2.zero;

        GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(50f, 50f);
        grid.spacing = new Vector2(5f, 5f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        levelManager.levelText = levelText;
        levelManager.iconsContainer = containerObj.transform;
    }

    private void CreateDivider(Transform parent, float anchorX)
    {
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(parent, false);
        RectTransform divRt = divider.AddComponent<RectTransform>();
        divRt.anchorMin = new Vector2(anchorX, 0f);
        divRt.anchorMax = new Vector2(anchorX, 1f);
        divRt.pivot = new Vector2(0.5f, 0.5f);
        divRt.anchoredPosition = Vector2.zero;
        divRt.sizeDelta = new Vector2(2f, -20f);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    }

    private void CreateControlHint(Transform parent, InputManager.ControlMode mode, string customText = "")
    {
        GameObject textObj = new GameObject("ControlsHint");
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 150f);
        rt.sizeDelta = new Vector2(800, 100);

        Text t = textObj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 36;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.text = customText;
    }

    public void RebuildToMenu()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject && obj.name != "SoundManager" && obj.name != "EventSystem")
                Destroy(obj);
        }

        isBuilt = false;

        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Menu Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.orthographic = true;
            camObj.transform.position = new Vector3(0, 0, -10);
        }

        if (MainMenuManager.Instance == null)
        {
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuManager>();
        }
    }

    public void SpawnToys(ClawController controller)
    {
        foreach (var toy in controller.toys) { if (toy != null) Destroy(toy); }
        controller.toys.Clear();

        string[] toyPrefabNames = new string[] {
            "cow_plush_toy", "labubu_classic_-_collectible_3d_figure",
            "mario_soft_toy", "teddy_bear_3d_model",
            "viral_monkey_plush_toy", "kangaroo_kids_toy_plushie"
        };

        Quaternion baseRot = Quaternion.Euler(-90, 190, 90);
        Quaternion[] toyRotations = new Quaternion[] {
            baseRot, baseRot, Quaternion.Euler(-90f, 90f, 90f),
            Quaternion.Euler(-90f, 10f, 90f), baseRot, baseRot
        };

        Vector3[] toyPositions = new Vector3[] {
            new Vector3( 0.10f, 0f, -0.35f), new Vector3( 0.40f, 0f,  0.20f),
            new Vector3( 0.40f, 0f, -0.35f), new Vector3(-0.50f, 0f,  0.20f),
            new Vector3( 0.40f, 0f,  0.00f), new Vector3( 0.10f, 0f,  0.20f),
        };

        float[] toyHeights = new float[] { 0.38f, 0.38f, 0.38f, 0.38f, 0.38f, 0.55f };
        float floorTopY = 1.0f;

        for (int i = 0; i < toyPrefabNames.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>(toyPrefabNames[i]);
            if (prefab == null) continue;

            GameObject toy = Instantiate(prefab, Vector3.zero, toyRotations[i]);
            toy.name = "Toy_" + toyPrefabNames[i];

            foreach (var col in toy.GetComponentsInChildren<Collider>()) Destroy(col);

            Bounds bounds = new Bounds(toy.transform.position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }
            if (!hasBounds) { Destroy(toy); continue; }

            float targetHeight = toyHeights[i];
            float currentHeight = bounds.size.y > 0.0001f ? bounds.size.y : 1f;
            toy.transform.localScale = toy.transform.localScale * (targetHeight / currentHeight);

            bounds = new Bounds(toy.transform.position, Vector3.zero);
            hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }

            Vector3 pos = toyPositions[i];
            toy.transform.position = new Vector3(pos.x, floorTopY - bounds.min.y, pos.z);

            BoxCollider box = toy.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, targetHeight * 0.5f, 0f);
            box.size = new Vector3(
                bounds.size.x / Mathf.Max(0.0001f, toy.transform.lossyScale.x) * 0.8f,
                targetHeight / Mathf.Max(0.0001f, toy.transform.lossyScale.y) * 0.9f,
                bounds.size.z / Mathf.Max(0.0001f, toy.transform.lossyScale.z) * 0.8f
            );

            Rigidbody rb = toy.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            controller.toys.Add(toy);
        }
    }

    private void CreateBigLevelText(Transform parent, LevelManager levelManager)
    {
        GameObject textObj = new GameObject("BigLevelText");
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 300);

        Text t = textObj.AddComponent<Text>();
        t.text = "Уровень 1";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.fontSize = 120;
        t.fontStyle = FontStyle.Bold;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4, -4);

        textObj.SetActive(false);
        levelManager.bigLevelText = t;
    }

    private void CreateButton(Transform parent, string name, string text, Vector2 pos, ClawController controller, bool isGrab)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 100);
        rt.anchoredPosition = pos;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);

        Image img = btnObj.AddComponent<Image>();
        img.color = isGrab ? new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.6f, 0.2f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        UIButtonController uiBtn = btnObj.AddComponent<UIButtonController>();
        uiBtn.clawController = controller;
        uiBtn.isGrabButton = isGrab;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.fontSize = 30;

        RectTransform trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
    }
}
