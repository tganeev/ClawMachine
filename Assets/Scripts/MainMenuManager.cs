using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    private Canvas menuCanvas;
    private GameObject menuPanel;
    private GameObject settingsPanel;
    private GameObject tutorialPanel;

    public InputManager.ControlMode selectedMode = InputManager.ControlMode.EMG;

    private GameObject dropdownPanel;
    private Text currentModeText;

    void Awake() { Instance = this; }
    void Start() { CreateMenuUI(); }

    private void CreateMenuUI()
    {
        // 1. ПРОВЕРКА КАМЕРЫ (С AudioListener)
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Menu Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>(); // <--- ДОБАВЛЕНО
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.orthographic = true;
            camObj.transform.position = new Vector3(0, 0, -10);
        }

        // 2. ПРОВЕРКА EVENTSYSTEM
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // 3. СОЗДАНИЕ UI
        GameObject canvasObj = new GameObject("MenuCanvas");
        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- ГЛАВНАЯ ПАНЕЛЬ МЕНЮ ---
        menuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.12f, 1f));
        CreateText(menuPanel.transform, "Claw Machine", new Vector2(0, 500), 100, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        CreateText(menuPanel.transform, "Бионический тренажер", new Vector2(0, 380), 50, new Color(0.5f, 0.8f, 1f), FontStyle.Italic, TextAnchor.MiddleCenter);

        Vector2 startPos = new Vector2(0, 100);
        float spacing = 150f;

        CreateButton(menuPanel.transform, "ИГРАТЬ", startPos, () => StartGame());
        CreateButton(menuPanel.transform, "ОБУЧЕНИЕ", startPos - new Vector2(0, spacing), () => ShowTutorial());
        CreateButton(menuPanel.transform, "НАСТРОЙКИ", startPos - new Vector2(0, spacing * 2), () => ShowSettings());
        CreateButton(menuPanel.transform, "ВЫХОД", startPos - new Vector2(0, spacing * 3), () => QuitGame());

        // --- ПАНЕЛЬ НАСТРОЕК ---
        settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", new Color(0.05f, 0.05f, 0.08f, 0.98f));
        CreateText(settingsPanel.transform, "Настройки", new Vector2(0, 700), 80, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        CreateText(settingsPanel.transform, "Режим управления:", new Vector2(0, 450), 50, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);

        CreateCustomDropdown(settingsPanel.transform, new Vector2(0, 300));
        CreateButton(settingsPanel.transform, "НАЗАД", new Vector2(0, -400), () => CloseSubMenus());
        settingsPanel.SetActive(false);

        // --- ПАНЕЛЬ ОБУЧЕНИЯ ---
        tutorialPanel = CreatePanel(canvasObj.transform, "TutorialPanel", new Color(0.05f, 0.05f, 0.08f, 0.98f));
        CreateText(tutorialPanel.transform, "Обучение", new Vector2(0, 700), 80, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        string tutorialText = "Добро пожаловать!\n\nУправление клешней имитирует\nработу бионического протеза.\n\nИспользуйте каналы ЭМГ для\nсжатия и разжатия пальцев.\n\nЦель: достать нужные игрушки!";
        CreateText(tutorialPanel.transform, tutorialText, new Vector2(0, 100), 45, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
        CreateButton(tutorialPanel.transform, "ПОНЯТНО", new Vector2(0, -600), () => CloseSubMenus());
        tutorialPanel.SetActive(false);
    }

    private void CreateCustomDropdown(Transform parent, Vector2 pos)
    {
        GameObject buttonObj = new GameObject("DropdownButton");
        buttonObj.transform.SetParent(parent, false);
        RectTransform brt = buttonObj.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = pos;
        brt.sizeDelta = new Vector2(500, 80);

        Image bgImg = buttonObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        Button btn = buttonObj.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        btn.onClick.AddListener(() => {
            if(SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            ToggleDropdown();
        });

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20, 0);
        trt.offsetMax = new Vector2(-20, 0);

        currentModeText = textObj.AddComponent<Text>();
        currentModeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        currentModeText.fontSize = 36;
        currentModeText.color = Color.white;
        currentModeText.alignment = TextAnchor.MiddleLeft;
        currentModeText.text = "ЭМГ (Клавиши A/D)";

        dropdownPanel = new GameObject("DropdownList");
        dropdownPanel.transform.SetParent(parent, false);
        RectTransform drt = dropdownPanel.AddComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.5f, 0.5f);
        drt.anchorMax = new Vector2(0.5f, 0.5f);
        drt.pivot = new Vector2(0.5f, 1f);
        drt.anchoredPosition = new Vector2(pos.x, pos.y - 40f - 5f);
        drt.sizeDelta = new Vector2(500, 240);

        Image listBg = dropdownPanel.AddComponent<Image>();
        listBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        CreateDropdownItem(dropdownPanel.transform, "ЭМГ (Клавиши A/D)", InputManager.ControlMode.EMG, -40f);
        CreateDropdownItem(dropdownPanel.transform, "СПППР (Клавиши W/S)", InputManager.ControlMode.SPPPR, -120f);
        CreateDropdownItem(dropdownPanel.transform, "Кнопки (Тач)", InputManager.ControlMode.DirectButtons, -200f);

        dropdownPanel.SetActive(false);
    }

    private void ToggleDropdown() => dropdownPanel.SetActive(!dropdownPanel.activeSelf);

    private void CreateDropdownItem(Transform parent, string text, InputManager.ControlMode mode, float yOffset)
    {
        GameObject itemObj = new GameObject("Item_" + text);
        itemObj.transform.SetParent(parent, false);
        RectTransform rt = itemObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(500, 80);

        Image img = itemObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button btn = itemObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => {
            if(SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            selectedMode = mode;
            currentModeText.text = text;
            dropdownPanel.SetActive(false);
        });

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(itemObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20, 0);
        trt.offsetMax = new Vector2(-20, 0);

        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 32;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
    }

    private void StartGame()
    {
        if (menuCanvas != null) Destroy(menuCanvas.gameObject);
        SceneBuilder builder = FindFirstObjectByType<SceneBuilder>();
        if (builder != null) builder.BuildGame(selectedMode);
    }

    private void ShowTutorial() { tutorialPanel.SetActive(true); }
    private void ShowSettings() { settingsPanel.SetActive(true); }
    private void CloseSubMenus() { tutorialPanel.SetActive(false); settingsPanel.SetActive(false); }
    private void QuitGame() { Application.Quit(); }

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        return panel;
    }

    private void CreateText(Transform parent, string text, Vector2 pos, int fontSize, Color color, FontStyle style, TextAnchor align)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, 200);
        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
    }

    private void CreateButton(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 110);

        Image shadowImg = btnObj.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject visualBtn = new GameObject("Visual");
        visualBtn.transform.SetParent(btnObj.transform, false);
        RectTransform vrt = visualBtn.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(0, 10);
        vrt.offsetMax = new Vector2(0, 0);

        Image img = visualBtn.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        btn.onClick.AddListener(() => {
            if(SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            onClick.Invoke();
        });

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(visualBtn.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 45;
        t.fontStyle = FontStyle.Bold;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
    }
}
