using UnityEngine;
using UnityEngine.UI;

public class SignalUI : MonoBehaviour
{
    private Image bar1Fill;
    private Image bar2Fill;
    private Text label1;
    private Text label2;

    private float maxHeight = 150f; // Сделали полоски чуть ниже

    public void Initialize()
    {
        GameObject canvasObj = new GameObject("SignalCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Располагаем полоски по центру, прямо над джойстиком
        Vector2 leftPos = new Vector2(-60f, 250f); // Левая полоска
        Vector2 rightPos = new Vector2(60f, 250f); // Правая полоска

        CreateBar(canvasObj.transform, leftPos, out bar1Fill, out label1, "СЖАТЬ");
        CreateBar(canvasObj.transform, rightPos, out bar2Fill, out label2, "РАЗЖАТЬ");
    }

    private void CreateBar(Transform parent, Vector2 pos, out Image fill, out Text label, string labelText)
    {
        // Фон полоски
        GameObject bgObj = new GameObject("BarBackground");
        bgObj.transform.SetParent(parent, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 0f);
        bgRt.anchorMax = new Vector2(0.5f, 0f);
        bgRt.pivot = new Vector2(0.5f, 0);
        bgRt.anchoredPosition = pos;
        bgRt.sizeDelta = new Vector2(50, maxHeight);

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f); // Темный фон

        // Заполняющая часть
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 0);
        fillRt.pivot = new Vector2(0.5f, 0);
        fillRt.sizeDelta = new Vector2(0, 0);

        fill = fillObj.AddComponent<Image>();
        fill.color = new Color(0.2f, 0.9f, 0.2f, 0.9f); // Ярко-зеленый

        // Подпись под полоской
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(bgObj.transform, false);
        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.5f, 0);
        labelRt.anchorMax = new Vector2(0.5f, 0);
        labelRt.pivot = new Vector2(0.5f, 1);
        labelRt.anchoredPosition = new Vector2(0, -10);
        labelRt.sizeDelta = new Vector2(120, 40);

        label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        float ch1 = InputManager.Instance.GetChannel1();
        float ch2 = InputManager.Instance.GetChannel2();

        if (bar1Fill != null)
        {
            bar1Fill.rectTransform.sizeDelta = new Vector2(0, maxHeight * ch1);
            bar1Fill.color = ch1 > 0.5f ? new Color(0.2f, 1f, 0.2f) : new Color(0.2f, 0.7f, 0.2f, 0.8f);
        }

        if (bar2Fill != null)
        {
            bar2Fill.rectTransform.sizeDelta = new Vector2(0, maxHeight * ch2);
            bar2Fill.color = ch2 > 0.5f ? new Color(0.2f, 1f, 0.2f) : new Color(0.2f, 0.7f, 0.2f, 0.8f);
        }
    }
}
