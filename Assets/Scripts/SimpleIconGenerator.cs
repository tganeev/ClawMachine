using UnityEngine;

public static class SimpleIconGenerator
{
    // --- Вспомогательные методы рисования ---
    private static void DrawEllipse(Color[] px, int size, float cx, float cy, float rx, float ry, Color color)
    {
        int minX = Mathf.FloorToInt(cx - rx);
        int maxX = Mathf.CeilToInt(cx + rx);
        int minY = Mathf.FloorToInt(cy - ry);
        int maxY = Mathf.CeilToInt(cy + ry);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (x < 0 || y < 0 || x >= size || y >= size) continue;
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1.0f)
                    px[y * size + x] = color;
            }
        }
    }

    private static void DrawRect(Color[] px, int size, int x, int y, int w, int h, Color color)
    {
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                int pxX = x + i;
                int pxY = y + j;
                if (pxX >= 0 && pxY >= 0 && pxX < size && pxY < size)
                    px[pxY * size + pxX] = color;
            }
    }

    private static Sprite CreateSprite(Color[] px, int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // --- Иконки ---

    public static Sprite CreateMarioIcon(int size)
    {
        Color[] px = new Color[size * size];
        float cx = size * 0.5f, cy = size * 0.45f;

        // Кепка (красная)
        DrawEllipse(px, size, cx, cy + size * 0.25f, size * 0.35f, size * 0.15f, Color.red);
        DrawRect(px, size, (int)(cx - size * 0.3f), (int)(cy + size * 0.15f), (int)(size * 0.6f), (int)(size * 0.1f), Color.red);

        // Лицо (телесное)
        DrawEllipse(px, size, cx, cy, size * 0.28f, size * 0.25f, new Color(1f, 0.8f, 0.6f));

        // Усы (коричневые)
        DrawRect(px, size, (int)(cx - size * 0.15f), (int)(cy - size * 0.05f), (int)(size * 0.3f), (int)(size * 0.06f), new Color(0.4f, 0.2f, 0.1f));

        // Глаза (черные)
        DrawEllipse(px, size, cx - size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.06f, Color.black);
        DrawEllipse(px, size, cx + size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.06f, Color.black);

        return CreateSprite(px, size);
    }

    public static Sprite CreateBearIcon(int size)
    {
        Color[] px = new Color[size * size];
        float cx = size * 0.5f, cy = size * 0.45f;
        Color brown = new Color(0.6f, 0.4f, 0.2f);
        Color lightBrown = new Color(0.8f, 0.6f, 0.4f);

        // Уши (круглые)
        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);

        // Голова
        DrawEllipse(px, size, cx, cy, size * 0.35f, size * 0.32f, brown);

        // Мордочка
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.15f, size * 0.12f, lightBrown);

        // Нос
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.05f, size * 0.04f, Color.black);

        // Глаза
        DrawEllipse(px, size, cx - size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);

        return CreateSprite(px, size);
    }

    public static Sprite CreateLabubuIcon(int size)
    {
        Color[] px = new Color[size * size];
        float cx = size * 0.5f, cy = size * 0.45f;
        Color pink = new Color(1f, 0.7f, 0.8f);

        // Огромные уши
        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.1f, size * 0.25f, pink);
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.1f, size * 0.25f, pink);

        // Внутренняя часть ушей
        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.05f, size * 0.15f, new Color(1f, 0.5f, 0.6f));
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.05f, size * 0.15f, new Color(1f, 0.5f, 0.6f));

        // Голова
        DrawEllipse(px, size, cx, cy, size * 0.25f, size * 0.22f, pink);

        // Глаза
        DrawEllipse(px, size, cx - size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.05f, Color.black);
        DrawEllipse(px, size, cx + size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.05f, Color.black);

        // Зубы
        DrawRect(px, size, (int)(cx - size * 0.06f), (int)(cy - size * 0.1f), (int)(size * 0.12f), (int)(size * 0.05f), Color.white);

        return CreateSprite(px, size);
    }

    public static Sprite CreateMonkeyIcon(int size)
    {
        Color[] px = new Color[size * size];
        float cx = size * 0.5f, cy = size * 0.45f;
        Color darkBrown = new Color(0.4f, 0.2f, 0.1f);
        Color lightFace = new Color(0.9f, 0.7f, 0.5f);

        // Уши
        DrawEllipse(px, size, cx - size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);
        DrawEllipse(px, size, cx + size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);

        // Голова
        DrawEllipse(px, size, cx, cy, size * 0.25f, size * 0.25f, darkBrown);

        // Лицо (светлое)
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.18f, size * 0.18f, lightFace);

        // Глаза
        DrawEllipse(px, size, cx - size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);

        // Улыбка
        DrawEllipse(px, size, cx, cy - size * 0.12f, size * 0.08f, size * 0.03f, Color.black);

        return CreateSprite(px, size);
    }

    public static Sprite CreateKangarooIcon(int size)
    {
        Color[] px = new Color[size * size];
        float cx = size * 0.5f, cy = size * 0.45f;
        Color tan = new Color(0.8f, 0.6f, 0.4f);
        Color dark = new Color(0.5f, 0.3f, 0.2f);

        // Длинные уши
        DrawEllipse(px, size, cx - size * 0.15f, cy + size * 0.3f, size * 0.05f, size * 0.18f, tan);
        DrawEllipse(px, size, cx + size * 0.15f, cy + size * 0.3f, size * 0.05f, size * 0.18f, tan);

        // Голова (вытянутая)
        DrawEllipse(px, size, cx, cy, size * 0.2f, size * 0.25f, tan);

        // Мордочка
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.12f, size * 0.1f, dark);

        // Глаза
        DrawEllipse(px, size, cx - size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);

        // Нос
        DrawEllipse(px, size, cx, cy - size * 0.15f, size * 0.04f, size * 0.03f, Color.black);

        return CreateSprite(px, size);
    }
}
