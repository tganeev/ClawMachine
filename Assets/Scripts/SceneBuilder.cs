using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        if (MainMenuManager.Instance == null)
        {
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuManager>();
        }
    }

    // Принимаем режим управления из меню
    public void BuildGame(InputManager.ControlMode mode)
    {
        if (isBuilt) return;

        // Очистка
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject && obj.name != "MainMenuManager" && obj.name != "MenuCanvas")
            {
                Destroy(obj);
            }
        }

        // Камера, Свет, Автомат, Пол...
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
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
        DestroyImmediate(invisibleFloor.GetComponent<Renderer>());

        // Менеджер ввода (создаем и сразу применяем выбранный режим)
        GameObject imObj = new GameObject("InputManager");
        InputManager inputManager = imObj.AddComponent<InputManager>();
        inputManager.UpdateInputMode(mode);

        // Клешня
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

        SpawnToys(controller);

        GameObject lmObj = new GameObject("LevelManager");
        LevelManager levelManager = lmObj.AddComponent<LevelManager>();
        levelManager.clawController = controller;
        levelManager.sceneBuilder = this;

        // UI
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

        CreateScoreBoard(canvasObj.transform, levelManager);
        CreateBigLevelText(canvasObj.transform, levelManager);

        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: КНОПКИ ТОЛЬКО ДЛЯ ТАЧ-РЕЖИМА ---
        if (mode == InputManager.ControlMode.DirectButtons)
        {
            CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ", new Vector2(-120f, 60f), controller, true);
            CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ", new Vector2(120f, 60f), controller, false);
            Debug.Log("[SceneBuilder] Созданы кнопки тач-управления.");
        }
        else
        {
            Debug.Log($"[SceneBuilder] Режим {mode}: экранные кнопки скрыты.");
            // Можно добавить индикаторы каналов (полоски ЭМГ), чтобы игрок видел сигнал
            CreateSignalIndicators(canvasObj.transform, inputManager);
        }

        isBuilt = true;
        Debug.Log("[SceneBuilder] Игра успешно запущена!");
    }

    // Простой визуализатор сигналов (полоски внизу экрана)
    private void CreateSignalIndicators(Transform parent, InputManager im)
    {
        // В будущем здесь можно сделать красивые полоски, которые заполняются при нажатии A/D.
        // Пока просто текстовое напоминание.
        GameObject textObj = new GameObject("ControlsHint");
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 120f);
        rt.sizeDelta = new Vector2(800, 100);

        Text t = textObj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 36;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        if (im.currentMode == InputManager.ControlMode.EMG)
            t.text = "Управление: A (Сжать) / D (Разжать)";
        else if (im.currentMode == InputManager.ControlMode.SPPPR)
            t.text = "Управление: W (Сжать) / S (Разжать)";
    }

    // ... (Остальные методы SpawnToys, CreateScoreBoard и т.д. оставляем без изменений) ...

    public void SpawnToys(ClawController controller) { /* ... код как раньше ... */ }
    private void CreateScoreBoard(Transform parent, LevelManager levelManager) { /* ... код как раньше ... */ }
    private void CreateBigLevelText(Transform parent, LevelManager levelManager) { /* ... код как раньше ... */ }
    private void CreateButton(Transform parent, string name, string text, Vector2 pos, ClawController controller, bool isGrab) { /* ... код как раньше ... */ }
}
