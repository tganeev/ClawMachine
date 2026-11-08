using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        // Создаем стартовое меню при запуске
        if (MainMenuManager.Instance == null)
        {
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuManager>();
        }
    }

    // Вызывается из меню по кнопке "Играть"
    public void BuildGame()
    {
        if (isBuilt) return;

        Debug.Log("[SceneBuilder] Очистка старой сцены и сборка новой...");

        // 1. УДАЛЯЕМ ВСЁ СТАРОЕ ИЗ СЦЕНЫ (чтобы не было двух клешней)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Оставляем только сам SceneBuilder и меню (оно само себя удалит)
            if (obj != this.gameObject && obj.name != "MainMenuManager" && obj.name != "MenuCanvas")
            {
                Destroy(obj);
            }
        }

        // 2. СОЗДАЕМ КАМЕРУ (как было изначально)
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f); // Темный фон
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 2.2f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        // 3. СОЗДАЕМ СВЕТ (как было изначально)
        GameObject lightObj = new GameObject("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 4. МОДЕЛЬ АВТОМАТА
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

        // 5. НЕВИДИМЫЙ ПОЛ
        GameObject invisibleFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invisibleFloor.name = "InvisibleFloor";
        invisibleFloor.transform.position = new Vector3(0, 0.9f, 0);
        invisibleFloor.transform.localScale = new Vector3(4f, 0.2f, 4f);
        DestroyImmediate(invisibleFloor.GetComponent<Renderer>());

        // 6. ИГРОВАЯ ЛОГИКА (Клешня)
        GameObject gm = new GameObject("GameManager");
        ClawController controller = gm.AddComponent<ClawController>();

        controller.maxAngle = 55f;
        controller.speed = 100f;
        controller.invertHand1 = true;
        controller.invertHand2 = false;
        controller.invertHand3 = false;
        controller.squareSize = 0.4f;
        controller.moveSpeed = 0.7f;
        controller.offsetX = 0f;
        controller.offsetZ = 0f;
        controller.minY = 1.4f;
        controller.lowerSpeed = 0.8f;
        controller.grabDistance = 0.3f;
        controller.ropeTopY = 3.5f;

        // 7. ИГРУШКИ
        SpawnToys(controller);

        // 8. МЕНЕДЖЕР УРОВНЕЙ
        GameObject lmObj = new GameObject("LevelManager");
        LevelManager levelManager = lmObj.AddComponent<LevelManager>();
        levelManager.clawController = controller;
        levelManager.sceneBuilder = this;

        // 9. ИГРОВОЙ UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // 10. ТАБЛО И КНОПКИ
        CreateScoreBoard(canvasObj.transform, levelManager);
        CreateBigLevelText(canvasObj.transform, levelManager);
        CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-120f, 60f), controller, true);
        CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(120f, 60f), controller, false);

        isBuilt = true;
        Debug.Log("[SceneBuilder] Игра успешно запущена!");
    }

    public void SpawnToys(ClawController controller)
    {
        foreach (var toy in controller.toys)
        {
            if (toy != null) Destroy(toy);
        }
        controller.toys.Clear();

        string[] toyPrefabNames = new string[] {
            "cow_plush_toy",
            "labubu_classic_-_collectible_3d_figure",
            "mario_soft_toy",
            "teddy_bear_3d_model",
            "viral_monkey_plush_toy",
            "kangaroo_kids_toy_plushie"
        };

        Quaternion baseRot = Quaternion.Euler(-90, 190, 90);
        Quaternion[] toyRotations = new Quaternion[] {
            baseRot,
            baseRot,
            Quaternion.Euler(-90f, 90f, 90f),
            Quaternion.Euler(-90f, 10f, 90f),
            baseRot,
            baseRot
        };

        Vector3[] toyPositions = new Vector3[] {
            new Vector3( 0.10f, 0f, -0.35f),
            new Vector3( 0.40f, 0f,  0.20f),
            new Vector3( 0.40f, 0f, -0.35f),
            new Vector3(-0.50f, 0f,  0.20f),
            new Vector3( 0.40f, 0f,  0.00f),
            new Vector3( 0.10f, 0f,  0.20f),
        };

        float[] toyHeights = new float[] {
            0.38f, 0.38f, 0.38f, 0.38f, 0.38f, 0.55f
        };

        float floorTopY = 1.0f;

        for (int i = 0; i < toyPrefabNames.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>(toyPrefabNames[i]);
            if (prefab == null) continue;

            GameObject toy = Instantiate(prefab, Vector3.zero, toyRotations[i]);
            toy.name = "Toy_" + toyPrefabNames[i];

            foreach (var col in toy.GetComponentsInChildren<Collider>())
                DestroyImmediate(col);

            Bounds bounds = new Bounds(toy.transform.position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }

            if (!hasBounds) { DestroyImmediate(toy); continue; }

            float targetHeight = toyHeights[i];
            float currentHeight = bounds.size.y > 0.0001f ? bounds.size.y : 1f;
            float scaleFactor = targetHeight / currentHeight;
            toy.transform.localScale = toy.transform.localScale * scaleFactor;

            bounds = new Bounds(toy.transform.position, Vector3.zero);
            hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }

            Vector3 pos = toyPositions[i];
            float deltaY = floorTopY - bounds.min.y;
            toy.transform.position = new Vector3(pos.x, deltaY, pos.z);

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
        rt.sizeDelta = new Vector2(200, 75);
        rt.anchoredPosition = pos;
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);

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
        t.fontSize = 20;

        RectTransform trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
    }
}
