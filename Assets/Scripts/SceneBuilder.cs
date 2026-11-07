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

        // Игрушки
        SpawnToys(controller);

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

        // Табло со счетом
        CreateScoreBoard(canvasObj.transform, "cow_icon");

        // Кнопки
        CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-400, 150), controller, true);
        CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(-400, 300), controller, false);

        Debug.Log("Сцена собрана!");
    }

    private void SpawnToys(ClawController controller)
    {
        string[] toyPrefabNames = new string[] {
            "cow_plush_toy",
            "viral_monkey_plush_toy",         // Обезьянка
            "mario_soft_toy",                 // Марио
            "teddy_bear_3d_model",            // Медвежонок
            "Vinnie_the_Pooh",                // Винни-Пух
            "kangaroo_kids_toy_plushie"       // Кенгуру
        };

        // Повороты
        Quaternion baseRot = Quaternion.Euler(-90, 190, 90);
        Quaternion[] toyRotations = new Quaternion[] {
            baseRot,                                  // 0: Корова
            baseRot,                                  // 1: Обезьянка
            Quaternion.Euler(-90f, 90f, 90f),         // 2: Марио
            Quaternion.Euler(-90f, 10f, 90f),         // 3: Медвежонок (развернут на 180 градусов)
            Quaternion.Euler(0f, 0f, 0f),             // 4: Винни-Пух
            baseRot                                   // 5: Кенгуру
        };

        // Позиции
        Vector3[] toyPositions = new Vector3[] {
            new Vector3( 0.10f, 0f, -0.35f),  // Корова (передний левый)
            new Vector3( 0.40f, 0f,  0.20f),  // Обезьянка (задний центр)
            new Vector3( 0.40f, 0f, -0.35f),  // Марио (передний центр)
            new Vector3(-0.50f, 0f,  0.20f),  // Медвежонок (за отсеком)
            new Vector3( 0.40f, 0f,  0.00f),  // Винни-Пух (в середине)
            new Vector3( 0.10f, 0f,  0.20f),  // Кенгуру (задний левый)
        };

        // Индивидуальные высоты для игрушек (масштаб)
        float[] toyHeights = new float[] {
            0.38f,  // Корова
            0.38f,  // Обезьянка
            0.38f,  // Марио
            0.38f,  // Медвежонок
            0.38f,  // Винни-Пух
            0.55f   // Кенгуру (сделали больше)
        };

        float floorTopY = 1.0f;

        controller.toys.Clear();

        for (int i = 0; i < toyPrefabNames.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>(toyPrefabNames[i]);
            if (prefab == null)
            {
                Debug.LogWarning($"[SceneBuilder] Префаб '{toyPrefabNames[i]}' не найден в Resources!");
                continue;
            }

            GameObject toy = Instantiate(prefab, Vector3.zero, toyRotations[i]);
            toy.name = "Toy_" + toyPrefabNames[i];

            // Убираем старые коллайдеры
            foreach (var col in toy.GetComponentsInChildren<Collider>())
                DestroyImmediate(col);

            // Считаем общий bounding box
            Bounds bounds = new Bounds(toy.transform.position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            if (!hasBounds)
            {
                Debug.LogWarning($"[SceneBuilder] У префаба '{toyPrefabNames[i]}' нет рендереров, пропускаем.");
                DestroyImmediate(toy);
                continue;
            }

            // Берем индивидуальную высоту
            float targetHeight = toyHeights[i];

            // Масштабируем до targetHeight
            float currentHeight = bounds.size.y;
            if (currentHeight <= 0.0001f) currentHeight = 1f;
            float scaleFactor = targetHeight / currentHeight;
            toy.transform.localScale = toy.transform.localScale * scaleFactor;

            // Пересчитываем bounds после масштабирования
            bounds = new Bounds(toy.transform.position, Vector3.zero);
            hasBounds = false;
            foreach (var r in toy.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            // Сажаем на пол
            Vector3 pos = toyPositions[i];
            float currentBottom = bounds.min.y;
            float deltaY = floorTopY - currentBottom;

            toy.transform.position = new Vector3(pos.x, deltaY, pos.z);

            // Добавляем BoxCollider
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

        Debug.Log($"[SceneBuilder] Создано игрушек: {controller.toys.Count}");
    }

    private void CreateScoreBoard(Transform parent, string iconResourceName)
    {
        GameObject scoreBoardObj = new GameObject("ScoreBoard");
        scoreBoardObj.transform.SetParent(parent, false);

        RectTransform rt = scoreBoardObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        rt.anchoredPosition = new Vector2(0f, -100f);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);

        Image img = scoreBoardObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        GameObject iconObj = new GameObject("TargetIcon");
        iconObj.transform.SetParent(scoreBoardObj.transform, false);

        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0f, 0.5f);
        iconRt.sizeDelta = new Vector2(80f, 80f);
        iconRt.anchoredPosition = new Vector2(10f, 0f);

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        Sprite iconSprite = Resources.Load<Sprite>(iconResourceName);
        if (iconSprite == null)
        {
            iconSprite = CowIconGenerator.CreateCowIcon(128);
            Debug.Log("[SceneBuilder] Спрайт не найден, сгенерирована процедурная иконка коровы.");
        }

        iconImg.sprite = iconSprite;
        iconImg.color = Color.white;

        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(scoreBoardObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(100f, 0f);
        textRt.offsetMax = new Vector2(-10f, 0f);

        Text t = textObj.AddComponent<Text>();
        t.text = "СЧЕТ: 0";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.yellow;
        t.fontSize = 48;

        ScoreManager scoreManager = scoreBoardObj.AddComponent<ScoreManager>();
        scoreManager.scoreText = t;
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
