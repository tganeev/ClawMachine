using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private ClawController clawController;
    private Text taskText;
    private int currentStep = 0;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private GameObject grabbedToyCheck;

    public void Initialize(ClawController controller, Transform parentUI)
    {
        clawController = controller;

        // Создаем UI для заданий (по центру, чуть выше середины)
        GameObject textObj = new GameObject("TutorialTaskText");
        textObj.transform.SetParent(parentUI, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 400); // Выше центра
        rt.sizeDelta = new Vector2(900, 150);

        taskText = textObj.AddComponent<Text>();
        taskText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        taskText.fontSize = 48;
        taskText.color = Color.yellow;
        taskText.alignment = TextAnchor.MiddleCenter;
        taskText.fontStyle = FontStyle.Bold;

        ShowStep(0);
    }

    void Update()
    {
        if (clawController == null) return;

        float ch1 = InputManager.Instance != null ? InputManager.Instance.GetChannel1() : Input.GetKey(KeyCode.A) ? 1f : 0f;
        float ch2 = InputManager.Instance != null ? InputManager.Instance.GetChannel2() : Input.GetKey(KeyCode.D) ? 1f : 0f;

        switch (currentStep)
        {
            case 0: // Активируй канал 1
                if (ch1 > 0.8f) NextStep();
                break;

            case 1: // Активируй канал 2
                if (ch2 > 0.8f) NextStep();
                break;

            case 2: // Захвати игрушку
                if (clawController.IsHoldingToy()) NextStep();
                break;

            case 3: // Удерживай захват 3 секунды
                if (clawController.IsHoldingToy() && ch1 > 0.5f)
                {
                    if (!isHolding)
                    {
                        isHolding = true;
                        holdTimer = 0f;
                    }
                    holdTimer += Time.deltaTime;

                    taskText.text = $"Удерживай захват: {Mathf.Max(0, 3f - holdTimer):F1} сек";

                    if (holdTimer >= 3f)
                    {
                        NextStep();
                    }
                }
                else if (isHolding && (!clawController.IsHoldingToy() || ch1 <= 0.5f))
                {
                    // Провал — заново
                    isHolding = false;
                    holdTimer = 0f;
                    taskText.text = "Ой! Ты отпустил игрушку. Попробуй еще раз!";
                    Invoke(nameof(RestartStep), 2f);
                }
                break;
        }
    }

    void NextStep()
    {
        currentStep++;
        isHolding = false;
        holdTimer = 0f;

        if (currentStep > 3)
        {
            FinishTutorial();
        }
        else
        {
            ShowStep(currentStep);
        }
    }

    void RestartStep()
    {
        ShowStep(currentStep);
    }

    void ShowStep(int step)
    {
        switch (step)
        {
            case 0:
                taskText.text = "Шаг 1: Активируй ПЕРВЫЙ канал (Сжать) до 100%";
                break;
            case 1:
                taskText.text = "Шаг 2: Активируй ВТОРОЙ канал (Разжать) до 100%";
                break;
            case 2:
                taskText.text = "Шаг 3: Подцепи любую игрушку клешнёй";
                break;
            case 3:
                taskText.text = "Шаг 4: Удерживай захват 3 секунды";
                break;
        }
    }

    void FinishTutorial()
    {
        taskText.text = "Обучение завершено! Ты готов покорять автомат!";
        taskText.color = Color.green;

        Invoke(nameof(ReturnToMenu), 3f);
    }

    void ReturnToMenu()
    {
        // Возвращаемся в меню
        SceneBuilder builder = FindFirstObjectByType<SceneBuilder>();
        if (builder != null)
        {
            builder.RebuildToMenu();
        }
    }
}
