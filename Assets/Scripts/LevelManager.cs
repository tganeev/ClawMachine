using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Настройки уровней")]
    public int currentLevel = 1;
    private int maxLevels = 3;

    private string[][] levelTargets = new string[][]
    {
        new string[] { "cow_plush_toy" },
        new string[] { "mario_soft_toy", "teddy_bear_3d_model" },
        new string[] { "labubu_classic_-_collectible_3d_figure", "viral_monkey_plush_toy", "kangaroo_kids_toy_plushie" }
    };

    [Header("UI Ссылки")]
    public Text levelText;
    public Transform iconsContainer;
    public Text bigLevelText;

    [Header("Ссылки")]
    public ClawController clawController;
    public SceneBuilder sceneBuilder;

    private Dictionary<string, bool> collectedThisLevel = new Dictionary<string, bool>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartLevel(1);
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        collectedThisLevel.Clear();

        if (clawController != null) clawController.ResetClawForNewLevel();

        if (level > maxLevels)
        {
            if (levelText != null)
            {
                levelText.text = "Победа";
                levelText.color = Color.yellow;
                levelText.alignment = TextAnchor.MiddleCenter;
                levelText.fontSize = 40;

                RectTransform lrt = levelText.GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0f, 0.5f);
                lrt.anchorMax = new Vector2(1f, 0.5f);
                lrt.offsetMin = new Vector2(0f, -40f);
                lrt.offsetMax = new Vector2(0f, 40f);
            }

            ClearIcons();
            ShowBigText("ИГРА ПРОЙДЕНА!", 999f);
            return;
        }

        if (levelText != null)
        {
            levelText.text = $"lvl: {level}";
            levelText.color = Color.white;
            levelText.alignment = TextAnchor.MiddleLeft;
            levelText.fontSize = 36;

            RectTransform lrt = levelText.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0.5f);
            lrt.anchorMax = new Vector2(0f, 0.5f);
            lrt.offsetMin = new Vector2(15f, -40f);
            lrt.offsetMax = new Vector2(135f, 40f);
        }

        ShowBigText($"Уровень {level}", 2f);

        foreach (var toyName in levelTargets[level - 1])
        {
            collectedThisLevel[toyName] = false;
        }

        if (sceneBuilder != null && clawController != null)
        {
            sceneBuilder.SpawnToys(clawController);
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.StartLevel(level);
            MissionManager.Instance.SetTotalToys(levelTargets[level - 1].Length);
        }

        UpdateIconsUI();
    }

    private void ShowBigText(string message, float duration)
    {
        if (bigLevelText != null)
        {
            bigLevelText.text = message;
            bigLevelText.gameObject.SetActive(true);
            bigLevelText.canvasRenderer.SetAlpha(1f);

            CancelInvoke(nameof(HideBigText));

            if (duration < 900f)
            {
                bigLevelText.CrossFadeAlpha(0f, duration, false);
                Invoke(nameof(HideBigText), duration);
            }
        }
    }

    private void HideBigText()
    {
        if (bigLevelText != null) bigLevelText.gameObject.SetActive(false);
    }

    public void OnToyCollected(string toyName)
    {
        if (currentLevel > maxLevels) return;

        if (collectedThisLevel.ContainsKey(toyName) && !collectedThisLevel[toyName])
        {
            collectedThisLevel[toyName] = true;
            UpdateIconsUI();

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnToyCollected();
            }

            if (collectedThisLevel.Values.All(v => v == true))
            {
                Invoke(nameof(GoToNextLevel), 3f);
            }
        }
    }

    private void GoToNextLevel()
    {
        StartLevel(currentLevel + 1);
    }

    private void UpdateIconsUI()
    {
        if (iconsContainer == null) return;

        foreach (Transform child in iconsContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentLevel > maxLevels) return;

        foreach (var toyName in levelTargets[currentLevel - 1])
        {
            GameObject iconObj = new GameObject("Icon_" + toyName);
            iconObj.transform.SetParent(iconsContainer, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 60);

            Image img = iconObj.AddComponent<Image>();
            img.preserveAspect = true;

            if (collectedThisLevel[toyName])
            {
                img.sprite = CreateGreenCheckmark(128);
            }
            else
            {
                img.sprite = GetIconForToy(toyName);
            }
        }
    }

    private void ClearIcons()
    {
        if (iconsContainer == null) return;
        foreach (Transform child in iconsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private Sprite GetIconForToy(string toyName)
    {
        switch (toyName)
        {
            case "cow_plush_toy": return CowIconGenerator.CreateCowIcon(128);
            case "mario_soft_toy": return SimpleIconGenerator.CreateMarioIcon(128);
            case "teddy_bear_3d_model": return SimpleIconGenerator.CreateBearIcon(128);
            case "labubu_classic_-_collectible_3d_figure": return SimpleIconGenerator.CreateLabubuIcon(128);
            case "viral_monkey_plush_toy": return SimpleIconGenerator.CreateMonkeyIcon(128);
            case "kangaroo_kids_toy_plushie": return SimpleIconGenerator.CreateKangarooIcon(128);
            default: return CowIconGenerator.CreateCowIcon(128);
        }
    }

    private Sprite CreateGreenCheckmark(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0);

        Color green = new Color(0.2f, 0.9f, 0.2f);

        int thickness = size / 20;
        if (thickness < 2) thickness = 2;

        Vector2 p1_start = new Vector2(size * 0.25f, size * 0.5f);
        Vector2 p1_end = new Vector2(size * 0.45f, size * 0.3f);
        Vector2 p2_start = new Vector2(size * 0.45f, size * 0.3f);
        Vector2 p2_end = new Vector2(size * 0.75f, size * 0.75f);

        DrawLine(px, size, p1_start, p1_end, thickness, green);
        DrawLine(px, size, p2_start, p2_end, thickness, green);

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void DrawLine(Color[] px, int size, Vector2 start, Vector2 end, int thickness, Color color)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance * 2);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            for (int x = -thickness; x <= thickness; x++)
            {
                for (int y = -thickness; y <= thickness; y++)
                {
                    int pxX = Mathf.RoundToInt(point.x) + x;
                    int pxY = Mathf.RoundToInt(point.y) + y;

                    if (pxX >= 0 && pxX < size && pxY >= 0 && pxY < size)
                    {
                        px[pxY * size + pxX] = color;
                    }
                }
            }
        }
    }
}
