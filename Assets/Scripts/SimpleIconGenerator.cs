using UnityEngine;

public static class SimpleIconGenerator
{
    // Вспомогательный метод для создания прозрачной текстуры
    private static Color[] CreateTransparentCanvas(int size)
    {
        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++)
            px[i] = new Color(0, 0, 0, 0);
        return px;
    }

    // Рисование эллипса
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

    // Рисование прямоугольника
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

    // Рисование линии (для галочек и деталей)
    private static void DrawLine(Color[] px, int size, Vector2 start, Vector2 end, int thickness, Color color)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance * 2);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            for (int x = -thickness; x <= thickness; x++)
            {
                for (int y = -thickness; y <= thickness; y++)
                {
                    int pxX = Mathf.RoundToInt(point.x) + x;
                    int pxY = Mathf.RoundToInt(point.y) + y;

                    if (pxX >= 0 && pxX < size && pxY >= 0 && pxY < size)
                    {
                        px[pxY * size + pxX] = color;
                    }
                }
            }
        }
    }

    // Создание спрайта из массива пикселей
    private static Sprite CreateSprite(Color[] px, int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // --- ИКОНКА МАРИО (Цветная, как на скриншоте) ---
    public static Sprite CreateMarioIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f;
        float cy = size * 0.45f;

        // Цвета
        Color skin = new Color(0.98f, 0.82f, 0.65f); // Телесный
        Color red = new Color(0.85f, 0.1f, 0.1f);    // Красный
        Color darkRed = new Color(0.6f, 0.05f, 0.05f); // Темно-красный (для козырька)
        Color white = Color.white;
        Color black = Color.black;
        Color brown = new Color(0.5f, 0.25f, 0.1f); // Коричневый для бакенбард

        // 1. Уши (телесные)
        DrawEllipse(px, size, cx - size * 0.32f, cy - size * 0.05f, size * 0.10f, size * 0.12f, skin);
        DrawEllipse(px, size, cx + size * 0.32f, cy - size * 0.05f, size * 0.10f, size * 0.12f, skin);

        // 2. Голова (широкий овал)
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.30f, size * 0.32f, skin);

        // 3. Бакенбарды (коричневые)
        DrawEllipse(px, size, cx - size * 0.22f, cy - size * 0.1f, size * 0.06f, size * 0.15f, brown);
        DrawEllipse(px, size, cx + size * 0.22f, cy - size * 0.1f, size * 0.06f, size * 0.15f, brown);

        // 4. Кепка (красная)
        DrawEllipse(px, size, cx, cy + size * 0.22f, size * 0.36f, size * 0.24f, red);
        // Козырек кепки (рисуем овал и стираем нижнюю часть)
        DrawEllipse(px, size, cx, cy + size * 0.12f, size * 0.38f, size * 0.10f, darkRed);
        DrawRect(px, size, (int)(cx - size * 0.4f), (int)(cy + size * 0.02f), (int)(size * 0.8f), (int)(size * 0.12f), new Color(0,0,0,0)); // Стираем нижнюю половину козырька
        // Перекрываем лицо там, где был стерт козырек
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.30f, size * 0.32f, skin);

        // 5. Буква "M" на кепке (белая, рисуем толстыми линиями)
        int mThick = (int)(size * 0.04f);
        Vector2 mCenter = new Vector2(cx, cy + size * 0.28f);
        Vector2 mLeftBase = new Vector2(mCenter.x - size * 0.1f, mCenter.y - size * 0.08f);
        Vector2 mRightBase = new Vector2(mCenter.x + size * 0.1f, mCenter.y - size * 0.08f);
        Vector2 mTopLeft = new Vector2(mCenter.x - size * 0.1f, mCenter.y + size * 0.08f);
        Vector2 mTopRight = new Vector2(mCenter.x + size * 0.1f, mCenter.y + size * 0.08f);
        Vector2 mMidBottom = new Vector2(mCenter.x, mCenter.y - size * 0.02f);

        DrawLine(px, size, mLeftBase, mTopLeft, mThick, white); // Левая палка
        DrawLine(px, size, mRightBase, mTopRight, mThick, white); // Правая палка
        DrawLine(px, size, mTopLeft, mMidBottom, mThick, white); // Левая диагональ
        DrawLine(px, size, mTopRight, mMidBottom, mThick, white); // Правая диагональ

        // 6. Глаза (большие белые овалы)
        DrawEllipse(px, size, cx - size * 0.10f, cy + size * 0.02f, size * 0.08f, size * 0.12f, white);
        DrawEllipse(px, size, cx + size * 0.10f, cy + size * 0.02f, size * 0.08f, size * 0.12f, white);

        // 7. Зрачки (черные овалы, сдвинуты вниз и к центру)
        DrawEllipse(px, size, cx - size * 0.08f, cy - size * 0.01f, size * 0.04f, size * 0.06f, black);
        DrawEllipse(px, size, cx + size * 0.08f, cy - size * 0.01f, size * 0.04f, size * 0.06f, black);

        // 8. Брови (черные)
        DrawEllipse(px, size, cx - size * 0.10f, cy + size * 0.16f, size * 0.07f, size * 0.03f, black);
        DrawEllipse(px, size, cx + size * 0.10f, cy + size * 0.16f, size * 0.07f, size * 0.03f, black);

        // 9. Нос (телесный, большой)
        DrawEllipse(px, size, cx, cy - size * 0.08f, size * 0.12f, size * 0.10f, skin);

        // 10. Усы (черные, толстые)
        DrawEllipse(px, size, cx - size * 0.10f, cy - size * 0.18f, size * 0.13f, size * 0.07f, black);
        DrawEllipse(px, size, cx + size * 0.10f, cy - size * 0.18f, size * 0.13f, size * 0.07f, black);
        // Закрываем центр усов (под носом), чтобы они не сливались в один овал
        DrawEllipse(px, size, cx, cy - size * 0.22f, size * 0.05f, size * 0.04f, skin);

        return CreateSprite(px, size);
    }

    // --- ОСТАЛЬНЫЕ ИКОНКИ ---

    public static Sprite CreateBearIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color brown = new Color(0.6f, 0.4f, 0.2f);
        Color lightBrown = new Color(0.8f, 0.6f, 0.4f);

        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.12f, size * 0.12f, brown);
        DrawEllipse(px, size, cx, cy, size * 0.35f, size * 0.32f, brown);
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.15f, size * 0.12f, lightBrown);
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.05f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx - size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.12f, cy + size * 0.1f, size * 0.04f, size * 0.04f, Color.black);

        return CreateSprite(px, size);
    }

    public static Sprite CreateLabubuIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color pink = new Color(1f, 0.7f, 0.8f);

        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.1f, size * 0.25f, pink);
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.1f, size * 0.25f, pink);
        DrawEllipse(px, size, cx - size * 0.25f, cy + size * 0.25f, size * 0.05f, size * 0.15f, new Color(1f, 0.5f, 0.6f));
        DrawEllipse(px, size, cx + size * 0.25f, cy + size * 0.25f, size * 0.05f, size * 0.15f, new Color(1f, 0.5f, 0.6f));
        DrawEllipse(px, size, cx, cy, size * 0.25f, size * 0.22f, pink);
        DrawEllipse(px, size, cx - size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.05f, Color.black);
        DrawEllipse(px, size, cx + size * 0.1f, cy + size * 0.05f, size * 0.04f, size * 0.05f, Color.black);
        DrawRect(px, size, (int)(cx - size * 0.06f), (int)(cy - size * 0.1f), (int)(size * 0.12f), (int)(size * 0.05f), Color.white);

        return CreateSprite(px, size);
    }

    public static Sprite CreateMonkeyIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color darkBrown = new Color(0.4f, 0.2f, 0.1f);
        Color lightFace = new Color(0.9f, 0.7f, 0.5f);

        DrawEllipse(px, size, cx - size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);
        DrawEllipse(px, size, cx + size * 0.3f, cy, size * 0.12f, size * 0.15f, darkBrown);
        DrawEllipse(px, size, cx, cy, size * 0.25f, size * 0.25f, darkBrown);
        DrawEllipse(px, size, cx, cy - size * 0.05f, size * 0.18f, size * 0.18f, lightFace);
        DrawEllipse(px, size, cx - size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx, cy - size * 0.12f, size * 0.08f, size * 0.03f, Color.black);

        return CreateSprite(px, size);
    }

    public static Sprite CreateKangarooIcon(int size)
    {
        Color[] px = CreateTransparentCanvas(size);
        float cx = size * 0.5f, cy = size * 0.45f;
        Color tan = new Color(0.8f, 0.6f, 0.4f);
        Color dark = new Color(0.5f, 0.3f, 0.2f);

        DrawEllipse(px, size, cx - size * 0.15f, cy + size * 0.3f, size * 0.05f, size * 0.18f, tan);
        DrawEllipse(px, size, cx + size * 0.15f, cy + size * 0.3f, size * 0.05f, size * 0.18f, tan);
        DrawEllipse(px, size, cx, cy, size * 0.2f, size * 0.25f, tan);
        DrawEllipse(px, size, cx, cy - size * 0.1f, size * 0.12f, size * 0.1f, dark);
        DrawEllipse(px, size, cx - size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx + size * 0.08f, cy + size * 0.05f, size * 0.03f, size * 0.04f, Color.black);
        DrawEllipse(px, size, cx, cy - size * 0.15f, size * 0.04f, size * 0.03f, Color.black);

        return CreateSprite(px, size);
    }
}
