using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum ControlMode { DirectButtons, EMG, SPPPR }

    [Header("Текущий режим")]
    public ControlMode currentMode = ControlMode.EMG;

    // Статические значения для слайдеров
    public float Channel1SliderValue = 0f;
    public float Channel2SliderValue = 0f;

    private IControlInput activeInput;

    void Awake()
    {
        Instance = this;
        UpdateInputMode(currentMode);
    }

    public void UpdateInputMode(ControlMode newMode)
    {
        currentMode = newMode;
        switch (currentMode)
        {
            case ControlMode.EMG:
                activeInput = new EmgEmulator();
                break;
            case ControlMode.SPPPR:
                activeInput = new SppprEmulator();
                break;
            case ControlMode.DirectButtons:
            default:
                activeInput = null;
                break;
        }
    }

    public float GetChannel1()
    {
        if (activeInput != null) return activeInput.GetChannel1();
        return Channel1SliderValue; // Если нет активного, берем слайдер
    }

    public float GetChannel2()
    {
        if (activeInput != null) return activeInput.GetChannel2();
        return Channel2SliderValue;
    }

    public string GetChannel1Name() => activeInput?.GetChannel1Name() ?? "CH1";
    public string GetChannel2Name() => activeInput?.GetChannel2Name() ?? "CH2";
}
