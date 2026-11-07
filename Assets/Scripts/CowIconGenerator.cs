using UnityEngine;

public static class CowIconGenerator
{
    /// <summary>
    /// Генерирует спрайт-иконку коровы (стилизованная голова) размером size x size.
    /// </summary>
    public static Sprite CreateCowIcon(int size = 128)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color black = new Color(0.1f, 0.1f, 0.1f);
        Color pink = new Color(1f, 0.65f, 0.75f);
        Color horn = new Color(0.95f, 0.9f, 0.7f);

        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = clear;

        float cx = size * 0.5f;
        float cy = size * 0.5f;

        void SetPx(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= size || y >= size) return;
            px[y * size + x] = c;
        }

        void FillEllipse(float ex, float ey, float rx, float ry, Color c)
        {
            int minX = Mathf.FloorToInt(ex - rx);
            int maxX = Mathf.CeilToInt(ex + rx);
            int minY = Mathf.FloorToInt(ey - ry);
            int maxY = Mathf.CeilToInt(ey + ry);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x - ex) / rx;
                    float dy = (y - ey) / ry;
                    if (dx * dx + dy * dy <= 1f)
                        SetPx(x, y, c);
                }
            }
        }

        // Рожки
        FillEllipse(cx - size * 0.28f, cy + size * 0.32f, size * 0.06f, size * 0.10f, horn);
        FillEllipse(cx + size * 0.28f, cy + size * 0.32f, size * 0.06f, size * 0.10f, horn);

        // Уши
        FillEllipse(cx - size * 0.36f, cy + size * 0.10f, size * 0.10f, size * 0.06f, white);
        FillEllipse(cx + size * 0.36f, cy + size * 0.10f, size * 0.10f, size * 0.06f, white);

        // Морда (основа)
        FillEllipse(cx, cy, size * 0.34f, size * 0.32f, white);

        // Чёрное пятно на лбу
        FillEllipse(cx - size * 0.10f, cy + size * 0.18f, size * 0.12f, size * 0.10f, black);
        // Пятно сбоку
        FillEllipse(cx + size * 0.18f, cy - size * 0.02f, size * 0.09f, size * 0.12f, black);

        // Мордочка (розовая)
        FillEllipse(cx, cy - size * 0.14f, size * 0.20f, size * 0.14f, pink);

        // Ноздри
        FillEllipse(cx - size * 0.07f, cy - size * 0.14f, size * 0.03f, size * 0.045f, black);
        FillEllipse(cx + size * 0.07f, cy - size * 0.14f, size * 0.03f, size * 0.045f, black);

        // Глаза
        FillEllipse(cx - size * 0.13f, cy + size * 0.04f, size * 0.05f, size * 0.06f, black);
        FillEllipse(cx + size * 0.13f, cy + size * 0.04f, size * 0.05f, size * 0.06f, black);
        // Блики
        SetPx(Mathf.RoundToInt(cx - size * 0.11f), Mathf.RoundToInt(cy + size * 0.06f), white);
        SetPx(Mathf.RoundToInt(cx + size * 0.15f), Mathf.RoundToInt(cy + size * 0.06f), white);

        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
}
