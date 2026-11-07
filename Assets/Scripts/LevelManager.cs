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

    // Какие игрушки нужно вытащить на каждом уровне
    private string[][] levelTargets = new string[][]
    {
        new string[] { "cow_plush_toy" },                                  // Уровень 1
        new string[] { "mario_soft_toy", "teddy_bear_3d_model" },          // Уровень 2
        new string[] { "labubu_classic_-_collectible_3d_figure", "viral_monkey_plush_toy", "kangaroo_kids_toy_plushie" } // Уровень 3
    };

    [Header("UI Ссылки")]
    public Text levelText;
    public Transform iconsContainer;
    public Text bigLevelText; // Ссылка на текст по центру экрана

    [Header("Ссылки")]
    public ClawController clawController;
    public SceneBuilder sceneBuilder; // Ссылка на сборщик для респавна

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
            if (levelText != null) levelText.text = "ИГРА ПРОЙДЕНА!";
            ClearIcons();
            ShowBigText("ИГРА ПРОЙДЕНА!", 999f); // Оставляем навсегда (999 секунд)
            return;
        }

        if (levelText != null) levelText.text = $"УРОВЕНЬ {level}";

        // Показываем большую надпись по центру (исчезнет через 2 секунды)
        ShowBigText($"УРОВЕНЬ {level}", 2f);

        foreach (var toyName in levelTargets[level - 1])
        {
            collectedThisLevel[toyName] = false;
        }

        // Восстанавливаем все игрушки в автомате
        if (sceneBuilder != null && clawController != null)
        {
            sceneBuilder.SpawnToys(clawController);
        }

        UpdateIconsUI();
        Debug.Log($"[LevelManager] Начался уровень {level}.");
    }

    // Метод для отображения большой надписи (duration - сколько секунд висит)
    private void ShowBigText(string message, float duration)
    {
        if (bigLevelText != null)
        {
            bigLevelText.text = message;
            bigLevelText.gameObject.SetActive(true);
            bigLevelText.canvasRenderer.SetAlpha(1f); // Делаем видимым

            CancelInvoke(nameof(HideBigText));

            if (duration < 900f) // Если это не финальная надпись, плавно скрываем
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
            Debug.Log($"[LevelManager] Собрана нужная игрушка: {toyName}");

            UpdateIconsUI();

            if (collectedThisLevel.Values.All(v => v == true))
            {
                Debug.Log($"[LevelManager] Уровень {currentLevel} пройден!");
                Invoke(nameof(GoToNextLevel), 3f); // Ждем 3 секунды перед сменой уровня
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

        // Удаляем старые иконки
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
            rt.sizeDelta = new Vector2(65, 65);

            Image img = iconObj.AddComponent<Image>();
            img.preserveAspect = true;

            if (collectedThisLevel[toyName])
            {
                // Если собрана — ставим вместо животного ЗЕЛЕНУЮ ГАЛОЧКУ
                img.sprite = CreateGreenCheckmark(128);
            }
            else
            {
                // Если не собрана — рисуем животное
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

    // Процедурный генератор зеленой галочки (чтобы не зависеть от шрифтов)
        private Sprite CreateGreenCheckmark(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0);

        Color green = new Color(0.2f, 0.9f, 0.2f); // Ярко-зеленый

        // Рисуем две тонкие линии (галочку) через попиксельную отрисовку
        int thickness = size / 20; // Тонкая линия (около 6 пикселей для 128)
        if (thickness < 2) thickness = 2;

        // Линия 1 (короткая, идет вниз справа налево)
        Vector2 p1_start = new Vector2(size * 0.25f, size * 0.5f);
        Vector2 p1_end = new Vector2(size * 0.45f, size * 0.3f);

        // Линия 2 (длинная, идет вверх слева направо)
        Vector2 p2_start = new Vector2(size * 0.45f, size * 0.3f);
        Vector2 p2_end = new Vector2(size * 0.75f, size * 0.75f);

        DrawLine(px, size, p1_start, p1_end, thickness, green);
        DrawLine(px, size, p2_start, p2_end, thickness, green);

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // Вспомогательный метод для рисования линии
    private void DrawLine(Color[] px, int size, Vector2 start, Vector2 end, int thickness, Color color)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance * 2); // Чем больше шагов, тем ровнее линия

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            // Рисуем квадратик вокруг точки (толщина)
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
