using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        if (!isBuilt)
        {
            BuildScene();
            isBuilt = true;
        }
    }

    [ContextMenu("Собрать сцену заново")]
    public void BuildScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject) DestroyImmediate(obj);
        }

        // Камера
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 2.2f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        // Свет
        GameObject lightObj = new GameObject("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Модель автомата
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

        // Невидимый пол
        GameObject invisibleFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invisibleFloor.name = "InvisibleFloor";
        invisibleFloor.transform.position = new Vector3(0, 0.9f, 0);
        invisibleFloor.transform.localScale = new Vector3(4f, 0.2f, 4f);
        DestroyImmediate(invisibleFloor.GetComponent<Renderer>());

        // GameManager
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
        controller.minY = 1.2f;
        controller.lowerSpeed = 0.8f;
        controller.grabDistance = 0.3f;
        controller.ropeTopY = 3.5f;

        // Игрушки (вызываем один раз при старте)
        SpawnToys(controller);

        // Менеджер уровней
        GameObject lmObj = new GameObject("LevelManager");
        LevelManager levelManager = lmObj.AddComponent<LevelManager>();
        levelManager.clawController = controller;
        levelManager.sceneBuilder = this;

        // UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // Табло с уровнем, иконками и счетом
        CreateScoreBoard(canvasObj.transform, levelManager);

        // Большой текст по центру экрана
        CreateBigLevelText(canvasObj.transform, levelManager);

        // Кнопки
        CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-400, 150), controller, true);
        CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(-400, 300), controller, false);

        Debug.Log("Сцена собрана!");
    }

    public void SpawnToys(ClawController controller)
    {
        // Удаляем старые игрушки
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
            "Vinnie_the_Pooh",
            "kangaroo_kids_toy_plushie"
        };

        Quaternion baseRot = Quaternion.Euler(-90, 190, 90);
        Quaternion[] toyRotations = new Quaternion[] {
            baseRot,
            baseRot,
            Quaternion.Euler(-90f, 90f, 90f),
            Quaternion.Euler(-90f, 10f, 90f),
            Quaternion.Euler(0f, 0f, 0f),
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
        rt.sizeDelta = new Vector2(600, 120);
        rt.anchoredPosition = new Vector2(0f, -100f);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);

        Image img = scoreBoardObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Текст уровня
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform lrt = levelTextObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.5f);
        lrt.anchorMax = new Vector2(0f, 0.5f);
        lrt.pivot = new Vector2(0f, 0.5f);
        lrt.anchoredPosition = new Vector2(15f, 0f);
        lrt.sizeDelta = new Vector2(160f, 100f);

        Text levelText = levelTextObj.AddComponent<Text>();
        levelText.text = "УРОВЕНЬ 1";
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.alignment = TextAnchor.MiddleLeft;
        levelText.color = Color.white;
        levelText.fontSize = 32;

        // Контейнер иконок
        GameObject containerObj = new GameObject("IconsContainer");
        containerObj.transform.SetParent(scoreBoardObj.transform, false);

        RectTransform crt = containerObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 0.5f);
        crt.anchorMax = new Vector2(1f, 0.5f);
        crt.pivot = new Vector2(0f, 0.5f);
        crt.offsetMin = new Vector2(180f, -40f);
        crt.offsetMax = new Vector2(-160f, 40f);

        HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // Текст счета
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(scoreBoardObj.transform, false);
        RectTransform srt = scoreTextObj.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(1f, 0.5f);
        srt.anchorMax = new Vector2(1f, 0.5f);
        srt.pivot = new Vector2(1f, 0.5f);
        srt.anchoredPosition = new Vector2(-15f, 0f);
        srt.sizeDelta = new Vector2(150f, 100f);

        Text scoreText = scoreTextObj.AddComponent<Text>();
        scoreText.text = "СЧЕТ: 0";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.alignment = TextAnchor.MiddleRight;
        scoreText.color = Color.yellow;
        scoreText.fontSize = 32;

        ScoreManager scoreManager = scoreBoardObj.AddComponent<ScoreManager>();
        scoreManager.scoreText = scoreText;

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
        t.text = "УРОВЕНЬ 1";
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
