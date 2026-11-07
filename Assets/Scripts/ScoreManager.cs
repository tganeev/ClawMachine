using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    public Text scoreText;

    private int score = 0;

    void Awake()
    {
        Instance = this;
        UpdateScoreDisplay();
    }

    public void AddPoint()
    {
        score++;
        UpdateScoreDisplay();
        Debug.Log("[ScoreManager] Очко добавлено! Счет: " + score);
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            // Теперь слово "СЧЕТ:" не пропадает при обновлении
            scoreText.text = "СЧЕТ: " + score;
        }
    }
}
