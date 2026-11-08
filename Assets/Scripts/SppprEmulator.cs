using UnityEngine;

public class SppprEmulator : IControlInput
{
    private float channel1 = 0f;
    private float channel2 = 0f;
    private float smoothing = 10f; // Сглаживание (имитация Softmax)

    public float GetChannel1()
    {
        // Имитация джойстика: W/S или стрелки
        float input = Input.GetAxisRaw("Vertical");
        // Если тянем вверх — сжимаем (канал 1), вниз — разжимаем (канал 2)
        float target1 = Mathf.Clamp01(input);
        channel1 = Mathf.Lerp(channel1, target1, Time.deltaTime * smoothing);
        return channel1;
    }

    public float GetChannel2()
    {
        float input = Input.GetAxisRaw("Vertical");
        // Если тянем вниз — активируем разжимание
        float target2 = Mathf.Clamp01(-input);
        channel2 = Mathf.Lerp(channel2, target2, Time.deltaTime * smoothing);
        return channel2;
    }

    public string GetChannel1Name() => "Flexion (Вверх)";
    public string GetChannel2Name() => "Extension (Вниз)";
}
