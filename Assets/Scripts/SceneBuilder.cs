using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        Debug.Log("[SceneBuilder] Инициализация...");

        // 1. СОЗДАЕМ SOUND MANAGER (с защитой от удаления)
        if (SoundManager.Instance == null)
        {
            GameObject smObj = new GameObject("SoundManager");
            smObj.AddComponent<SoundManager>();
        }

        // 2. ПРОВЕРКА КАМЕРЫ
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

        // 3. ПРОВЕРКА EVENTSYSTEM
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // 4. ЗАПУСКАЕМ МЕНЮ
        if (MainMenuManager.Instance == null)
        {
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuManager>();
        }
    }

    public void BuildGame(InputManager.ControlMode mode)
    {
        if (isBuilt) return;
        isBuilt = true;

        Debug.Log("[SceneBuilder] Очистка и сборка игры...");

        // 1. БЕЗОПАСНАЯ ОЧИСТКА
        // Мы НЕ используем DestroyImmediate для удаления меню, чтобы не сломать текущий кадр.
        // Мы просто удаляем всё лишнее, кроме SoundManager и EventSystem.
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject &&
                obj.name != "SoundManager" &&
                obj.name != "EventSystem")
            {
                // Удаляем именно объекты меню и старые камеры
                if (obj.name.Contains("Camera") || obj.name.Contains("Menu") ||
                    obj.name.Contains("Light") || obj.name.Contains("Canvas") ||
                    obj.name.Contains("Manager") || obj.name.Contains("Model") ||
                    obj.name.Contains("Floor") || obj.name.Contains("Toy_"))
                {
                    Destroy(obj);
                }
            }
        }

        // 2. ИГРОВАЯ КАМЕРА (с обязательным AudioListener)
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>(); // Звук в игре

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StartMusic();
        }


        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 2.2f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        // 3. СВЕТ
        GameObject lightObj = new GameObject("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 4. АВТОМАТ
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

        // 5. ПОЛ
        GameObject invisibleFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invisibleFloor.name = "InvisibleFloor";
        invisibleFloor.transform.position = new Vector3(0, 0.9f, 0);
        invisibleFloor.transform.localScale = new Vector3(4f, 0.2f, 4f);
        Destroy(invisibleFloor.GetComponent<Renderer>());

        // 6. ВВОД
        GameObject imObj = new GameObject("InputManager");
        InputManager inputManager = imObj.AddComponent<InputManager>();
        inputManager.UpdateInputMode(mode);

        // 7. КЛЕШНЯ
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

        // 8. ИГРУШКИ
        SpawnToys(controller);

        // 9. УРОВНИ
        GameObject lmObj = new GameObject("LevelManager");
        LevelManager levelManager = lmObj.AddComponent<LevelManager>();
        levelManager.clawController = controller;
        levelManager.sceneBuilder = this;

        // 10. ИГРОВОЙ UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        CreateScoreBoard(canvasObj.transform, levelManager);
        CreateBigLevelText(canvasObj.transform, levelManager);

        // 11. КНОПКИ
        if (mode == InputManager.ControlMode.DirectButtons)
        {
            CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-150f, 100f), controller, true);
            CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(150f, 100f), controller, false);
        }
        else
        {
            GameObject textObj = new GameObject("ControlsHint");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 150f);
            rt.sizeDelta = new Vector2(800, 100);

            Text t = textObj.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 40;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;

            t.text = mode == InputManager.ControlMode.EMG
                ? "Управление: A (Сжать) / D (Разжать)"
                : "Управление: W (Сжать) / S (Разжать)";
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

    private void CreateScoreBoard(Transform parent, LevelManager levelManager)
    {
        GameObject scoreBoardObj = new GameObject("ScoreBoard");
        scoreBoardObj.transform.SetParent(parent, false);

        RectTransform rt = scoreBoardObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(480f, 100f);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -40f);

        Image img = scoreBoardObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform lrt = levelTextObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.5f);
        lrt.anchorMax = new Vector2(0f, 0.5f);
        lrt.pivot = new Vector2(0f, 0.5f);
        lrt.anchoredPosition = new Vector2(20f, 0f);
        lrt.sizeDelta = new Vector2(120f, 80f);

        Text levelText = levelTextObj.AddComponent<Text>();
        levelText.text = "lvl: 1";
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.alignment = TextAnchor.MiddleLeft;
        levelText.color = Color.white;
        levelText.fontSize = 36;

        GameObject containerObj = new GameObject("IconsContainer");
        containerObj.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform crt = containerObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 0.5f);
        crt.anchorMax = new Vector2(1f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.offsetMin = new Vector2(140f, -35f);
        crt.offsetMax = new Vector2(-20f, 35f);

        HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 15f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ScoreManager scoreManager = scoreBoardObj.AddComponent<ScoreManager>();
        scoreManager.scoreText = null;

        levelManager.levelText = levelText;
        levelManager.iconsContainer = containerObj.transform;
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
