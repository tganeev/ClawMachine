using UnityEngine;
using UnityEngine.UI;

public class SignalUI : MonoBehaviour
{
    private Image bar1Fill;
    private Image bar2Fill;
    private Text valueText1;
    private Text valueText2;
    private float maxHeight = 200f;

        public void Initialize()
    {
        GameObject canvasObj = new GameObject("SignalCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Сдвигаем весь блок правее (X = 80) и поднимаем выше (Y = 280)
        Vector2 centerPos = new Vector2(80f, 280f);

        // Левая шкала (Канал 1)
        CreateProperSlider(canvasObj.transform, centerPos + new Vector2(-80f, 0), true, "Сжать");

        // Правая шкала (Канал 2)
        CreateProperSlider(canvasObj.transform, centerPos + new Vector2(80f, 0), false, "Разжать");
    }

    private void CreateProperSlider(Transform parent, Vector2 pos, bool isChannel1, string labelText)
    {
        // 1. ГЛАВНЫЙ КОНТЕЙНЕР (Slider)
        GameObject sliderObj = new GameObject("Slider_" + labelText);
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRt = sliderObj.AddComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0.5f, 0f);
        sliderRt.anchorMax = new Vector2(0.5f, 0f);
        sliderRt.pivot = new Vector2(0.5f, 0f);
        sliderRt.anchoredPosition = pos;

        // Высота самой шкалы осталась 170
        sliderRt.sizeDelta = new Vector2(60, 170f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.BottomToTop;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        // 2. BACKGROUND (Фон полоски)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        // 3. FILL AREA
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(5, 5);
        faRt.offsetMax = new Vector2(-5, -5);

        // 4. FILL
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(1, 0);
        fillRt.sizeDelta = Vector2.zero;

        Image fill = fillObj.AddComponent<Image>();
        fill.color = new Color(0.2f, 0.9f, 0.2f, 0.9f);

        if (isChannel1) bar1Fill = fill;
        else bar2Fill = fill;

        // 5. HANDLE SLIDE AREA
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(0, 10);
        haRt.offsetMax = new Vector2(0, -10);

        // 6. HANDLE
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleArea.transform, false);
        RectTransform hRt = handleObj.AddComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(60, 20);

        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = new Color(1f, 1f, 1f, 0.5f);

        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;

        // 7. ЧИСЛОВОЕ ЗНАЧЕНИЕ (Над шкалой)
        GameObject valueObj = new GameObject("ValueText");
        valueObj.transform.SetParent(sliderObj.transform, false);
        RectTransform vRt = valueObj.AddComponent<RectTransform>();

        vRt.anchorMin = new Vector2(0.5f, 1f);
        vRt.anchorMax = new Vector2(0.5f, 1f);
        vRt.pivot = new Vector2(0.5f, 0f);
        vRt.anchoredPosition = new Vector2(0, 10);
        vRt.sizeDelta = new Vector2(100, 40);

        Text valText = valueObj.AddComponent<Text>();
        valText.text = "0.00";
        valText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valText.fontSize = 28;
        valText.fontStyle = FontStyle.Bold;
        valText.color = Color.yellow;
        valText.alignment = TextAnchor.MiddleCenter;

        if (isChannel1) valueText1 = valText;
        else valueText2 = valText;

        // Слушатель
        slider.onValueChanged.AddListener((val) => {
            if (isChannel1) {
                InputManager.Instance.Channel1SliderValue = val;
                valueText1.text = val.ToString("F2");
            }
            else {
                InputManager.Instance.Channel2SliderValue = val;
                valueText2.text = val.ToString("F2");
            }
        });

        // 8. ПОДПИСЬ (Внизу шкалы)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0);
        lrt.anchorMax = new Vector2(0.5f, 0);
        lrt.pivot = new Vector2(0.5f, 1);

        // Опускаем текст "Сжать/Разжать" чуть ниже, чтобы освободить место для полоски
        lrt.anchoredPosition = new Vector2(0, -15);
        lrt.sizeDelta = new Vector2(120, 40);

        Text lText = labelObj.AddComponent<Text>();
        lText.text = labelText;
        lText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lText.fontSize = 24;
        lText.fontStyle = FontStyle.Bold;
        lText.color = Color.white;
        lText.alignment = TextAnchor.MiddleCenter;
    }
}
