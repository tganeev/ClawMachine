using UnityEngine;

public static class SevenSegmentDisplay
{
    public static Font CreateFont(int size = 128)
    {
        // Создаем текстуру-атлас, где будут храниться все цифры
        // 10 цифр по горизонтали
        Texture2D tex = new Texture2D(size * 10, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color[] px = new Color[tex.width * tex.height];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0); // Прозрачный фон

        // Координаты для отрисовки цифр
        for (int i = 0; i < 10; i++)
        {
            DrawDigit(px, tex.width, i * size, 0, size, i);
        }

        tex.SetPixels(px);
        tex.Apply();

        // Создаем шрифт
        Font font = Font.CreateDynamicFontFromOSFont("Arial", size); // Временный шрифт
        // К сожалению, Unity не позволяет легко создавать кастомные шрифты из текстуры в рантайме.
        // Поэтому мы пойдем другим путем: создадим спрайты и будем менять их на UI.
        return null;
    }

    // Вспомогательный метод для рисования прямоугольника
    private static void DrawRect(Color[] px, int width, int startX, int startY, int w, int h, Color color)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int pxX = startX + x;
                int pxY = startY + y;
                if (pxX >= 0 && pxX < width && pxY >= 0 && pxY < px.Length / width)
                {
                    px[pxY * width + pxX] = color;
                }
            }
        }
    }

    // Рисуем цифру (упрощенный семисегментный вид)
    private static void DrawDigit(Color[] px, int texWidth, int offsetX, int offsetY, int size, int digit)
    {
        Color red = new Color(1f, 0.1f, 0.1f); // Ярко-красный

        int thick = size / 8; // Толщина линий
        int margin = size / 10;
        int w = size - margin * 2;
        int h = size - margin * 2;

        // Сегменты: A (верх), B (верх-право), C (низ-право), D (низ), E (низ-лево), F (верх-лево), G (центр)
        bool[] seg = new bool[7];

        switch (digit)
        {
            case 0: seg = new bool[] { true, true, true, true, true, true, false }; break;
            case 1: seg = new bool[] { false, true, true, false, false, false, false }; break;
            case 2: seg = new bool[] { true, true, false, true, true, false, true }; break;
            case 3: seg = new bool[] { true, true, true, true, false, false, true }; break;
            case 4: seg = new bool[] { false, true, true, false, false, true, true }; break;
            case 5: seg = new bool[] { true, false, true, true, false, true, true }; break;
            case 6: seg = new bool[] { true, false, true, true, true, true, true }; break;
            case 7: seg = new bool[] { true, true, true, false, false, false, false }; break;
            case 8: seg = new bool[] { true, true, true, true, true, true, true }; break;
            case 9: seg = new bool[] { true, true, true, true, false, true, true }; break;
        }

        // Рисуем сегменты
        if (seg[0]) DrawRect(px, texWidth, offsetX + margin + thick, offsetY + margin + h - thick, w - thick * 2, thick, red); // Верх
        if (seg[1]) DrawRect(px, texWidth, offsetX + margin + w - thick, offsetY + margin + h / 2, thick, h / 2 - thick, red); // Верх-право
        if (seg[2]) DrawRect(px, texWidth, offsetX + margin + w - thick, offsetY + margin, thick, h / 2 - thick, red); // Низ-право
        if (seg[3]) DrawRect(px, texWidth, offsetX + margin + thick, offsetY + margin, w - thick * 2, thick, red); // Низ
        if (seg[4]) DrawRect(px, texWidth, offsetX + margin, offsetY + margin, thick, h / 2 - thick, red); // Низ-лево
        if (seg[5]) DrawRect(px, texWidth, offsetX + margin, offsetY + margin + h / 2, thick, h / 2 - thick, red); // Верх-лево
        if (seg[6]) DrawRect(px, texWidth, offsetX + margin + thick, offsetY + margin + h / 2 - thick / 2, w - thick * 2, thick, red); // Центр
    }
}
