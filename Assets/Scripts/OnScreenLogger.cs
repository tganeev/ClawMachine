using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OnScreenLogger : MonoBehaviour
{
    public static OnScreenLogger Instance;

    [Header("Настройки лога")]
    public int maxLines = 12;
    public int fontSize = 24;

    private Text logText;
    private readonly Queue<string> lines = new Queue<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateLogUI();

        Application.logMessageReceived += HandleLog;

        Debug.Log("[OnScreenLogger] Логгер успешно запущен!");
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;

        if (Instance == this)
            Instance = null;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string prefix;

        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                prefix = "[ERROR] ";
                break;

            case LogType.Warning:
                prefix = "[WARN] ";
                break;

            default:
                prefix = "[LOG] ";
                break;
        }

        AddLine(prefix + logString);
    }

    void AddLine(string line)
    {
        if (logText == null)
            return;

        lines.Enqueue(line);

        while (lines.Count > maxLines)
            lines.Dequeue();

        logText.text = string.Join("\n", lines.ToArray());
    }

    void CreateLogUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");

            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ВАЖНО:
        // Image и Text должны быть на РАЗНЫХ объектах.
        // panelObj — фон.
        GameObject panelObj = new GameObject("OnScreenLogPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0f, 0.68f);
        panelRt.anchorMax = new Vector2(1f, 1f);
        panelRt.offsetMin = new Vector2(10f, 10f);
        panelRt.offsetMax = new Vector2(-10f, -10f);

        Image background = panelObj.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);
        background.raycastTarget = false;

        // textObj — отдельный дочерний объект для текста.
        GameObject textObj = new GameObject("OnScreenLogText");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 10f);
        textRt.offsetMax = new Vector2(-10f, -10f);

        logText = textObj.AddComponent<Text>();
        logText.font = GetBuiltinFont();
        logText.fontSize = fontSize;
        logText.alignment = TextAnchor.UpperLeft;
        logText.color = Color.yellow;
        logText.raycastTarget = false;
        logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        logText.verticalOverflow = VerticalWrapMode.Truncate;
        logText.supportRichText = false;

        panelObj.transform.SetAsLastSibling();
    }

    Font GetBuiltinFont()
    {
        Font font = null;

        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            font = null;
        }

        if (font == null)
        {
            try
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                font = null;
            }
        }

        return font;
    }
}
