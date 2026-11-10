using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    private class Mission
    {
        public string description;
        public bool isCompleted;
        public bool isFailed;
        public System.Action updateAction;
    }

    private List<Mission> activeMissions = new List<Mission>();

    private Image timerDigitTens;
    private Image timerDigitOnes;
    private Text missionsText;

    private float levelStartTime;
    private float timeLimit = 60f;
    private int toysCollectedThisLevel = 0;
    private int totalToysRequired = 0;
    private bool hasUsedChannel2 = false;
    private bool hasUsedChannel1 = false;
    private bool hasFailed = false;
    private bool isLevelCompleted = false;

    private Sprite[] digitSprites = new Sprite[10];

    void Awake()
    {
        Instance = this;
        GenerateDigitSprites();
    }

    // --- ГЕНЕРАЦИЯ СПРАЙТОВ ---
    private void GenerateDigitSprites()
    {
        int size = 128;
        for (int i = 0; i < 10; i++)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[size * size];
            for (int j = 0; j < px.Length; j++) px[j] = new Color(0, 0, 0, 0);
            DrawDigitToPixels(px, size, i);
            tex.SetPixels(px);
            tex.Apply();
            digitSprites[i] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }

    private void DrawDigitToPixels(Color[] px, int size, int digit)
    {
        Color red = new Color(1f, 0f, 0f);
        int thick = size / 10;
        int margin = size / 10;
        int w = size - margin * 2;
        int h = size - margin * 2;
        bool[] seg = new bool[7];
        switch (digit)
        {
            case 0: seg = new bool[] { true, true, true, true, true, true, false }; break;
            case 1: seg = new bool[] { false, true, true, false, false, false, false }; break;
            case 2: seg = new bool[] { true, true, false, true, true, false, true }; break;
            case 3: seg = new bool[] { true, true, true, true, false, false, true }; break;
            case 4: seg = new bool[] { false, true, true, false, false, true, true }; break;
            case 5: seg = new bool[] { true, false, true, true, false, true, true }; break;
            case 6: seg = new bool[] { true, false, true, true, true, true, true }; break;
            case 7: seg = new bool[] { true, true, true, false, false, false, false }; break;
            case 8: seg = new bool[] { true, true, true, true, true, true, true }; break;
            case 9: seg = new bool[] { true, true, true, true, false, true, true }; break;
        }
        if (seg[0]) DrawRect(px, size, margin + thick, margin + h - thick, w - thick * 2, thick, red);
        if (seg[1]) DrawRect(px, size, margin + w - thick, margin + h / 2, thick, h / 2 - thick, red);
        if (seg[2]) DrawRect(px, size, margin + w - thick, margin, thick, h / 2 - thick, red);
        if (seg[3]) DrawRect(px, size, margin + thick, margin, w - thick * 2, thick, red);
        if (seg[4]) DrawRect(px, size, margin, margin, thick, h / 2 - thick, red);
        if (seg[5]) DrawRect(px, size, margin, margin + h / 2, thick, h / 2 - thick, red);
        if (seg[6]) DrawRect(px, size, margin + thick, margin + h / 2 - thick / 2, w - thick * 2, thick, red);
    }

    private void DrawRect(Color[] px, int size, int x, int y, int w, int h, Color color)
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int pxX = x + i;
                int pxY = y + j;
                if (pxX >= 0 && pxX < size && pxY >= 0 && pxY < size) px[pxY * size + pxX] = color;
            }
        }
    }

    // --- ИНИЦИАЛИЗАЦИЯ ---
    public void Initialize(Transform canvasParent, Transform missionsAnchor)
    {
        GameObject timerBgObj = new GameObject("TimerBackground");
        timerBgObj.transform.SetParent(canvasParent, false);
        RectTransform bgRt = timerBgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 0f);
        bgRt.anchorMax = new Vector2(0.5f, 0f);
        bgRt.pivot = new Vector2(0.5f, 0f);
        bgRt.anchoredPosition = new Vector2(-213.3f, 210f);
        bgRt.sizeDelta = new Vector2(120f, 120f);
        Image bgImg = timerBgObj.AddComponent<Image>();
        bgImg.color = Color.black;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(timerBgObj.transform, false);
        RectTransform contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.anchorMin = Vector2.zero;
        contentRt.anchorMax = Vector2.one;
        contentRt.offsetMin = new Vector2(15f, 15f);
        contentRt.offsetMax = new Vector2(-15f, -15f);

        GameObject tensObj = new GameObject("Tens");
        tensObj.transform.SetParent(contentObj.transform, false);
        RectTransform tensRt = tensObj.AddComponent<RectTransform>();
        tensRt.anchorMin = new Vector2(0f, 0f);
        tensRt.anchorMax = new Vector2(0.5f, 1f);
        tensRt.sizeDelta = new Vector2(-2, 0);
        tensRt.anchoredPosition = new Vector2(0, 0);
        timerDigitTens = tensObj.AddComponent<Image>();
        timerDigitTens.preserveAspect = true;

        GameObject onesObj = new GameObject("Ones");
        onesObj.transform.SetParent(contentObj.transform, false);
        RectTransform onesRt = onesObj.AddComponent<RectTransform>();
        onesRt.anchorMin = new Vector2(0.5f, 0f);
        onesRt.anchorMax = new Vector2(1f, 1f);
        onesRt.sizeDelta = new Vector2(-2, 0);
        onesRt.anchoredPosition = new Vector2(0, 0);
        timerDigitOnes = onesObj.AddComponent<Image>();
        timerDigitOnes.preserveAspect = true;

        GameObject missionsObj = new GameObject("MissionsText");
        missionsObj.transform.SetParent(missionsAnchor, false);
        RectTransform mRt = missionsObj.AddComponent<RectTransform>();
        mRt.anchorMin = Vector2.zero;
        mRt.anchorMax = Vector2.one;
        mRt.sizeDelta = Vector2.zero;

        missionsText = missionsObj.AddComponent<Text>();
        missionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        missionsText.fontSize = 20;
        missionsText.color = Color.white;
        missionsText.alignment = TextAnchor.UpperLeft;
        missionsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        missionsText.verticalOverflow = VerticalWrapMode.Overflow;
        missionsText.supportRichText = true;
        missionsText.text = "";
    }

    // --- ЛОГИКА ---
    public void StartLevel(int level, LevelPlayMode playMode)
    {
        levelStartTime = Time.time;
        toysCollectedThisLevel = 0;
        hasUsedChannel2 = false;
        hasUsedChannel1 = false;
        hasFailed = false;
        isLevelCompleted = false;
        totalToysRequired = 1;

        activeMissions.Clear();

        activeMissions.Add(new Mission { description = "Собери все указанные игрушки", updateAction = CheckCollectAll });

        if (level == 1)
        {
            timeLimit = 50f;
            activeMissions.Add(new Mission { description = "Собери за 50 секунд", updateAction = () => CheckTime(50f) });
            activeMissions.Add(new Mission { description = "Используй только сжатие", updateAction = CheckIsolation });
        }
        else if (level == 2)
        {
            timeLimit = 70f;
            activeMissions.Add(new Mission { description = "Собери за 70 секунд", updateAction = () => CheckTime(70f) });
            activeMissions.Add(new Mission { description = "Используй только разжатие", updateAction = CheckIsolation });
        }
        else
        {
            timeLimit = 90f;
            activeMissions.Add(new Mission { description = "Собери за 90 секунд", updateAction = () => CheckTime(90f) });
            activeMissions.Add(new Mission { description = "Используй обе мышцы", updateAction = CheckDualChannels });
        }

        UpdateMissionUI();
    }

    private void CheckCollectAll()
    {
        if (toysCollectedThisLevel >= totalToysRequired)
            activeMissions[0].isCompleted = true;
    }

    private void CheckTime(float limit)
    {
        if (Time.time - levelStartTime > limit && toysCollectedThisLevel < totalToysRequired)
        {
            var m = activeMissions.Find(x => x.description.Contains("Собери за"));
            if (m != null) m.isFailed = true;
        }
    }

    private void CheckIsolation()
    {
        if (LevelManager.Instance.currentLevel == 1 && hasUsedChannel2)
        {
            var m = activeMissions.Find(x => x.description.Contains("только сжатие"));
            if (m != null) m.isFailed = true;
        }
        else if (LevelManager.Instance.currentLevel == 2 && hasUsedChannel1)
        {
            var m = activeMissions.Find(x => x.description.Contains("только разжатие"));
            if (m != null) m.isFailed = true;
        }
    }

    private void CheckDualChannels()
    {
        var m = activeMissions.Find(x => x.description.Contains("обе мышцы"));
        if (m != null && hasUsedChannel1 && hasUsedChannel2)
        {
            m.isCompleted = true;
        }
    }

    public void OnToyCollected()
    {
        toysCollectedThisLevel++;
    }

    public void OnFailedAction(bool wasWrongChannel = false)
    {
        if (wasWrongChannel)
        {
            if (LevelManager.Instance.currentLevel == 1 && !hasUsedChannel2) hasUsedChannel2 = true;
            else if (LevelManager.Instance.currentLevel == 2 && !hasUsedChannel1) hasUsedChannel1 = true;
            else
            {
                if (!hasUsedChannel1) hasUsedChannel1 = true;
                if (!hasUsedChannel2) hasUsedChannel2 = true;
            }
        }
        else hasFailed = true;
    }

    public void SetTotalToys(int count)
    {
        totalToysRequired = count;
    }

        void Update()
    {
        if (activeMissions.Count == 0) return;
        if (isLevelCompleted) return;

        // Таймер
        float timeLeft = timeLimit - (Time.time - levelStartTime);
        int timeInt = Mathf.Max(0, Mathf.CeilToInt(timeLeft));

        if (timerDigitTens != null && timerDigitOnes != null)
        {
            int tens = timeInt / 10;
            int ones = timeInt % 10;
            if (tens > 9) tens = 9;

            timerDigitTens.sprite = digitSprites[tens];
            timerDigitOnes.sprite = digitSprites[ones];
        }

        // Проверка миссий в процессе игры
        foreach (var m in activeMissions)
        {
            if (!m.isCompleted && !m.isFailed && m.updateAction != null)
                m.updateAction.Invoke();
        }

        // --- ЛОГИКА ПЕРЕХОДА ---
        // Проверяем, собраны ли все игрушки (первая миссия)
        bool mainMissionDone = activeMissions[0].isCompleted;

        if (mainMissionDone)
        {
            isLevelCompleted = true;

            // ЦЕПНАЯ РЕАКЦИЯ: если главная миссия выполнена,
            // остальные миссии становятся выполненными, ЕСЛИ они не провалены
            for (int i = 1; i < activeMissions.Count; i++)
            {
                if (!activeMissions[i].isFailed)
                {
                    activeMissions[i].isCompleted = true;
                }
            }

            UpdateMissionUI(); // Обновляем UI, чтобы закрасить зеленым

            // Проверяем, есть ли красные (проваленные) миссии
            bool anyFailed = activeMissions.Any(m => m.isFailed);

            if (!anyFailed)
            {
                Debug.Log("[MissionManager] Все миссии выполнены! Переход.");
                Invoke(nameof(TriggerLevelTransition), 1.5f);
            }
            else
            {
                Debug.Log("[MissionManager] Есть проваленные миссии! Перезапуск.");
                Invoke(nameof(RestartLevel), 2f);
            }
        }
    }

    void TriggerLevelTransition()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.StartTransitionToNextLevel();
    }

    void RestartLevel()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.RestartCurrentLevel();
    }

    void UpdateMissionUI()
    {
        if (missionsText == null) return;

        string displayText = "";
        foreach (var m in activeMissions)
        {
            string status = m.isCompleted ? "<color=green>✓</color>" : (m.isFailed ? "<color=red>✗</color>" : "•");
            string color = m.isCompleted ? "green" : (m.isFailed ? "red" : "white");
            displayText += $"{status} <color={color}>{m.description}</color>\n";
        }
        missionsText.text = displayText;
    }
}
