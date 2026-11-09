using UnityEngine;

public class EmgEmulator : IControlInput
{
    public float GetChannel1()
    {
        // Если слайдер двигали — берем его значение
        return InputManager.Instance.Channel1SliderValue;
    }

    public float GetChannel2()
    {
        return InputManager.Instance.Channel2SliderValue;
    }

    public string GetChannel1Name() => "Канал 1 (Сжать)";
    public string GetChannel2Name() => "Канал 2 (Разжать)";
}
