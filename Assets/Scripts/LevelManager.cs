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

    [Header("Ссылки на игрушки")]
    public ClawController clawController;

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

        if (level > maxLevels)
        {
            if (levelText != null) levelText.text = "ИГРА ПРОЙДЕНА!";
            ClearIcons();
            return;
        }

        if (levelText != null) levelText.text = $"УРОВЕНЬ {level}";

        foreach (var toyName in levelTargets[level - 1])
        {
            collectedThisLevel[toyName] = false;
        }

        UpdateIconsUI();
        Debug.Log($"[LevelManager] Начался уровень {level}. Нужно достать: {string.Join(", ", levelTargets[level - 1])}");
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
                Invoke(nameof(GoToNextLevel), 2f);
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

        // Создаем новые иконки
        foreach (var toyName in levelTargets[currentLevel - 1])
        {
            GameObject iconObj = new GameObject("Icon_" + toyName);
            iconObj.transform.SetParent(iconsContainer, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(65, 65);

            Image img = iconObj.AddComponent<Image>();
            img.preserveAspect = true;

            img.sprite = GetIconForToy(toyName);

            // Если собрана — делаем ее полупрозрачной и зеленоватой
            if (collectedThisLevel[toyName])
            {
                img.color = new Color(0.5f, 1f, 0.5f, 0.6f);
            }
            else
            {
                img.color = Color.white;
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

    // Вызов качественных иконок
    private Sprite GetIconForToy(string toyName)
    {
        switch (toyName)
        {
            case "cow_plush_toy":
                return CowIconGenerator.CreateCowIcon(128);
            case "mario_soft_toy":
                return SimpleIconGenerator.CreateMarioIcon(128);
            case "teddy_bear_3d_model":
                return SimpleIconGenerator.CreateBearIcon(128);
            case "labubu_classic_-_collectible_3d_figure":
                return SimpleIconGenerator.CreateLabubuIcon(128);
            case "viral_monkey_plush_toy":
                return SimpleIconGenerator.CreateMonkeyIcon(128);
            case "kangaroo_kids_toy_plushie":
                return SimpleIconGenerator.CreateKangarooIcon(128);
            default:
                return CowIconGenerator.CreateCowIcon(128);
        }
    }
}
