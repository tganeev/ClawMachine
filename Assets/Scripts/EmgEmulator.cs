using UnityEngine;

public class EmgEmulator : IControlInput
{
    private float channel1 = 0f;
    private float channel2 = 0f;

    // Настройки эмуляции (имитация нажатия мышцы)
    private float attackSpeed = 3f;  // Как быстро нарастает сигнал
    private float releaseSpeed = 5f; // Как быстро спадает

    public float GetChannel1()
    {
        // Эмуляция левой мышцы (Сгибатель)
        if (Input.GetKey(KeyCode.A)) channel1 = Mathf.MoveTowards(channel1, 1f, attackSpeed * Time.deltaTime);
        else channel1 = Mathf.MoveTowards(channel1, 0f, releaseSpeed * Time.deltaTime);
        return channel1;
    }

    public float GetChannel2()
    {
        // Эмуляция правой мышцы (Разгибатель)
        if (Input.GetKey(KeyCode.D)) channel2 = Mathf.MoveTowards(channel2, 1f, attackSpeed * Time.deltaTime);
        else channel2 = Mathf.MoveTowards(channel2, 0f, releaseSpeed * Time.deltaTime);
        return channel2;
    }

    public string GetChannel1Name() => "Канал 1 (Сжать)";
    public string GetChannel2Name() => "Канал 2 (Разжать)";
}
