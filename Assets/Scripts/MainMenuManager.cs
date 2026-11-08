using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    private Canvas menuCanvas;
    private GameObject menuPanel;
    private GameObject settingsPanel;
    private GameObject tutorialPanel;

    public InputManager.ControlMode selectedMode = InputManager.ControlMode.EMG;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateMenuUI();
    }

    private void CreateMenuUI()
    {
        // Создаем отдельный Canvas для меню
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

        CreateWorkingDropdown(settingsPanel.transform, new Vector2(0, 300));

        CreateButton(settingsPanel.transform, "НАЗАД", new Vector2(0, -400), () => CloseSubMenus());
        settingsPanel.SetActive(false);

        // --- ПАНЕЛЬ ОБУЧЕНИЯ ---
        tutorialPanel = CreatePanel(canvasObj.transform, "TutorialPanel", new Color(0.05f, 0.05f, 0.08f, 0.98f));
        CreateText(tutorialPanel.transform, "Обучение", new Vector2(0, 700), 80, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);

        string tutorialText = "Добро пожаловать!\n\n" +
                              "Управление клешней имитирует\n" +
                              "работу бионического протеза.\n\n" +
                              "Используйте каналы ЭМГ для\n" +
                              "сжатия и разжатия пальцев.\n\n" +
                              "Цель: достать нужные игрушки!";

        CreateText(tutorialPanel.transform, tutorialText, new Vector2(0, 100), 45, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
        CreateButton(tutorialPanel.transform, "ПОНЯТНО", new Vector2(0, -600), () => CloseSubMenus());
        tutorialPanel.SetActive(false);
    }

    // --- ПРАВИЛЬНОЕ СОЗДАНИЕ DROPDOWN ---
    private void CreateWorkingDropdown(Transform parent, Vector2 pos)
    {
        GameObject dropdownObj = new GameObject("ControlModeDropdown");
        dropdownObj.transform.SetParent(parent, false);
        RectTransform rt = dropdownObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(500, 80);

        Image bgImage = dropdownObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();

        // 1. LABEL (Отображаемый текст)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(20, 0);
        lrt.offsetMax = new Vector2(-20, 0);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 36;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = labelText;

        // 2. TEMPLATE (Шаблон списка)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform trt = templateObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 0f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -5); // Небольшой отступ вниз
        trt.sizeDelta = new Vector2(0, 300); // Высота самого выпадающего окна

        Image templateImg = templateObj.AddComponent<Image>();
        templateImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();

        // 3. VIEWPORT (Маска)
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform vrt = viewportObj.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.sizeDelta = Vector2.zero;
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;
        viewportObj.AddComponent<Image>().color = Color.white;

        // 4. CONTENT (Список элементов)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform crt = contentObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(0, 80);

        // 5. ITEM (Один элемент списка - ОБЯЗАТЕЛЬНО С TOGGLE)
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform irt = itemObj.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0f, 0.5f);
        irt.anchorMax = new Vector2(1f, 0.5f);
        irt.sizeDelta = new Vector2(0, 80);

        // Фон элемента
        itemObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // ГЛАВНАЯ ОШИБКА БЫЛА ЗДЕСЬ — нужен Toggle!
        Toggle itemToggle = itemObj.AddComponent<Toggle>();

        // Фон-галочка (просто прозрачная картинка)
        GameObject checkObj = new GameObject("Item Checkmark");
        checkObj.transform.SetParent(itemObj.transform, false);
        RectTransform chrt = checkObj.AddComponent<RectTransform>();
        chrt.anchorMin = new Vector2(0f, 0.5f);
        chrt.anchorMax = new Vector2(0f, 0.5f);
        chrt.sizeDelta = new Vector2(30, 30);
        chrt.anchoredPosition = new Vector2(15, 0);
        checkObj.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f); // Синяя галочка
        itemToggle.graphic = checkObj.GetComponent<Image>();

        // Текст элемента
        GameObject itemTextObj = new GameObject("Item Label");
        itemTextObj.transform.SetParent(itemObj.transform, false);
        RectTransform itrt = itemTextObj.AddComponent<RectTransform>();
        itrt.anchorMin = Vector2.zero;
        itrt.anchorMax = Vector2.one;
        itrt.offsetMin = new Vector2(40, 0);
        itrt.offsetMax = new Vector2(-10, 0);
        Text itemText = itemTextObj.AddComponent<Text>();
        itemText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemText.fontSize = 32;
        itemText.color = Color.white;
        itemText.alignment = TextAnchor.MiddleLeft;

        // Связываем всё в Dropdown
        dropdown.template = trt;
        dropdown.captionText = labelText;
        dropdown.itemText = itemText;

        // Связываем ScrollRect
        scrollRect.content = crt;
        scrollRect.viewport = vrt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Настройки Toggle
        itemToggle.targetGraphic = itemObj.GetComponent<Image>();
        itemToggle.isOn = true;

        // Добавляем опции
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("ЭМГ (Клавиши A/D)"));
        dropdown.options.Add(new Dropdown.OptionData("СПППР (Клавиши W/S)"));
        dropdown.options.Add(new Dropdown.OptionData("Кнопки (Тач)"));

        dropdown.value = 0; // По умолчанию ЭМГ

        // Обработка выбора
        dropdown.onValueChanged.AddListener((index) =>
        {
            switch (index)
            {
                case 0: selectedMode = InputManager.ControlMode.EMG; break;
                case 1: selectedMode = InputManager.ControlMode.SPPPR; break;
                case 2: selectedMode = InputManager.ControlMode.DirectButtons; break;
            }
            Debug.Log($"[Menu] Выбран режим: {selectedMode}");
        });
    }

    private void StartGame()
    {
        if (menuCanvas != null) Destroy(menuCanvas.gameObject);

        if (FindObjectOfType<SceneBuilder>() != null)
        {
            FindObjectOfType<SceneBuilder>().BuildGame(selectedMode);
        }
    }

    private void ShowTutorial() { tutorialPanel.SetActive(true); }
    private void ShowSettings() { settingsPanel.SetActive(true); }
    private void CloseSubMenus() { tutorialPanel.SetActive(false); settingsPanel.SetActive(false); }
    private void QuitGame() { Application.Quit(); }

    // ... (Вспомогательные методы CreatePanel, CreateText, CreateButton) ...

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
        btn.onClick.AddListener(onClick);

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
