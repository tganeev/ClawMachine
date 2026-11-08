using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    private Canvas menuCanvas;
    private GameObject menuPanel;
    private GameObject settingsPanel;
    private GameObject tutorialPanel;

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
        // Создаем отдельный Canvas для меню (поверх всего)
        GameObject canvasObj = new GameObject("MenuCanvas");
        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 100; // Меню поверх игрового UI

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- ГЛАВНАЯ ПАНЕЛЬ МЕНЮ (Темный фон) ---
        menuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.12f, 1f));

        // Заголовок
        CreateText(menuPanel.transform, "Claw Machine", new Vector2(0, 500), 100, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        CreateText(menuPanel.transform, "Бионический тренажер", new Vector2(0, 380), 50, new Color(0.5f, 0.8f, 1f), FontStyle.Italic, TextAnchor.MiddleCenter);

        // Кнопки
        Vector2 startPos = new Vector2(0, 100);
        float spacing = 150f;

        CreateButton(menuPanel.transform, "ИГРАТЬ", startPos, () => StartGame());
        CreateButton(menuPanel.transform, "ОБУЧЕНИЕ", startPos - new Vector2(0, spacing), () => ShowTutorial());
        CreateButton(menuPanel.transform, "НАСТРОЙКИ", startPos - new Vector2(0, spacing * 2), () => ShowSettings());
        CreateButton(menuPanel.transform, "ВЫХОД", startPos - new Vector2(0, spacing * 3), () => QuitGame());

        // --- ПАНЕЛЬ НАСТРОЕК ---
        settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", new Color(0.05f, 0.05f, 0.08f, 0.98f));
        CreateText(settingsPanel.transform, "Настройки", new Vector2(0, 600), 80, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        CreateText(settingsPanel.transform, "Здесь будут настройки звука и управления.", new Vector2(0, 400), 40, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
        CreateButton(settingsPanel.transform, "НАЗАД", new Vector2(0, -600), () => CloseSubMenus());
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

    private void StartGame()
    {
        // Прячем меню
        if (menuCanvas != null)
        {
            Destroy(menuCanvas.gameObject);
        }

        Debug.Log("[MainMenu] Игра запущена!");

        // Запускаем игру
        if (FindObjectOfType<SceneBuilder>() != null)
        {
            FindObjectOfType<SceneBuilder>().BuildGame();
        }
    }

    private void ShowTutorial() { tutorialPanel.SetActive(true); }
    private void ShowSettings() { settingsPanel.SetActive(true); }
    private void CloseSubMenus() { tutorialPanel.SetActive(false); settingsPanel.SetActive(false); }
    private void QuitGame() { Application.Quit(); }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ UI ---

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

        // Тень
        Image shadowImg = btnObj.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.6f);

        // Сама кнопка
        GameObject visualBtn = new GameObject("Visual");
        visualBtn.transform.SetParent(btnObj.transform, false);
        RectTransform vrt = visualBtn.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(0, 10);
        vrt.offsetMax = new Vector2(0, 0);

        Image img = visualBtn.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Синий цвет кнопок

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        btn.onClick.AddListener(onClick);

        // Текст
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
