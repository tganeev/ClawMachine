public interface IControlInput
{
    // Возвращает значение от 0 до 1 для первого канала (Сгибание)
    float GetChannel1();
    // Возвращает значение от 0 до 1 для второго канала (Разгибание)
    float GetChannel2();

    // Названия каналов для UI
    string GetChannel1Name();
    string GetChannel2Name();
}
