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

        // Корова
        GameObject cowInstance = null;
        GameObject cowPrefab = Resources.Load<GameObject>("cow_plush_toy");
        if (cowPrefab != null)
        {
            cowInstance = Instantiate(cowPrefab, new Vector3(0.5f, 1.2f, 0f), Quaternion.Euler(-90, 190, 90));
            cowInstance.name = "CowToy";
            cowInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            if (cowInstance.GetComponent<Collider>() == null) cowInstance.AddComponent<BoxCollider>();
            Rigidbody rb = cowInstance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning("Модель cow_plush_toy не найдена!");
        }

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
        controller.grabDistance = 1.5f;
        controller.ropeTopY = 3.5f;

        if (cowInstance != null)
        {
            controller.targetToy = cowInstance;
        }

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

        // Табло со счетом и иконкой животного
        CreateScoreBoard(canvasObj.transform, "cow_icon");

        // Кнопки
        CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-400, 150), controller, true);
        CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(-400, 300), controller, false);

        Debug.Log("Сцена собрана!");
    }

    private void CreateScoreBoard(Transform parent, string iconResourceName)
    {
        // Фон табло
        GameObject scoreBoardObj = new GameObject("ScoreBoard");
        scoreBoardObj.transform.SetParent(parent, false);

        RectTransform rt = scoreBoardObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        // Захардкожено: Pos Y = -100
        rt.anchoredPosition = new Vector2(0f, -100f);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);

        Image img = scoreBoardObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // --- Иконка животного (слева) ---
        GameObject iconObj = new GameObject("TargetIcon");
        iconObj.transform.SetParent(scoreBoardObj.transform, false);

        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        // Привязываем к левому краю табло
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
            // Если PNG нет — генерируем иконку кодом
            iconSprite = CowIconGenerator.CreateCowIcon(128);
            Debug.Log("[SceneBuilder] Спрайт не найден, сгенерирована процедурная иконка коровы.");
        }

        iconImg.sprite = iconSprite;
        iconImg.color = Color.white;

        // --- Текст счета (справа от иконки) ---
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(scoreBoardObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        // Сдвигаем текст вправо, чтобы не перекрывался иконкой
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(100f, 0f); // отступ слева под иконку
        textRt.offsetMax = new Vector2(-10f, 0f); // небольшой отступ справа

        Text t = textObj.AddComponent<Text>();
        t.text = "СЧЕТ: 0";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.yellow;
        t.fontSize = 48;

        // Добавляем ScoreManager
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
