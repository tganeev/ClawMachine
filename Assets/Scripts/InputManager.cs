using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum ControlMode { DirectButtons, EMG, SPPPR }

    [Header("Текущий режим")]
    public ControlMode currentMode = ControlMode.EMG;

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
                activeInput = null; // В этом режиме клешня использует UI кнопки (старый код)
                break;
        }
        Debug.Log($"[InputManager] Режим управления изменен на: {currentMode}");
    }

    public float GetChannel1() => activeInput?.GetChannel1() ?? 0f;
    public float GetChannel2() => activeInput?.GetChannel2() ?? 0f;

    public string GetChannel1Name() => activeInput?.GetChannel1Name() ?? "Нет";
    public string GetChannel2Name() => activeInput?.GetChannel2Name() ?? "Нет";
}
